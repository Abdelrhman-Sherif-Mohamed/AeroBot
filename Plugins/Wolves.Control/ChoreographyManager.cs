using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Control;

public enum FormationType
{
    None,
    Orbit,          // دوران مستمر حول القائد / دوران حول بعض
    CounterOrbit,   // دوران متعاكس حلزوني (حلقات متداخلة)
    Line,           // صف أفقي متساوي (كتف بكتف)
    Wedge,          // تشكيل رأس السهم العسكري V
    Circle,         // حلقة دائرية ثابتة حول الليدر
    Square,         // مربع حماية
    Column,         // طابور عسكري متتالي خلف الليدر
    Pulse,          // نبض وتوسع استعراضي (تنفس دائري)
    Stack,          // المشي المتطابق داخل بعض (İç İçe / Ghost Stack)
    Scatter,        // تشتت عشوائي استعراضي (Rastgele Scatter)
    ScatterX,       // تشتت عشوائي على محور X
    ScatterY,       // تشتت عشوائي على محور Y
    Triangle,       // تشكيل مثلث استعراضي
    Cross,          // تشكيل صليب / تقاطع +
    Diamond         // تشكيل معين ماسي
}

public enum EmoteActionType : byte
{
    Bow = 1,
    Cheer = 2,
    Clap = 3,
    SitStand = 4,
    Cry = 5,
    Anger = 6,
    Dance = 7
}

public class ChoreographyManager
{
    private static ChoreographyManager _instance;
    public static ChoreographyManager Instance => _instance ??= new ChoreographyManager();

    private Timer _formationTimer;
    private float _orbitAngle = 0f;
    private float _pulsePhase = 0f;
    private float _lastLeaderX = 0f;
    private float _lastLeaderY = 0f;
    private float _lastHeadingAngle = (float)(Math.PI / 2.0); // Default facing North

    public bool IsActive => CurrentFormation != FormationType.None;
    public FormationType CurrentFormation { get; private set; } = FormationType.None;
    public string TargetLeader { get; set; } = string.Empty;
    public float Radius { get; set; } = 6.0f;
    public int IntervalMs { get; set; } = 800;
    public int ManualSlotIndex { get; set; } = -1; // -1 = Auto
    public bool BroadcastToParty { get; set; } = true;
    public bool MimicLeader { get; set; } = false;
    public string StatusText { get; private set; } = "جاهز للإطلاق (Idle)";

    public event System.Action OnStateChanged;

    private ChoreographyManager()
    {
    }

    public void StartFormation(FormationType formation, string leaderName = null, float? radius = null)
    {
        if (formation == FormationType.None)
        {
            StopFormation();
            return;
        }

        CurrentFormation = formation;

        if (!string.IsNullOrWhiteSpace(leaderName))
            TargetLeader = leaderName.Trim();

        if (radius.HasValue && radius.Value > 1.0f)
            Radius = radius.Value;

        _orbitAngle = 0f;
        _pulsePhase = 0f;

        _formationTimer?.Dispose();
        _formationTimer = new Timer(FormationTick, null, 0, Math.Max(400, IntervalMs));

        StatusText = $"تشكيل نشط: {GetFormationArabicName(formation)} (نصف القطر: {Radius:F0}م)";
        ControlManager.Instance.Log($"[Choreography] بدأت الحركة الاستعراضية: {GetFormationArabicName(formation)}");

        OnStateChanged?.Invoke();
    }

    public void StopFormation()
    {
        CurrentFormation = FormationType.None;
        _formationTimer?.Dispose();
        _formationTimer = null;

        StatusText = "متوقف (Halted)";
        ControlManager.Instance.Log("[Choreography] تم إيقاف الحركة الاستعراضية وتثبيت المواقع.");

        OnStateChanged?.Invoke();
    }

    public void PerformSyncEmote(EmoteActionType action)
    {
        try
        {
            var packet = new Packet(0x704F);
            packet.WriteByte((byte)action);
            PacketManager.SendPacket(packet, PacketDestination.Server);

            ControlManager.Instance.Log($"[Choreography] حركة تعبيرية: {action}");
        }
        catch (Exception ex)
        {
            ControlManager.Instance.Log($"[Choreography] خطأ في الحركة التعبيرية: {ex.Message}");
        }
    }

    public void PerformEmoteWave(EmoteActionType action)
    {
        Task.Run(async () =>
        {
            int mySlot = DetermineMySlotIndex(out int _);
            int delayMs = Math.Max(0, mySlot * 350);

            if (delayMs > 0)
                await Task.Delay(delayMs);

            PerformSyncEmote(action);
        });
    }

