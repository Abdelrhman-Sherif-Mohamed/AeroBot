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
using SDUI;
using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Region = RSBot.Core.Objects.Region;

namespace RSBot.Map.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    /// <summary>
    ///     The grid size
    /// </summary>
    private const int GridSize = 3;

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
    ///     The Zoom identifier
    /// </summary>
    private readonly float _scale = SectorSize / 192.0f;

    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    private BufferedGraphics bufferedGraphics;

    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    private readonly BufferedGraphicsContext bufferedGraphicsContext;

    private readonly NavMeshRenderer _navMeshRenderer;

    /// <summary>
    /// The region name
    /// </summary>
    private string _regionName;

    private bool _applyingMapLayout;
    private Size _mapBufferSize;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        InitializeComponent();
        if (DesignMode)
            return;

        _cachedImages ??= new();

        EventManager.SubscribeEvent("OnEnterGame", OnEnterGame);

        bufferedGraphicsContext = BufferedGraphicsManager.Current;
        bufferedGraphicsContext.MaximumBuffer = new Size(mapCanvas.Width + 1, mapCanvas.Height + 1);
        bufferedGraphics = bufferedGraphicsContext.Allocate(mapCanvas.CreateGraphics(), mapCanvas.ClientRectangle);

        // All
        comboViewType.SelectedIndex = 6;
        checkEnableCollisions.Checked = Kernel.EnableCollisionDetection;
        tabNavMeshViewer.Visible = true;

        _navMeshRenderer = new NavMeshRenderer()
        {
            Dock = DockStyle.Fill,
        };

        panelNavMeshRendererCanvas.Controls.Add(_navMeshRenderer);
        labelSectorInfo.Visible = true;
        tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
        mapCanvas.Resize += MapCanvas_Resize;

        ApplyMapLocalization();
        ApplyMapLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyMapLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyMapLocalization();
        ApplyMapLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private static string T(string key, string fallback)
    {
        return LanguageManager.GetLangBySpecificKey("RSBot.Map", key, fallback);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.Anchor = AnchorStyles.None;
        control.Dock = DockStyle.None;
        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private void PlaceLabel(SDUI.Controls.Label label, int x, int y, int width, int height,
        ContentAlignment alignment)
    {
        if (label == null)
            return;

        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    private void MapCanvas_Resize(object sender, EventArgs e)
    {
        ResizeMapBuffer();
    }

    private void ResizeMapBuffer()
    {
        if (mapCanvas == null || mapCanvas.Width <= 0 || mapCanvas.Height <= 0)
            return;

        if (_mapBufferSize == mapCanvas.Size)
            return;

        try
        {
            _mapBufferSize = mapCanvas.Size;
            bufferedGraphicsContext.MaximumBuffer = new Size(mapCanvas.Width + 1, mapCanvas.Height + 1);
            bufferedGraphics?.Dispose();
            bufferedGraphics = bufferedGraphicsContext.Allocate(mapCanvas.CreateGraphics(), mapCanvas.ClientRectangle);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Lays the map page out for the actual tab surface. The designer used
    ///     fixed 384 px side panel coordinates; re-flowing both columns keeps the
    ///     map and the entity list readable at the running 1300x820 size.
    /// </summary>
    private void ApplyMapLayout()
    {
        if (_applyingMapLayout || IsDisposed || tabControl1 == null || panel1 == null)
            return;

        var scale = DeviceDpi / 96f;
        var pageW = (int)(ClientSize.Width / scale);
        var pageH = (int)(ClientSize.Height / scale);
        if (pageW < 400 || pageH < 300)
            return;

        _applyingMapLayout = true;
        try
        {
            const int margin = 14;
            const int gap = 12;

            var contentW = pageW - margin * 2;
            var contentH = pageH - margin * 2;
            var rightW = Math.Max(320, Math.Min(380, (int)(contentW * 0.34f)));
            var leftW = contentW - rightW - gap;

            Place(tabControl1, margin, margin, leftW, contentH);
            Place(panel1, margin + leftW + gap, margin, rightW, contentH);

            // Right column: filter row, entity list and the two options below it.
            const int filterH = 54;
            const int optionsH = 132;
            var listH = Math.Max(120, contentH - filterH - optionsH);

            Place(panel3, 0, 0, rightW, filterH);
            PlaceLabel(label4, 14, 16, 44, 22, ContentAlignment.MiddleLeft);
            Place(comboViewType, 62, 13, rightW - 76, 26);

            Place(lvMonster, 0, filterH, rightW, listH);
            Place(checkEnableCollisions, 14, filterH + listH + 8, rightW - 28, 28);
            Place(checkBoxAutoSelectUniques, 14, filterH + listH + 40, rightW - 28, 28);
            Place(label3, 14, filterH + listH + 72, rightW - 28, 54);

            colName.Width = Math.Max(110, rightW - 24 - 70 - 50 - 78);
            colType.Width = 70;
            colLevel.Width = 50;
            colPosition.Width = 78;

            // NavMesh tab: canvas plus its bottom toolbar.
            var tabPageW = tabNavMeshViewer.ClientSize.Width;
            var tabPageH = tabNavMeshViewer.ClientSize.Height;
            if (tabPageW <= 0 || scale <= 0)
                tabPageW = Math.Max(1, leftW - 8);
            if (tabPageH <= 0)
                tabPageH = Math.Max(1, contentH - 30);

            var navW = (int)(tabPageW / scale);
            var navH = (int)(tabPageH / scale);

            Place(panelNavMeshRendererCanvas, 0, 0, navW, Math.Max(80, navH - 44));
            Place(panel2, 0, Math.Max(0, navH - 44), navW, 44);
            Place(btnNvmResetToPlayer, 14, 8, 150, 28);
            PlaceLabel(labelSectorInfo, Math.Max(174, navW - 204), 8, 190, 28,
                ContentAlignment.MiddleRight);

            // Theme integration: use the application surfaces instead of the
            // designer's white tab pages and black caption colors.
            panel1.BackColor = ColorScheme.BackColor;
            panel3.BackColor = ColorScheme.BackColor;
            panel2.BackColor = ColorScheme.BackColor;
            mapCanvas.BackColor = ColorScheme.BackColor;
            tabNavMeshViewer.BackColor = ColorScheme.BackColor;
            mapCanvas.Padding = new Padding(0);
            tabNavMeshViewer.Padding = new Padding(0);

            lvMonster.BackColor = ColorScheme.TableBackColor;
            lvMonster.ForeColor = ColorScheme.ForeColor;
            lvMonster.BorderStyle = BorderStyle.None;

            label4.ForeColor = ColorScheme.ForeColor;
            labelSectorInfo.ForeColor = ColorScheme.ForeColor;
            label3.ForeColor = ColorScheme.SecondaryForeColor;
            checkEnableCollisions.ForeColor = ColorScheme.ForeColor;
            checkBoxAutoSelectUniques.ForeColor = ColorScheme.ForeColor;

            ResizeMapBuffer();
        }
        finally
        {
            _applyingMapLayout = false;
        }
    }

    /// <summary>
    ///     Localizes the map page, including the entity type captions and the
    ///     navmesh render settings menu which the generic language walker does
    ///     not reach.
    /// </summary>
    private void ApplyMapLocalization()
    {
        if (IsDisposed || lvMonster == null)
            return;

        mapCanvas.Text = T("RSBot.Map.Tab.Minimap", "Minimap");
        tabNavMeshViewer.Text = T("RSBot.Map.Tab.NavMesh", "NavMesh Viewer");
        label4.Text = T("RSBot.Map.Show", "Show:");

        colName.Text = T("RSBot.Map.Columns.Name", "Name");
        colType.Text = T("RSBot.Map.Columns.Type", "Type");
        colLevel.Text = T("RSBot.Map.Columns.Level", "Level");
        colPosition.Text = T("RSBot.Map.Columns.Position", "Position");

        checkEnableCollisions.Text = T("RSBot.Map.Collision", "Enable collision detection");
        checkBoxAutoSelectUniques.Text = T("RSBot.Map.AutoSelectUniques", "Automatically select uniques");
        label3.Text = T("RSBot.Map.AutoSelectHint",
            "This function does not work while the bot is active.\n If you want the bot to auto attack while active,\n set Avoidance.");
        btnNvmResetToPlayer.Text = T("RSBot.Map.ResetToPlayer", "Reset to player");

        if (Game.Player == null)
            labelSectorInfo.Text = T("RSBot.Map.NotInGame", "Not in game");

        var selected = comboViewType.SelectedIndex;
        comboViewType.BeginUpdate();
        comboViewType.Items.Clear();
        comboViewType.Items.AddRange(new object[]
        {
            T("RSBot.Map.View.Monsters", "Monsters"),
            T("RSBot.Map.View.Players", "Players"),
            T("RSBot.Map.View.Party", "Party"),
            T("RSBot.Map.View.Npcs", "NPCs"),
            T("RSBot.Map.View.Cos", "COS"),
            T("RSBot.Map.View.Portals", "Portals"),
            T("RSBot.Map.View.All", "All")
        });
        comboViewType.EndUpdate();
        comboViewType.SelectedIndex = selected >= 0 && selected < comboViewType.Items.Count
            ? selected
            : 6;

        _navMeshRenderer?.ApplyLocalization(T);
    }

    private static string TranslateRarity(string rarity)
    {
        if (string.IsNullOrWhiteSpace(rarity))
            return rarity;

        switch (rarity.Trim())
        {
            case "General":
                return T("RSBot.Map.Rarity.General", rarity);
            case "Champion":
                return T("RSBot.Map.Rarity.Champion", rarity);
            case "Unique":
                return T("RSBot.Map.Rarity.Unique", rarity);
            case "Giant":
                return T("RSBot.Map.Rarity.Giant", rarity);
            case "Titan":
                return T("RSBot.Map.Rarity.Titan", rarity);
            case "Elite":
                return T("RSBot.Map.Rarity.Elite", rarity);
            case "Event":
                return T("RSBot.Map.Rarity.Event", rarity);
            case "Elite (Strong)":
                return T("RSBot.Map.Rarity.EliteStrong", rarity);
            case "General (Party)":
                return T("RSBot.Map.Rarity.GeneralParty", rarity);
            case "Champion (Party)":
                return T("RSBot.Map.Rarity.ChampionParty", rarity);
            case "Unique (Party)":
                return T("RSBot.Map.Rarity.UniqueParty", rarity);
            case "Giant (Party)":
                return T("RSBot.Map.Rarity.GiantParty", rarity);
            case "Titan (Party)":
                return T("RSBot.Map.Rarity.TitanParty", rarity);
            case "Elite (Party)":
                return T("RSBot.Map.Rarity.EliteParty", rarity);
            case "Unknown":
                return T("RSBot.Map.Rarity.Unknown", rarity);
            default:
                return rarity;
        }
    }

    /// <summary>
    ///     Draws a themed placeholder in the minimap tab while there is no
    ///     logged-in player, instead of showing the small stale render buffer.
    /// </summary>
    private void DrawMapPlaceholder(Graphics graphics)
    {
        var rect = mapCanvas.ClientRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        using (var background = new SolidBrush(ColorScheme.BackColor))
            graphics.FillRectangle(background, rect);

        var dark = ColorScheme.BackColor.IsDark();
        using (var gridPen = new Pen(Color.FromArgb(dark ? 26 : 16, ColorScheme.ForeColor)))
        {
            for (var x = 64; x < rect.Width; x += 64)
                graphics.DrawLine(gridPen, x, 0, x, rect.Height);

            for (var y = 64; y < rect.Height; y += 64)
                graphics.DrawLine(gridPen, 0, y, rect.Width, y);
        }

        var centerX = rect.Width / 2;
        var centerY = rect.Height / 2 - 40;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var accentPen = new Pen(ColorScheme.AccentColor, 2))
        using (var dotBrush = new SolidBrush(ColorScheme.AccentColor))
        {
            graphics.DrawEllipse(accentPen, centerX - 30, centerY - 30, 60, 60);
            graphics.FillEllipse(dotBrush, centerX - 4, centerY - 4, 8, 8);
            graphics.DrawLine(accentPen, centerX - 46, centerY, centerX - 34, centerY);
            graphics.DrawLine(accentPen, centerX + 34, centerY, centerX + 46, centerY);
            graphics.DrawLine(accentPen, centerX, centerY - 46, centerX, centerY - 34);
            graphics.DrawLine(accentPen, centerX, centerY + 34, centerX, centerY + 46);
        }

        using var titleFont = new Font(Font.FontFamily, 13.5F, FontStyle.Bold);
        using var hintFont = new Font(Font.FontFamily, 9F, FontStyle.Regular);

        var titleRect = new Rectangle(24, centerY + 48, Math.Max(1, rect.Width - 48), 32);
        TextRenderer.DrawText(graphics, T("RSBot.Map.Placeholder.Title", "Not in game"), titleFont,
            titleRect, ColorScheme.ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var hintRect = new Rectangle(24, centerY + 80, Math.Max(1, rect.Width - 48), 48);
        TextRenderer.DrawText(graphics,
            T("RSBot.Map.Placeholder.Hint", "The mini-map will appear after you log in."),
            hintFont, hintRect, ColorScheme.SecondaryForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak |
            TextFormatFlags.NoPrefix);
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
    private void AddGridItem(string name, string type, byte level, Position position)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = T("NoName", "<No name>");

        var item = new ListViewItem(name);
        item.SubItems.Add(type);
        item.SubItems.Add(level.ToString());
        item.SubItems.Add(position.ToString());
        lvMonster.Items.Add(item);
    }

    /// <summary>
    ///     Draws the point at.
    /// </summary>
    private void DrawPointAt(Graphics gfx, Position position, int entityIndex)
    {
        var distanceToPosition = position.DistanceToPlayer();
        if (distanceToPosition > 150)
            return;

        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

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
        var distanceToPosition = position.DistanceToPlayer();
        if (distanceToPosition > 150)
            return;

        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

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
        var distanceToPositionA = source.DistanceToPlayer();
        var distanceToPositionB = destination.DistanceToPlayer();
        if (distanceToPositionA > 150 || distanceToPositionB > 150)
            return;

        var srcX = GetMapX(source);
        var srcY = GetMapY(source);
        var destinationX = GetMapX(destination);
        var destinationY = GetMapY(destination);

        gfx.DrawLine(color, srcX, srcY, destinationX, destinationY);
    }

    private void DrawCircleAt(Graphics gfx, Position position, Color color, int diameter)
    {
        var distanceToPosition = position.DistanceToPlayer();
        if (distanceToPosition > 150)
            return;

        try
        {
            var x = GetMapX(position);
            var y = GetMapY(position);

            using var brush = new SolidBrush(color);

            var diameterF = diameter * _scale;
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
    private void PopulateMapAndGrid(Graphics graphics)
    {
        lvMonster.BeginUpdate();
        lvMonster.Items.Clear();

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
                        AddGridItem(entry.Record.GetRealName(), TranslateRarity(entry.Rarity.GetName()),
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
                            AddGridItem(entry.Name, T("RSBot.Map.Entity.Pet", "Pet"), entry.Record.Level, entry.Movement.Source);
                            DrawPointAt(graphics, entry.Movement.Source, 1);
                        }

            if (comboViewType.SelectedIndex == 2 || comboViewType.SelectedIndex == 6)
                if (Game.Party != null && Game.Party.Members != null)
                    foreach (var member in Game.Party.Members.ToArray())
                    {
                        if (Game.Player.Position.DistanceTo(member.Position) > 50)
                            continue;

                        if (member.Name == Game.Player.Name)
                            continue;

                        DrawPointAt(graphics, member.Position, 6);
                        AddGridItem(member.Name, T("RSBot.Map.Entity.PartyMember", "Party member"), member.Level, member.Position);
                    }

            if (comboViewType.SelectedIndex == 1 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedPlayer>(out var players))
                    foreach (var entry in players)
                    {
                        if (Game.Party != null && Game.Party.Members != null &&
                            Game.Party.GetMemberByName(entry.Name) != null)
                            return;

                        AddGridItem(entry.Name, T("RSBot.Map.Entity.Player", "Player"), 0, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 3);
                    }

            if (comboViewType.SelectedIndex == 3 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedNpcNpc>(out var npcs))
                    foreach (var entry in npcs)
                    {
                        AddGridItem(entry.Record.GetRealName(), T("RSBot.Map.Entity.Npc", "NPC"),
                            entry.Record.Level, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 2);
                    }

            if (comboViewType.SelectedIndex == 5 || comboViewType.SelectedIndex == 6)
                if (SpawnManager.TryGetEntities<SpawnedPortal>(out var portals))
                    foreach (var entry in portals)
                    {
                        AddGridItem(entry.Record.GetRealName(), T("RSBot.Map.Entity.Teleport", "Teleport"), 0, entry.Movement.Source);
                        DrawPointAt(graphics, entry.Movement.Source, 7);
                    }
        }
        catch (Exception ex)
        {
            Log.Debug($"[Map] Render error: {ex.Message}");
        }

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
        if (bufferedGraphicsContext.MaximumBuffer.Width != mapCanvas.Width + 1)
        {
            bufferedGraphicsContext.MaximumBuffer = mapCanvas.Size;

            bufferedGraphics = bufferedGraphicsContext.Allocate(mapCanvas.CreateGraphics(), mapCanvas.ClientRectangle);
        }

        // Set layer path & sectors
        var p = Game.Player.Movement.Source;

        var tempX = p.Region.X;
        var tempY = p.Region.Y;

        if (p.Region.IsDungeon)
        {
            tempX = p.GetSectorFromOffset(p.XOffset);
            tempY = p.GetSectorFromOffset(p.YOffset);
        }

        if (tempX == _currentXSec && tempY == _currentYSec)
            return;

        _currentXSec = tempX;
        _currentYSec = tempY;

        if (_cachedImages.Count >= 25)
            _cachedImages.Clear();

        try
        {
            _currentSectorGraphic = new Bitmap(SectorSize * 3, SectorSize * 3, PixelFormat.Format32bppArgb);

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

                _regionName = roomNameTranslated;
                dungeonName = RegionInfoManager.GetDungeonName(p.Region);
            }
            else
                _regionName = Game.ReferenceManager.GetTranslation(Game.Player.Position.Region.ToString());

            for (byte x = 0; x < GridSize; x++)
            {
                for (byte z = 0; z < GridSize; z++)
                {
                    var xSector = (byte)(_currentXSec + x - 1);
                    var ySector = (byte)(_currentYSec + z - 1);

                    var sectorImgName = GetMinimapFileName(new Region(xSector, ySector), dungeonName, floorName);
                    using var bitmap = LoadSectorImage(sectorImgName);
                    var pos = new Point(bitmap.Width * x, bitmap.Height * (GridSize - 1 - z));

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
        if (!string.IsNullOrWhiteSpace(dungeonName) && !string.IsNullOrWhiteSpace(floorName))
        {
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

    private void DrawObjects(Graphics graphics)
    {
        if (_currentSectorGraphic != null)
        {
            graphics.InterpolationMode = InterpolationMode.Bicubic;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;

            PointF point = new()
            {
                X = mapCanvas.Width / 2f - SectorSize - Game.Player.Movement.Source.XSectorOffset / 10f * _scale,
                Y = mapCanvas.Height / 2f - SectorSize * 2f + Game.Player.Movement.Source.YSectorOffset / 10f * _scale
            };

            graphics.DrawImage(_currentSectorGraphic, point);

            PopulateMapAndGrid(graphics);
            DrawPointAt(graphics, Game.Player.Movement.Source, 0);
        }
    }

    private void trmInterval_Tick(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        if (!Visible)
            return;

        if (tabControl1.SelectedTab == tabNavMeshViewer && Game.Player != null)
        {
            if (Game.Player.Position.TryGetNavMeshTransform(out var playerTransform))
                _navMeshRenderer?.Update(playerTransform);
        }

        labelSectorInfo.Text =
            $"{Game.Player.Movement.Source.Region} ({Game.Player.Movement.Source.Region.X}x{Game.Player.Movement.Source.Region.Y})";

        bufferedGraphics.Graphics.Clear(Color.Black);
        RedrawMap();
        DrawObjects(bufferedGraphics.Graphics);

        using var font = new Font(Font, FontStyle.Bold);

        var text = Game.Player.Position.ToString();
        var measuredText = bufferedGraphics.Graphics.MeasureString(text, font);

        var x = mapCanvas.Width - measuredText.Width;

        bufferedGraphics.Graphics.DrawString(_regionName, font, Brushes.Black, 12, 7);
        bufferedGraphics.Graphics.DrawString(_regionName, font, Brushes.White, 10, 5);

        bufferedGraphics.Graphics.DrawString(text, font, Brushes.Black, x - 8, 21 - measuredText.Height / 2);
        bufferedGraphics.Graphics.DrawString(text, font, Brushes.White, x - 10, 20 - measuredText.Height / 2);

        mapCanvas.Invalidate();
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
        return mapCanvas.Width / 2f + (gamePosition.X - Game.Player.Movement.Source.X) * _scale;
    }

    private float GetMapY(Position gamePosition)
    {
        return mapCanvas.Height / 2f + (gamePosition.Y - Game.Player.Movement.Source.Y) * _scale * -1.0f;
    }

    private void mapCanvas_MouseClick(object sender, MouseEventArgs e)
    {
        if(!Game.Ready) return;

        var position = Game.Player.Movement.Source;
        position.XOffset = Game.Player.Movement.Source.XOffset +
                           (mapCanvas.Width / 2f - e.X) / SectorSize * 192f * 10 * -1f;
        position.YOffset = Game.Player.Movement.Source.YOffset + (mapCanvas.Height / 2f - e.Y) / SectorSize * 192f * 10;

        Game.Player.MoveTo(position, false);
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
        if (Game.Player != null && Game.Player.Position.TryGetNavMeshTransform(out var playerTransform))
            _navMeshRenderer?.Update(playerTransform, true);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabControl1.SelectedTab == tabNavMeshViewer && Game.Player != null)
        {
            if (Game.Player.Position.TryGetNavMeshTransform(out var playerTransform))
                _navMeshRenderer?.Update(playerTransform, true);
        }

        ApplyMapLayout();
    }

    private void tabMinimap_Paint(object sender, PaintEventArgs e)
    {
        if (Game.Player == null)
        {
            DrawMapPlaceholder(e.Graphics);
            return;
        }

        bufferedGraphics?.Render();
    }
}