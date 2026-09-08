using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;

namespace RSBot.Trivia;

public class TriviaEntry
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class TriviaManager
{
    private static TriviaManager _instance;
    public static TriviaManager Instance => _instance ??= new TriviaManager();

    public bool Enabled { get; set; } = true;
    public string QuestionPattern { get; set; } = @"\[Trivia\]\s*(.+)";
    public string AnswerPattern { get; set; } = @"\[Trivia\]\s*The answer was:\s*(.+)";
    public string ReplyTo { get; set; } = string.Empty;

    public List<TriviaEntry> Entries { get; } = new();
    public string LastQuestion { get; private set; } = string.Empty;
    public string StatusText { get; private set; } = "Idle";

    public event System.Action OnStateChanged;
    public event System.Action OnDatabaseChanged;

    private static string DatabasePath => Path.Combine(Application.StartupPath, "Data", "Trivia.json");

    private TriviaManager()
    {
        LoadConfig();
        LoadDatabase();
    }

    public void LoadConfig()
    {
        Enabled = PlayerConfig.Get("SonicBot.Trivia.Enabled", true);
        QuestionPattern = PlayerConfig.Get("SonicBot.Trivia.QuestionPattern", @"\[Trivia\]\s*(.+)");
        AnswerPattern = PlayerConfig.Get("SonicBot.Trivia.AnswerPattern", @"\[Trivia\]\s*The answer was:\s*(.+)");
        ReplyTo = PlayerConfig.Get("SonicBot.Trivia.ReplyTo", string.Empty);
    }

    public void SaveConfig()
    {
        PlayerConfig.Set("SonicBot.Trivia.Enabled", Enabled);
        PlayerConfig.Set("SonicBot.Trivia.QuestionPattern", QuestionPattern);
        PlayerConfig.Set("SonicBot.Trivia.AnswerPattern", AnswerPattern);
        PlayerConfig.Set("SonicBot.Trivia.ReplyTo", ReplyTo);
    }

    public void LoadDatabase()
    {
        try
        {
            Entries.Clear();
            if (File.Exists(DatabasePath))
            {
                var json = File.ReadAllText(DatabasePath);
                var list = JsonSerializer.Deserialize<List<TriviaEntry>>(json);
                if (list != null)
                    Entries.AddRange(list);
            }
            OnDatabaseChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Debug($"[Trivia] LoadDatabase error: {ex.Message}");
        }
    }

    public void SaveDatabase()
    {
        try
        {
            var dir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Entries, options);
            File.WriteAllText(DatabasePath, json);
            OnDatabaseChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Debug($"[Trivia] SaveDatabase error: {ex.Message}");
        }
    }

    public void AddOrUpdate(string question, string answer)
    {
        if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            return;

        var q = question.Trim();
        var a = answer.Trim();

        var existing = Entries.FirstOrDefault(e => string.Equals(e.Question, q, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Answer = a;
        }
        else
        {
            Entries.Add(new TriviaEntry { Question = q, Answer = a });
        }

        SaveDatabase();
    }

    public void Remove(string question)
    {
        var existing = Entries.FirstOrDefault(e => string.Equals(e.Question, question.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            Entries.Remove(existing);
            SaveDatabase();
        }
    }

    public string FindAnswer(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return null;

        var q = question.Trim();

        // Exact match first
        var found = Entries.FirstOrDefault(e => string.Equals(e.Question, q, StringComparison.OrdinalIgnoreCase));
        if (found != null)
            return found.Answer;

        // Substring / fuzzy match
        found = Entries.FirstOrDefault(e =>
            q.Contains(e.Question, StringComparison.OrdinalIgnoreCase) ||
            e.Question.Contains(q, StringComparison.OrdinalIgnoreCase));

        return found?.Answer;
    }

    public void OnMessageReceived(string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(message))
            return;

        // Check question pattern
        if (!string.IsNullOrWhiteSpace(QuestionPattern))
        {
            try
            {
                var match = Regex.Match(message, QuestionPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var question = match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : message.Trim();
                    StatusText = $"Question detected: {question}";
                    OnStateChanged?.Invoke();

                    var answer = FindAnswer(question);
                    if (!string.IsNullOrEmpty(answer))
                    {
                        SendAnswer(answer);
                        StatusText = $"Answer sent: {answer}";
                        OnStateChanged?.Invoke();
                    }
                    else
                    {
                        LastQuestion = question;
                        StatusText = $"Answer unknown for: {question}";
                        OnStateChanged?.Invoke();
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"[Trivia] Question regex error: {ex.Message}");
            }
        }

        // Check answer pattern (learn answer for last question)
        if (!string.IsNullOrWhiteSpace(AnswerPattern))
        {
            try
            {
                var match = Regex.Match(message, AnswerPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var answer = match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : message.Trim();
                    if (!string.IsNullOrEmpty(LastQuestion))
                    {
                        AddOrUpdate(LastQuestion, answer);
                        StatusText = $"Learned: Q:'{LastQuestion}' -> A:'{answer}'";
                        LastQuestion = string.Empty;
                        OnStateChanged?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"[Trivia] Answer regex error: {ex.Message}");
            }
        }
    }

    public void SendAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return;

        var packet = new Packet(0x7025);
        if (!string.IsNullOrWhiteSpace(ReplyTo))
        {
            packet.WriteByte(ChatType.Private);
            packet.WriteByte(1);
            if (Game.ClientType > GameClientType.Vietnam) packet.WriteByte(0);
            if (Game.ClientType >= GameClientType.Chinese) packet.WriteByte(0);
            packet.WriteString(ReplyTo.Trim());
            packet.WriteConditonalString(answer);
        }
        else
        {
            packet.WriteByte(ChatType.All);
            packet.WriteByte(1);
            if (Game.ClientType > GameClientType.Vietnam) packet.WriteByte(0);
            if (Game.ClientType >= GameClientType.Chinese) packet.WriteByte(0);
            packet.WriteConditonalString(answer);
        }

        PacketManager.SendPacket(packet, PacketDestination.Server);
    }
}