    public void BroadcastCommand(string command)
    {
        if (Game.Player == null) return;

        try
        {
            var cleanCmd = command.Trim().ToUpperInvariant();
            if (!cleanCmd.StartsWith("!"))
                cleanCmd = "!" + cleanCmd;

            var packet = new Packet(0x7025);
            packet.WriteByte(ChatType.Party);
            packet.WriteByte(1);
            if (Game.ClientType > GameClientType.Vietnam) packet.WriteByte(0);
            if (Game.ClientType >= GameClientType.Chinese) packet.WriteByte(0);
            packet.WriteConditonalString(cleanCmd);

            PacketManager.SendPacket(packet, PacketDestination.Server);
            ControlManager.Instance.Log($"[Choreography] تم إرسال الأمر لشات البارتي: {cleanCmd}");
        }
        catch (Exception ex)
        {
            ControlManager.Instance.Log($"[Choreography] خطأ أثناء البث: {ex.Message}");
        }
    }

    public void ComeToLeader()
    {
        string leader = ResolveLeaderName();
        if (string.IsNullOrEmpty(leader) || Game.Player == null) return;

        if (SpawnManager.TryGetEntity<SpawnedPlayer>(p => string.Equals(p.Name, leader, StringComparison.OrdinalIgnoreCase), out var leaderEntity))
        {
            Game.Player.MoveTo(leaderEntity.Position, false);
            StatusText = $"انتقال فوري لموقع القائد [{leader}]";
            ControlManager.Instance.Log($"[Choreography] انتقال فوري إلى القائد [{leader}] (GEL).");
            OnStateChanged?.Invoke();
        }
    }

    private void FormationTick(object state)
    {
        try
        {
            if (CurrentFormation == FormationType.None || Game.Player == null)
                return;

            string leader = ResolveLeaderName();
            if (string.IsNullOrEmpty(leader))
            {
                StatusText = "بانتظار تحديد القائد...";
                OnStateChanged?.Invoke();
                return;
            }

            // Locate Leader Position & Heading
            Position anchorPos;
            float headingAngle = _lastHeadingAngle;

            if (string.Equals(leader, Game.Player.Name, StringComparison.OrdinalIgnoreCase))
            {
                // Current bot IS the leader / anchor
                anchorPos = Game.Player.Movement.Source;
            }
            else if (SpawnManager.TryGetEntity<SpawnedPlayer>(p => string.Equals(p.Name, leader, StringComparison.OrdinalIgnoreCase), out var leaderEntity))
            {
                anchorPos = leaderEntity.Position;

                // Dynamically update heading angle from velocity vector if moving
                float dxMove = anchorPos.X - _lastLeaderX;
                float dyMove = anchorPos.Y - _lastLeaderY;
                if ((dxMove * dxMove + dyMove * dyMove) > 0.15f)
                {
                    headingAngle = (float)Math.Atan2(dyMove, dxMove);
                    _lastHeadingAngle = headingAngle;
                }
                _lastLeaderX = anchorPos.X;
                _lastLeaderY = anchorPos.Y;
            }
            else
            {
                StatusText = $"القائد [{leader}] غير متواجد في المحيط";
                OnStateChanged?.Invoke();
                return;
            }

            // If current player is the leader and formation requires followers only, leader can walk freely
            if (string.Equals(leader, Game.Player.Name, StringComparison.OrdinalIgnoreCase))
            {
                StatusText = $"أنت قائد التشكيل [{GetFormationArabicName(CurrentFormation)}]";
                OnStateChanged?.Invoke();
                return;
            }

            int mySlot = DetermineMySlotIndex(out int totalFollowers);
            totalFollowers = Math.Max(1, totalFollowers);

            // Compute Target Offset (dX, dY)
            CalculateFormationOffset(CurrentFormation, mySlot, totalFollowers, headingAngle, out float dX, out float dY);

            float targetX = anchorPos.X + dX;
            float targetY = anchorPos.Y + dY;
            var targetPos = new Position(targetX, targetY, anchorPos.Region);

            float distToTarget = (float)Game.Player.Position.DistanceTo(targetPos);

            // Only move if needed (> 0.7m to prevent shaking) and within reasonable range (< 45m)
            if (distToTarget > 0.7f && distToTarget < 45.0f)
            {
                Game.Player.MoveTo(targetPos, false);
            }

            StatusText = $"تشكيل: {GetFormationArabicName(CurrentFormation)} | مقعد: {mySlot + 1}/{totalFollowers} | المسافة: {distToTarget:0.0}م";
            OnStateChanged?.Invoke();
        }
        catch
        {
        }
    }

