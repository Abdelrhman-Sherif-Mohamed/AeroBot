using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Map.Renderer;
using RSBot.NavMeshApi.Dungeon;
using SDUI.Controls;
using SDUI.Helpers;
using Region = RSBot.Core.Objects.Region;

namespace RSBot.Map.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    /// <summary>
    ///     Available zoom levels
    /// </summary>
    private static readonly float[] ZoomLevels = { 0.20f, 0.25f, 0.33f, 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };

    /// <summary>
    ///     The current zoom level index (default 1.0f)
    /// </summary>
    private int _zoomIndex = 5;

    private float CurrentZoom => ZoomLevels[_zoomIndex];
    private float CurrentScale => (SectorSize / 192.0f) * CurrentZoom;
    private int CurrentGridSize => CurrentZoom <= 0.25f ? 9 : (CurrentZoom <= 0.35f ? 7 : (CurrentZoom <= 0.6f ? 5 : 3));
    private int _currentGridSizeDrawn = 0;

    private Position? _activeWalkTarget;
    private readonly System.Windows.Forms.Timer _walkTimer = new() { Interval = 600 };

    /// <summary>
    ///     The Sector Image Size
    /// </summary>
    private const int SectorSize = 256;

    /// <summary>
    ///     The cached Images
    /// </summary>
    private readonly Dictionary<string, Image> _cachedImages;

    /// <summary>
    ///     The current sector graphic
    /// </summary>
    private Image _currentSectorGraphic;

    /// <summary>
    ///     The X Sector identifier
    /// </summary>
    private byte _currentXSec;

    /// <summary>
    ///     The Y Sector identifier
    /// </summary>
    private byte _currentYSec;


    /// <summary>
    ///     The map points
    /// </summary>
    private Image[] _mapEntityImages;

    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    private Bitmap _frame;

    /// <summary>
    ///     Cached town teleporters (static game data): teleport + position + display name.
    /// </summary>
    private List<(RefTeleport Teleport, Position Position, string Name)> _towns;

    private readonly NavMeshRenderer _navMeshRenderer;
    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);
        if (DesignMode)
            return;

        _cachedImages ??= new();

        EventManager.SubscribeEvent("OnEnterGame", OnEnterGame);

        mapCanvas.Paint += mapCanvas_Paint;
        mapCanvas.MouseWheel += mapCanvas_MouseWheel;
        mapCanvas.MouseEnter += (s, e) => mapCanvas.Focus();

        _walkTimer.Tick += WalkTimer_Tick;
        _walkTimer.Start();

        // All
        comboViewType.SelectedIndex = 6;
        checkEnableCollisions.Checked = Kernel.EnableCollisionDetection;
        tabNavMeshViewer.Visible = Kernel.Debug;

        if (Kernel.Debug)
        {
            _navMeshRenderer = new NavMeshRenderer()
            {
                Dock = DockStyle.Fill,
            };

            panelNavMeshRendererCanvas.Controls.Add(_navMeshRenderer);
            labelSectorInfo.Visible = true;
        }
    }

    #region Core Handlers

    private void OnEnterGame()
    {
        _cachedImages.Clear();
        if (_mapEntityImages == null)
            _mapEntityImages = new[]
            {
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_character.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_animal.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_npc.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_otherplayer.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_monster.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_unique.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_party.ddj").ToImage(),
                Game.MediaPk2.GetFile("interface\\minimap\\mm_sign_unique.ddj").ToImage()
            };
    }

    #endregion Core Handlers

    /// <summary>
    ///     Adds the grid item.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="type">The type.</param>
    /// <param name="level">The level.</param>
    /// <param name="position"></param>
    private bool _gridRebuild = true;
    private long _mapTick;

    private void AddGridItem(string name, string type, byte level, Position position)
    {
        if (!_gridRebuild)
            return;

        if (string.IsNullOrWhiteSpace(name))
            name = LanguageManager.GetLang("NoName");

        var item = new ListViewItem(name);
        item.SubItems.Add(type);
        item.SubItems.Add(level.ToString());
        item.SubItems.Add($"X:{position.X:0} Y:{position.Y:0}");
        lvMonster.Items.Add(item);
    }

    /// <summary>
    ///     Draws the point at.
    /// </summary>
    private void DrawPointAt(Graphics gfx, Position position, int entityIndex)
    {
        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

            if (x < -40 || x > mapCanvas.Width + 40 || y < -40 || y > mapCanvas.Height + 40)
                return;

            using var img = (Image)_mapEntityImages[entityIndex].Clone();

            if (entityIndex == 0)
                gfx.DrawImage(RotateImage(img, Geometry.RadianToDegree(Game.Player.Movement.Angle)),
                    x - img.Width / 2,
                    y - img.Height / 2);
            else
                gfx.DrawImage(img, x - img.Width / 2, y - img.Height / 2);
        }
        catch
        {
        }
    }

    private void DrawRectangleAt(Graphics gfx, Position position, Brush brush, Size size, string label = "")
    {
        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

            if (x < -60 || x > mapCanvas.Width + 60 || y < -60 || y > mapCanvas.Height + 60)
                return;

            if (!string.IsNullOrEmpty(label))
                gfx.DrawString(label, Font, brush, x + size.Width, y - size.Width / 2);

            gfx.FillRectangle(brush, new RectangleF(new PointF(x, y), size));
        }
        catch
        {
        }
    }

    private void DrawLineAt(Graphics gfx, Position source, Position destination, Pen color)
    {
        var srcX = GetMapX(source);
        var srcY = GetMapY(source);
        var dstX = GetMapX(destination);
        var dstY = GetMapY(destination);

        var minX = Math.Min(srcX, dstX);
        var maxX = Math.Max(srcX, dstX);
        var minY = Math.Min(srcY, dstY);
        var maxY = Math.Max(srcY, dstY);
        if (maxX < -50 || minX > mapCanvas.Width + 50 || maxY < -50 || minY > mapCanvas.Height + 50)
            return;

        gfx.DrawLine(color, srcX, srcY, dstX, dstY);
    }

    private void DrawCircleAt(Graphics gfx, Position position, Color color, int diameter)
    {
        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

            if (x < -100 || x > mapCanvas.Width + 100 || y < -100 || y > mapCanvas.Height + 100)
                return;

            using var brush = new SolidBrush(color);

            var diameterF = diameter * CurrentScale;
            var point = new PointF(x - diameterF / 2, y - diameterF / 2);

            gfx.FillEllipse(brush, new RectangleF(point, new SizeF(diameterF, diameterF)));
            gfx.DrawEllipse(new Pen(color), new RectangleF(point, new SizeF(diameterF, diameterF)));
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Fills the grid.
    /// </summary>
    private void PopulateMapAndGrid(Graphics graphics, bool rebuildGrid)
    {
        _gridRebuild = rebuildGrid;
        if (rebuildGrid)
        {
            lvMonster.BeginUpdate();
            lvMonster.Items.Clear();
        }

        try
        {
            if (Game.Player.Movement.HasDestination)
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                using var pen = new Pen(Color.BlanchedAlmond, 1);
                pen.DashStyle = DashStyle.Dot;
                DrawLineAt(graphics, Game.Player.Movement.Source, Game.Player.Movement.Destination, pen);

                DrawCircleAt(graphics, Game.Player.Movement.Destination, Color.PaleGreen, 4);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
            }

            if (_activeWalkTarget.HasValue)
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                using var walkPen = new Pen(Color.LimeGreen, 1.5f) { DashStyle = DashStyle.Dash };
                DrawLineAt(graphics, Game.Player.Movement.Source, _activeWalkTarget.Value, walkPen);
                DrawCircleAt(graphics, _activeWalkTarget.Value, Color.LimeGreen.Alpha(180), 8);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
            }

            //Draw walk script
            if (ScriptManager.Running)
            {
                var walkScript = ScriptManager.GetWalkScript();
                for (var i = 0; i < walkScript.Count; i++)
                {
                    var nextPosition = walkScript[i];

                    DrawLineAt(graphics, i != 0 ? walkScript[i - 1] : nextPosition, nextPosition, Pens.LightBlue);
                    DrawCircleAt(graphics, nextPosition, Color.CornflowerBlue.Alpha(150), 4);
                }
            }

            if (Kernel.Bot.Running)
            {
                var position = Kernel.Bot.Botbase.Area.Position;
                var radius = Kernel.Bot.Botbase.Area.Radius;

                DrawCircleAt(graphics, position, Color.DarkRed.Alpha(100), radius * 2);
                DrawCircleAt(graphics, position, Color.LawnGreen.Alpha(50), radius);
            }

            if (comboViewType.SelectedIndex == 0 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedMonster>(out var monsters))
                    foreach (var entry in monsters)
                    {
                        AddGridItem(entry.Record.GetRealName(), entry.Rarity.GetName(),
                            entry.Record.Level, entry.Movement.Source);

                        if (Game.SelectedEntity?.UniqueId == entry.UniqueId)
                            DrawCircleAt(graphics, entry.Position, Color.Wheat.Alpha(100), 6);

                        if (entry.Rarity == MonsterRarity.Unique || entry.Rarity == MonsterRarity.Unique2)
                            DrawPointAt(graphics, entry.Position, 5);
                        else
                            DrawPointAt(graphics, entry.Position, 4);
                    }

            if (comboViewType.SelectedIndex == 4 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedCos>(out var coses))
                    foreach (var entry in coses)
                        // Avoid painting vehicles from main player
                        if (Game.Player.Vehicle?.UniqueId != entry.UniqueId)
                        {
                            AddGridItem(entry.Name, "Pet", entry.Record.Level, entry.Movement.Source);
                            DrawPointAt(graphics, entry.Movement.Source, 1);
                        }

            if (comboViewType.SelectedIndex == 2 || comboViewType.SelectedIndex == 6)
                if (Game.Party != null && Game.Party.Members != null)
                    foreach (var member in Game.Party.Members.ToArray())
                    {
                        if (member.Name == Game.Player.Name)
                            continue;

                        DrawPointAt(graphics, member.Position, 6);

                        var mx = GetMapX(member.Position);
                        var my = GetMapY(member.Position);
                        if (mx >= -40 && mx <= mapCanvas.Width + 40 && my >= -40 && my <= mapCanvas.Height + 40)
                        {
                            using var font = new Font("Segoe UI", 8, FontStyle.Bold);
                            using var brush = new SolidBrush(Color.Cyan);
                            var nameSize = graphics.MeasureString(member.Name, font);
                            graphics.DrawString(member.Name, font, brush, mx - nameSize.Width / 2f, my - 16);
                        }

                        AddGridItem(member.Name, "Party Member", member.Level, member.Position);
                    }

            if (comboViewType.SelectedIndex == 1 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedPlayer>(out var players))
                    foreach (var entry in players)
                    {
                        if (Game.Party != null && Game.Party.Members != null &&
                            Game.Party.GetMemberByName(entry.Name) != null)
                            continue;

                        AddGridItem(entry.Name, "Player", 0, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 3);
                    }

            if (comboViewType.SelectedIndex == 3 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedNpcNpc>(out var npcs))
                    foreach (var entry in npcs)
                    {
                        AddGridItem(entry.Record.GetRealName(), entry.UniqueId.ToString(),
                            entry.Record.Level, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 2);
                    }

            if (comboViewType.SelectedIndex == 5 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedPortal>(out var portals))
                    foreach (var entry in portals)
                    {
                        AddGridItem(entry.Record.GetRealName(), "Teleport", 0, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 7);
                    }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Map] Render error: {ex.Message}");
        }

        if (rebuildGrid)
            lvMonster.EndUpdate();
    }

    private Image LoadSectorImage(string sectorImgName)
    {
        if (_cachedImages.ContainsKey(sectorImgName))
            return (Image)_cachedImages[sectorImgName].Clone();

        if (Game.MediaPk2.FileExists(sectorImgName) && Game.MediaPk2.TryGetFile(sectorImgName, out var file))
        {
            var img = file.ToImage();
            _cachedImages.Add(sectorImgName, img);

            return (Image)img.Clone();
        }

        return new Bitmap(SectorSize, SectorSize);
    }


    /// <summary>
    ///     Redraw the map image
    /// </summary>
    private void RedrawMap()
    {
        // Set layer path & sectors
        var p = Game.Player.Movement.Source;
        
        var tempX = p.Region.X;
        var tempY = p.Region.Y;

        if (p.Region.IsDungeon)
        {
            tempX = p.GetSectorFromOffset(p.XOffset);
            tempY = p.GetSectorFromOffset(p.YOffset);
        }

        var gridSize = CurrentGridSize;
        if (tempX == _currentXSec && tempY == _currentYSec && _currentSectorGraphic != null && _currentGridSizeDrawn == gridSize)
            return;

        _currentXSec = tempX;
        _currentYSec = tempY;
        _currentGridSizeDrawn = gridSize;

        if (_cachedImages.Count >= 60)
            _cachedImages.Clear();

        try
        {
            _currentSectorGraphic?.Dispose();
            _currentSectorGraphic = new Bitmap(SectorSize * gridSize, SectorSize * gridSize, PixelFormat.Format32bppArgb);

            using var gfx = Graphics.FromImage(_currentSectorGraphic);
            gfx.InterpolationMode = InterpolationMode.Bicubic;

            var floorName = string.Empty;
            var dungeonName = string.Empty;
            if (p.Region.IsDungeon)
            {
                if (!p.TryGetNavMeshTransform(out var pTransform))
                    return;

                if (pTransform.Instance is not NavMeshInstBlock dungeonBlock)
                    return;

                if (dungeonBlock.Parent is not NavMeshDungeon dungeon)
                    return;

                floorName = dungeon.FloorStringIDs[dungeonBlock.FloorIndex]; //e.g "DH_A01_FLOOR01"
                var roomName = dungeon.RoomStringIDs[dungeonBlock.RoomIndex];
                var roomNameTranslated = Game.ReferenceManager.GetTranslation(roomName);

                lblRegion.Text = roomNameTranslated;
                dungeonName = RegionInfoManager.GetDungeonName(p.Region);
            } 

            var halfGrid = gridSize / 2;
            for (byte x = 0; x < gridSize; x++)
            {
                for (byte z = 0; z < gridSize; z++)
                {
                    var xSector = (byte) (_currentXSec + x - halfGrid);
                    var ySector = (byte) (_currentYSec + z - halfGrid);

                    var sectorImgName = GetMinimapFileName(new Region(xSector, ySector), dungeonName, floorName);
                    using var bitmap = LoadSectorImage(sectorImgName);
                    var pos = new Point(bitmap.Width * x, bitmap.Height * (gridSize - 1 - z));

                    gfx.DrawImage(bitmap, pos);

                    if (Kernel.Debug)
                    {
                        using var pen = new Pen(Color.Black);
                        pen.DashStyle = DashStyle.Dot;
                        gfx.DrawRectangle(pen, new Rectangle(pos, new Size(SectorSize, SectorSize)));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn($"Error in minimap: {e.Message}");
        }
    }
    private string GetMinimapFileName(Region region, string dungeonName, string floorName)
    {
        if (!string.IsNullOrWhiteSpace(dungeonName) && !string.IsNullOrWhiteSpace(floorName)) {
            return $"minimap_d\\{dungeonName}\\{floorName}_{region.X}x{region.Y}.ddj";
        }

        return $"minimap\\{region.X}x{region.Y}.ddj";
    }

    private Bitmap RotateImage(Image image, float angle)
    {
        var sizedBitmap = new Bitmap(image.Width + 1, image.Height + 1);

        using (var matrix = new Matrix())
        {
            matrix.RotateAt(angle, new PointF(sizedBitmap.Width / 2, sizedBitmap.Height / 2));

            sizedBitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(sizedBitmap))
            {
                graphics.Transform = matrix;
                graphics.InterpolationMode = InterpolationMode.Bicubic;
                graphics.DrawImage(image, 1, 1);
            }
        }

        sizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

        return sizedBitmap;
    }

    private void mapCanvas_Paint(object sender, PaintEventArgs e)
    {
        if (_frame != null)
        {
            try
            {
                e.Graphics.DrawImage(_frame, 0, 0);
            }
            catch
            {
            }
        }
    }

    private void DrawObjects(Graphics graphics, bool rebuildGrid)
    {
        if (_currentSectorGraphic != null)
        {
            graphics.InterpolationMode = InterpolationMode.Bicubic;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;

            var gridSize = _currentGridSizeDrawn > 0 ? _currentGridSizeDrawn : CurrentGridSize;
            var halfGrid = gridSize / 2;

            var p = Game.Player.Movement.Source;
            var playerXSecOffsetInMeters = p.XSectorOffset / 10f;
            var playerYSecOffsetInMeters = p.YSectorOffset / 10f;
            var unscaledScale = SectorSize / 192f;

            var unscaledPlayerX = halfGrid * SectorSize + playerXSecOffsetInMeters * unscaledScale;
            var unscaledPlayerY = (halfGrid + 1) * SectorSize - playerYSecOffsetInMeters * unscaledScale;

            var destX = mapCanvas.Width / 2f - unscaledPlayerX * CurrentZoom;
            var destY = mapCanvas.Height / 2f - unscaledPlayerY * CurrentZoom;
            var destW = _currentSectorGraphic.Width * CurrentZoom;
            var destH = _currentSectorGraphic.Height * CurrentZoom;

            graphics.DrawImage(_currentSectorGraphic, destX, destY, destW, destH);

            PopulateMapAndGrid(graphics, rebuildGrid);
            DrawTownMarkers(graphics);
            DrawPointAt(graphics, Game.Player.Movement.Source, 0);
        }
    }

    /// <summary>
    ///     True ONLY for real towns (respawn points per game data).
    /// </summary>
    private static bool IsTownTeleport(RefTeleport t)
    {
        try
        {
            if (t.GetLinks().Count == 0)
                return false;

            return t.CanBeResurrectPos == 1 || t.CanGotoResurrectPos == 1;
        }
        catch
        {
            return false;
        }
    }

    private void EnsureTowns()
    {
        if (_towns != null)
            return;

        _towns = new List<(RefTeleport, Position, string)>();
        try
        {
            foreach (var t in Game.ReferenceManager.TeleportData)
            {
                if (!IsTownTeleport(t))
                    continue;

                var name = string.IsNullOrEmpty(t.ZoneName) ? t.CodeName : t.ZoneName;
                _towns.Add((t, t.GetPosition(), name));
            }
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Draws golden markers + names for nearby towns on the minimap.
    /// </summary>
    private void DrawTownMarkers(Graphics graphics)
    {
        try
        {
            EnsureTowns();

            foreach (var town in _towns)
            {
                var x = GetMapX(town.Position);
                var y = GetMapY(town.Position);
                if (x < -60 || x > mapCanvas.Width + 60 || y < -60 || y > mapCanvas.Height + 60)
                    continue;

                DrawCircleAt(graphics, town.Position, Color.Gold, 6);

                try
                {
                    using var font = new Font(Font.FontFamily, 8, FontStyle.Bold);
                    using var brush = new SolidBrush(Color.Gold);
                    graphics.DrawString(town.Name, font, brush, x + 6, y - 8);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Updates the "nearest town" label (world knowledge, from game data).
    /// </summary>
    private void UpdateNearestTown()
    {
        try
        {
            EnsureTowns();

            var bestName = string.Empty;
            var bestDist = double.MaxValue;
            foreach (var town in _towns)
            {
                var dist = town.Position.DistanceToPlayer();
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestName = town.Name;
                }
            }

            lblNearestTown.Text = string.IsNullOrEmpty(bestName)
                ? "Nearest town: -"
                : $"Nearest town: {bestName} ({bestDist:0}m)";
        }
        catch
        {
        }
    }

    private void trmInterval_Tick(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        if (!Visible)
            return;

        try
        {
            lblRegion.Text = Game.ReferenceManager.GetTranslation(Game.Player.Position.Region.ToString()) +
                             (Game.Player.Position.Region.IsDungeon ? " (Dungeon)" : "");

            lblX.Text = Game.Player.Position.X.ToString("0.0");
            lblY.Text = Game.Player.Position.Y.ToString("0.0");

            UpdateNearestTown();

            if (Kernel.Debug)
                labelSectorInfo.Text =
                    $"{Game.Player.Movement.Source.Region} ({Game.Player.Movement.Source.Region.X}x{Game.Player.Movement.Source.Region.Y})";

            var w = mapCanvas.ClientSize.Width;
            var h = mapCanvas.ClientSize.Height;
            if (w <= 0 || h <= 0)
                return;

            // Fresh frame every tick: never draws to a dead/stale surface,
            // and Paint replays the last frame (minimize/restore/tab switch safe).
            if (_frame == null || _frame.Width != w || _frame.Height != h)
            {
                _frame?.Dispose();
                _frame = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            }

            using (var gfx = Graphics.FromImage(_frame))
            {
                gfx.Clear(Color.Black);
                RedrawMap();
                // Entity list rebuilds ~2x/sec; canvas repaints every tick
                DrawObjects(gfx, _mapTick++ % 3 == 0);
            }

            using (var target = mapCanvas.CreateGraphics())
                target.DrawImage(_frame, 0, 0);

            // Keep the NavMesh viewer centered on the player automatically.
            // (It stayed gray because Update() only ran on "Reset to player".)
            if (_navMeshRenderer != null && tabControl1.SelectedTab == tabNavMeshViewer)
            {
                try
                {
                    if (Game.Player.Position.TryGetNavMeshTransform(out var playerTransform))
                        _navMeshRenderer.Update(playerTransform);
                }
                catch
                {
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Map] tick: {ex.Message}");
        }
    }

    private void checkBoxAutoSelectUniques_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Map.AutoSelectUnique", checkBoxAutoSelectUniques.Checked);
        timerUniqueChecker.Enabled = checkBoxAutoSelectUniques.Checked;
    }

    private void timerUniqueChecker_Tick(object sender, EventArgs e)
    {
        if (!checkBoxAutoSelectUniques.Checked)
            return;

        if (Kernel.Bot.Running)
            return;

        if (Game.SelectedEntity?.Record.Rarity == ObjectRarity.ClassD)
            return;

        if (SpawnManager.TryGetEntity<SpawnedMonster>(
                p => p.Record.Rarity == ObjectRarity.ClassD || p.Record.Rarity == ObjectRarity.ClassI,
                out var uniqueEntity))
            uniqueEntity.TrySelect();
    }

    private float GetMapX(Position gamePosition)
    {
        return mapCanvas.Width / 2f + (gamePosition.X - Game.Player.Movement.Source.X) * CurrentScale;
    }

    private float GetMapY(Position gamePosition)
    {
        return mapCanvas.Height / 2f - (gamePosition.Y - Game.Player.Movement.Source.Y) * CurrentScale;
    }

    private void WalkTimer_Tick(object sender, EventArgs e)
    {
        if (!_activeWalkTarget.HasValue || Game.Player == null)
            return;

        StepTowardsActiveTarget();
    }

    private void StepTowardsActiveTarget()
    {
        if (!_activeWalkTarget.HasValue || Game.Player == null)
            return;

        var playerPos = Game.Player.Movement.Source;
        var dest = _activeWalkTarget.Value;
        var dist = playerPos.DistanceTo(dest);

        if (dist <= 4.0f)
        {
            _activeWalkTarget = null;
            return;
        }

        // If player is already walking towards a step and is not close to finishing it yet, wait
        if (Game.Player.Movement.HasDestination)
        {
            var curStepDist = playerPos.DistanceTo(Game.Player.Movement.Destination);
            if (curStepDist > 12.0f)
                return;
        }

        if (dist <= 90.0f)
        {
            Game.Player.MoveTo(dest, false);
        }
        else
        {
            var ratio = 90.0f / (float)dist;
            var stepX = playerPos.X + (dest.X - playerPos.X) * ratio;
            var stepY = playerPos.Y + (dest.Y - playerPos.Y) * ratio;
            var stepPosition = new Position(stepX, stepY, playerPos.Region);
            Game.Player.MoveTo(stepPosition, false);
        }
    }

    private void ShowMapContextMenu(Point screenPoint, Position targetPosition)
    {
        var menu = new SDUI.Controls.ContextMenuStrip();

        var walkItem = new ToolStripMenuItem("Walk to this location (امشِ إلى هنا)");
        walkItem.Click += (s, ev) =>
        {
            _activeWalkTarget = targetPosition;
            StepTowardsActiveTarget();
        };
        menu.Items.Add(walkItem);

        var areaItem = new ToolStripMenuItem("Set Training Center Here (تعيين مركز التدريب)");
        areaItem.Click += (s, ev) =>
        {
            PlayerConfig.Set("RSBot.Area.Region", targetPosition.Region.Id);
            PlayerConfig.Set("RSBot.Area.X", targetPosition.XOffset);
            PlayerConfig.Set("RSBot.Area.Y", targetPosition.YOffset);
            PlayerConfig.Set("RSBot.Area.Z", targetPosition.ZOffset);
        };
        menu.Items.Add(areaItem);

        if (_activeWalkTarget.HasValue)
        {
            var cancelItem = new ToolStripMenuItem("Cancel Movement (إلغاء التحرك)");
            cancelItem.Click += (s, ev) =>
            {
                _activeWalkTarget = null;
                Game.Player.MoveTo(Game.Player.Movement.Source, false);
            };
            menu.Items.Add(cancelItem);
        }

        menu.Show(mapCanvas, screenPoint);
    }

    private void mapCanvas_MouseClick(object sender, MouseEventArgs e)
    {
        if (Game.Player == null)
            return;

        var deltaX = (e.X - mapCanvas.Width / 2f) / CurrentScale;
        var deltaY = (mapCanvas.Height / 2f - e.Y) / CurrentScale;

        var playerPos = Game.Player.Movement.Source;
        var targetX = playerPos.X + deltaX;
        var targetY = playerPos.Y + deltaY;

        var targetPosition = new Position(targetX, targetY, playerPos.Region);

        if (e.Button == MouseButtons.Right)
        {
            ShowMapContextMenu(e.Location, targetPosition);
            return;
        }

        _activeWalkTarget = targetPosition;
        StepTowardsActiveTarget();
    }

    private void ZoomIn()
    {
        if (_zoomIndex < ZoomLevels.Length - 1)
        {
            _zoomIndex++;
            OnZoomChanged();
        }
    }

    private void ZoomOut()
    {
        if (_zoomIndex > 0)
        {
            _zoomIndex--;
            OnZoomChanged();
        }
    }

    private void OnZoomChanged()
    {
        lblZoomLevel.Text = $"{(int)(CurrentZoom * 100)}%";
        _currentGridSizeDrawn = 0; // Force RedrawMap with new grid size if changed
    }

    private void btnZoomIn_Click(object sender, EventArgs e) => ZoomIn();
    private void btnZoomOut_Click(object sender, EventArgs e) => ZoomOut();
    private void lblZoomLevel_Click(object sender, EventArgs e)
    {
        _zoomIndex = 5; // Reset to 100%
        OnZoomChanged();
    }

    private void mapCanvas_MouseWheel(object sender, MouseEventArgs e)
    {
        if (e.Delta > 0)
            ZoomIn();
        else if (e.Delta < 0)
            ZoomOut();
    }

    private void checkEnableCollisions_CheckedChanged(object sender, EventArgs e)
    {
        Kernel.EnableCollisionDetection = checkEnableCollisions.Checked;
    }

    /// <summary>
    ///     Occurs before Main form is displayed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Main_Load(object sender, EventArgs e)
    {
        checkBoxAutoSelectUniques.Checked = PlayerConfig.Get("RSBot.Map.AutoSelectUnique", false);
        timerUniqueChecker.Enabled = checkBoxAutoSelectUniques.Checked;
    }

    private void btnNvmResetToPlayer_Click(object sender, EventArgs e)
    {
        if (Game.Player.Position.TryGetNavMeshTransform(out var playerTransform))
            _navMeshRenderer?.Update(playerTransform);
    }
}