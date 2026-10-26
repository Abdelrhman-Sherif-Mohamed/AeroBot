using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RSBot.Core.Security;

public class AuthConfig
{
    private static AuthConfig _instance;
    public static AuthConfig Instance => _instance ??= Load();

    [JsonPropertyName("firebaseUrl")]
    public string FirebaseUrl { get; set; } = "https://wolves-1eb1f-default-rtdb.firebaseio.com/";

    [JsonPropertyName("authSecret")]
    public string AuthSecret { get; set; } = string.Empty;

    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; } = true;

    [JsonPropertyName("rememberedUsername")]
    public string RememberedUsername { get; set; } = string.Empty;

    [JsonPropertyName("rememberedPassword")]
    public string RememberedPassword { get; set; } = string.Empty;

    [JsonPropertyName("autoLogin")]
    public bool AutoLogin { get; set; } = false;

    private static string GetConfigPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataDir = Path.Combine(baseDir, "Data");
        if (!Directory.Exists(dataDir))
        {
            try { Directory.CreateDirectory(dataDir); } catch { }
        }
        return Path.Combine(dataDir, "auth_config.json");
    }

    public static AuthConfig Load()
    {
        var path = GetConfigPath();
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<AuthConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (config != null)
                {
                    if (string.IsNullOrWhiteSpace(config.FirebaseUrl) ||
                        config.FirebaseUrl.Contains("wolves-bot-default-rtdb"))
                    {
                        config.FirebaseUrl = "https://wolves-1eb1f-default-rtdb.firebaseio.com/";
                        config.Save();
                    }

                    _instance = config;
                    return config;
                }
            }
            catch { }
        }

        var newConfig = new AuthConfig();
        newConfig.Save();
        _instance = newConfig;
        return newConfig;
    }

    public void Save()
    {
        try
        {
            var path = GetConfigPath();
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
        }
        catch { }
    }
}