    private void CalculateFormationOffset(FormationType formation, int slotIndex, int totalCount, float heading, out float dX, out float dY)
    {
        dX = 0f;
        dY = 0f;

        float r = Radius;

        switch (formation)
        {
            case FormationType.Orbit:
                {
                    float angle = _orbitAngle + (float)(2.0 * Math.PI * slotIndex / totalCount);
                    dX = r * (float)Math.Cos(angle);
                    dY = r * (float)Math.Sin(angle);
                    _orbitAngle += 0.35f;
                    if (_orbitAngle > (float)(Math.PI * 2.0))
                        _orbitAngle -= (float)(Math.PI * 2.0);
                    break;
                }

            case FormationType.CounterOrbit:
                {
                    bool isEven = (slotIndex % 2 == 0);
                    float ringR = isEven ? r : r + 2.5f;
                    float dir = isEven ? 1.0f : -1.0f;
                    int subIndex = slotIndex / 2;
                    int subTotal = Math.Max(1, (totalCount + 1) / 2);

                    float angle = (dir * _orbitAngle) + (float)(2.0 * Math.PI * subIndex / subTotal);
                    dX = ringR * (float)Math.Cos(angle);
                    dY = ringR * (float)Math.Sin(angle);

                    _orbitAngle += 0.35f;
                    if (_orbitAngle > (float)(Math.PI * 2.0))
                        _orbitAngle -= (float)(Math.PI * 2.0);
                    break;
                }

            case FormationType.Line:
                {
                    // Line abreast perpendicular to heading
                    float spacing = Math.Max(2.5f, r * 0.6f);
                    float lateral = (slotIndex - (totalCount - 1) / 2.0f) * spacing;
                    float forward = -1.5f; // slightly behind leader

                    dX = (lateral * -(float)Math.Sin(heading)) + (forward * (float)Math.Cos(heading));
                    dY = (lateral * (float)Math.Cos(heading)) + (forward * (float)Math.Sin(heading));
                    break;
                }

            case FormationType.Wedge:
                {
                    // Military V formation
                    int side = (slotIndex % 2 == 0) ? -1 : 1;
                    int rank = (slotIndex / 2) + 1;
                    float lateral = side * rank * (r * 0.5f);
                    float forward = -rank * (r * 0.6f);

                    dX = (lateral * -(float)Math.Sin(heading)) + (forward * (float)Math.Cos(heading));
                    dY = (lateral * (float)Math.Cos(heading)) + (forward * (float)Math.Sin(heading));
                    break;
                }

            case FormationType.Circle:
                {
                    float angle = (float)(2.0 * Math.PI * slotIndex / totalCount);
                    dX = r * (float)Math.Cos(angle);
                    dY = r * (float)Math.Sin(angle);
                    break;
                }

            case FormationType.Square:
                {
                    float half = r;
                    float[,] boxPoints = new float[,]
                    {
                        { -half,  half }, {  half,  half }, {  half, -half }, { -half, -half },
                        {   0f,   half }, {  half,    0f }, {   0f,  -half }, { -half,    0f }
                    };
                    int ptIdx = slotIndex % 8;
                    float lx = boxPoints[ptIdx, 0];
                    float ly = boxPoints[ptIdx, 1];

                    dX = (lx * -(float)Math.Sin(heading)) + (ly * (float)Math.Cos(heading));
                    dY = (lx * (float)Math.Cos(heading)) + (ly * (float)Math.Sin(heading));
                    break;
                }

            case FormationType.Column:
                {
                    // Single-file march
                    float distBehind = -(slotIndex + 1) * Math.Max(2.5f, r * 0.5f);
                    dX = distBehind * (float)Math.Cos(heading);
                    dY = distBehind * (float)Math.Sin(heading);
                    break;
                }

            case FormationType.Pulse:
                {
                    float pulseR = r + (float)(3.0 * Math.Sin(_pulsePhase));
                    _pulsePhase += 0.25f;
                    if (_pulsePhase > (float)(Math.PI * 2.0))
                        _pulsePhase -= (float)(Math.PI * 2.0);

                    float angle = (float)(2.0 * Math.PI * slotIndex / totalCount);
                    dX = pulseR * (float)Math.Cos(angle);
                    dY = pulseR * (float)Math.Sin(angle);
                    break;
                }

            case FormationType.Stack:
                {
                    // İç İçe (Ghost stack): Exact zero offset, character models overlap 100%
                    dX = 0f;
                    dY = 0f;
                    break;
                }

            case FormationType.Scatter:
                {
                    // Rastgele: Spread scatter around leader
                    float angle = (float)(2.0 * Math.PI * ((slotIndex * 37 + 13) % totalCount) / totalCount);
                    float dist = Math.Max(1.5f, (r * 0.4f) + ((slotIndex % 3) * (r * 0.35f)));
                    dX = dist * (float)Math.Cos(angle);
                    dY = dist * (float)Math.Sin(angle);
                    break;
                }

            case FormationType.ScatterX:
                {
                    // Rastgele X: Lateral dispersal
                    float lateral = (slotIndex - (totalCount - 1) / 2.0f) * (r * 0.7f);
                    dX = lateral * -(float)Math.Sin(heading);
                    dY = lateral * (float)Math.Cos(heading);
                    break;
                }

            case FormationType.ScatterY:
                {
                    // Rastgele Y: Longitudinal dispersal along heading
                    float forward = -(slotIndex + 1) * (r * 0.6f);
                    dX = forward * (float)Math.Cos(heading);
                    dY = forward * (float)Math.Sin(heading);
                    break;
                }

            case FormationType.Triangle:
                {
                    // Equilateral triangle
                    int side = slotIndex % 3;
                    float baseAngle = (float)(side * 2.0 * Math.PI / 3.0);
                    float triDist = r * 1.1f;
                    dX = triDist * (float)Math.Cos(baseAngle + heading);
                    dY = triDist * (float)Math.Sin(baseAngle + heading);
                    break;
                }

            case FormationType.Cross:
                {
                    // Cross / Plus (+) formation
                    int arm = slotIndex % 4;
                    float crossAngle = (float)(arm * Math.PI / 2.0);
                    float crossDist = r * (0.6f + (slotIndex / 4) * 0.5f);
                    dX = crossDist * (float)Math.Cos(crossAngle + heading);
                    dY = crossDist * (float)Math.Sin(crossAngle + heading);
                    break;
                }

            case FormationType.Diamond:
                {
                    // Diamond / Rhombus
                    int diamondPoint = slotIndex % 4;
                    float diaAngle = (float)(diamondPoint * Math.PI / 2.0 + Math.PI / 4.0);
                    float diaDist = r;
                    dX = diaDist * (float)Math.Cos(diaAngle + heading);
                    dY = diaDist * (float)Math.Sin(diaAngle + heading);
                    break;
                }
        }
    }

    private string ResolveLeaderName()
    {
        if (!string.IsNullOrWhiteSpace(TargetLeader))
            return TargetLeader;

        if (Game.Party?.Leader != null && !string.IsNullOrWhiteSpace(Game.Party.Leader.Name))
            return Game.Party.Leader.Name;

        return ControlManager.Instance.Leaders.FirstOrDefault() ?? string.Empty;
    }

    private int DetermineMySlotIndex(out int totalCount)
    {
        if (ManualSlotIndex >= 0)
        {
            totalCount = Math.Max(ManualSlotIndex + 1, 8);
            return ManualSlotIndex;
        }

        string leader = ResolveLeaderName();

        if (Game.Party?.Members != null && Game.Party.Members.Count > 0)
        {
            var followers = Game.Party.Members
                .Where(m => !string.Equals(m.Name, leader, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.MemberId)
                .ToList();

            totalCount = Math.Max(1, followers.Count);
            int idx = followers.FindIndex(m => string.Equals(m.Name, Game.Player?.Name, StringComparison.OrdinalIgnoreCase));
            return idx >= 0 ? idx : 0;
        }

        // Vicinity fallback: calculate from nearby players with similar names / party
        totalCount = 4;
        return 0;
    }

    public static string GetFormationArabicName(FormationType formation)
    {
        return formation switch
        {
            FormationType.Orbit => "دوران استعراضي (Orbit Carousel)",
            FormationType.CounterOrbit => "دوران حلزوني متعاكس (Counter-Orbit)",
            FormationType.Line => "صف أفقي متساوي (Line Formation)",
            FormationType.Wedge => "رأس سهم عسكري (V-Wedge)",
            FormationType.Circle => "حلقة دائرية (Circle Ring)",
            FormationType.Square => "مربع حماية (Square Box)",
            FormationType.Column => "طابور خلف بعض (Column March)",
            FormationType.Pulse => "نبض وتوسع استعراضي (Pulse In/Out)",
            FormationType.Stack => "المشي داخل بعض (Ghost Stack)",
            FormationType.Scatter => "تشتت استعراضي عشوائي (Scatter)",
            FormationType.ScatterX => "تشتت أفقي (Scatter X)",
            FormationType.ScatterY => "تشتت طولي (Scatter Y)",
            FormationType.Triangle => "تشكيل مثلث (Triangle)",
            FormationType.Cross => "تشكيل صليب / تقاطع (Cross)",
            FormationType.Diamond => "تشكيل معيّن ماسي (Diamond)",
            _ => "لا يوجد"
        };
    }
}
