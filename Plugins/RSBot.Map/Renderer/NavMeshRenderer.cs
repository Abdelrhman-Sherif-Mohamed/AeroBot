using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.NavMeshApi;
using RSBot.NavMeshApi.Dungeon;
using RSBot.NavMeshApi.Mathematics;
using RSBot.NavMeshApi.Object;
using RSBot.NavMeshApi.Terrain;
using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace RSBot.Map.Renderer;

public partial class NavMeshRenderer : DoubleBufferedControl
{
    private bool _isDragging;
    private Point _dragPosition;

    private NavMeshTransform _transform;
    private NavMeshTransform _mouseTransform;
    private NavMeshRaycastHit _hit = null;

    private readonly Font _font;
    private readonly Font _smallFont;

    private float _zoom = 1.0f;

    private bool _drawRegionId = true;
    private bool _drawRegionBorder = true;
    private bool _drawTerrainGlobalEdges = true;
    private bool _drawTerrainInternalEdges = true;
    private bool _drawObjectGlobalEdges = true;
    private bool _drawObjectInternalEdges = true;
    private bool _drawObjectGround = true;

    private bool _drawTerrainCellID = false;
    private bool _drawTerrainGlobalEdgeID = false;
    private bool _drawTerrainInternalEdgeID = false;
    private bool _drawObjectCellID = false;
    private bool _drawObjectGlobalEdgeID = false;
    private bool _drawObjectInternalEdgeID = false;

    private bool _raycastVisualizer = false;

    public NavMeshRenderer()
    {
        this.InitializeComponent();

        if (this.DesignMode)
            return;

        _font = new Font("Arial", 64f);
        _smallFont = new Font("Arial", 32f);
        _transform = new NavMeshTransform(Vector3.Zero);
        _mouseTransform = new NavMeshTransform(Vector3.Zero);
    }

    public void Update(NavMeshTransform transform, bool force = false)
    {
        if (!force && _transform != null && transform.Position.Distance(_transform.Position) < 0.5f)
            return;

        _transform = transform;
        _mouseTransform = new NavMeshTransform(Vector3.Zero);

        this.Invalidate();
    }

    /// <summary>
    ///     Translates the render-settings context menu.
    /// </summary>
    public void ApplyLocalization(Func<string, string, string> translate)
    {
        if (translate == null)
            return;

        displayToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.Display", "Display");
        regionIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.RegionId", "RegionID");
        regionBorderToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.RegionBorder", "RegionBorder");
        terrainToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.Terrain", "Terrain");
        terrainCellIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.CellId", "CellID");
        terrainGlobalEdgesToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.GlobalEdges", "GlobalEdges");
        terrainGlobalEdgeIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.GlobalEdgeId", "GlobalEdgeID");
        terrainInternalEdgesToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.InternalEdges", "InternalEdges");
        terrainInternalEdgeIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.InternalEdgeId", "InternalEdgeID");
        objectsToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.Objects", "Objects");
        objectCellIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.CellId", "CellID");
        objectGroundToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.Ground", "Ground");
        objectGlobalEdgesToolStripMenuItem1.Text = translate("RSBot.Map.NavMesh.GlobalEdges", "GlobalEdges");
        objectGlobalEdgeIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.GlobalEdgeId", "GlobalEdgeID");
        objectInternalEdgesToolStripMenuItem1.Text = translate("RSBot.Map.NavMesh.InternalEdges", "InternalEdges");
        objectInternalEdgeIDToolStripMenuItem.Text = translate("RSBot.Map.NavMesh.InternalEdgeId", "InternalEdgeID");
        raycastMenuItem.Text = translate("RSBot.Map.NavMesh.Raycast", "Raycast visualizer");
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(SDUI.ColorScheme.BackColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_transform == null || _transform.Region == 0u)
        {
            var gEmpty = e.Graphics;

            using var emptyBrush = new SolidBrush(Color.FromArgb(160, 175, 200));
            using var emptyFont = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            using var emptyFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var text = LanguageManager.GetLangBySpecificKey("RSBot.Map", "RSBot.Map.NavMesh.Empty",
                "NavMesh Viewer - waiting for the game to align and render the 3D navmesh...");
            gEmpty.DrawString(text, emptyFont, emptyBrush,
                new System.Drawing.RectangleF(24, 24, Math.Max(1, Width - 48), Math.Max(1, Height - 48)), emptyFormat);
            return;
        }

        var g = e.Graphics;

        float uniformScale = Math.Max(0.01f, (Math.Min(this.Width, this.Height) / 1920.0f) * _zoom);

        var matrix = new Matrix();
        matrix.Translate(this.Width / 2.0f, this.Height / 2.0f);
        matrix.Scale(uniformScale, uniformScale);
        matrix.Translate(-_transform.Offset.X, -_transform.Offset.Z);
        g.MultiplyTransform(matrix);

        var set = new HashSet<int>();

        if (_transform.Region.IsDungeon)
        {
            if (!NavMeshManager.TryGetNavMeshDungeon(_transform.Region, out var dungeon))
                return;

            this.DrawNavMeshDungeon(set, g, dungeon);
        }
        else
        {
            for (int rz = _transform.Region.Z - 1; rz < _transform.Region.Z + 2; rz++)
            {
                for (int rx = _transform.Region.X - 1; rx < _transform.Region.X + 2; rx++)
                {
                    var rid = new RID((byte)rx, (byte)rz);
                    if (!NavMeshManager.TryGetNavMeshTerrain(rid, out var terrain))
                        continue;

                    foreach (var edge in terrain.GlobalEdges)
                        edge.Link();

                    this.DrawTerrain(set, g, terrain);
                }
            }
        }

        // Draw destination path if player has an active destination
        if (RSBot.Core.Game.Player?.Movement.HasDestination == true)
        {
            var dest = RSBot.Core.Game.Player.Movement.Destination;
            if (dest.TryGetNavMeshTransform(out var destTransform))
            {
                var localDestOffset = RID.Transform(destTransform.Offset, _transform.Region, destTransform.Region);
                using var destPen = new Pen(Color.LimeGreen, 3f / uniformScale);
                g.DrawLine(destPen, _transform.Offset.X, _transform.Offset.Z, localDestOffset.X, localDestOffset.Z);

                float dRadius = 10f;
                using var destBrush = new SolidBrush(Color.LimeGreen);
                g.FillEllipse(destBrush, localDestOffset.X - dRadius, localDestOffset.Z - dRadius, dRadius * 2, dRadius * 2);
            }
        }


        // Draw Player Marker on NavMesh
        float pRadius = 12f;
        using var playerBrush = new SolidBrush(Color.DeepSkyBlue);
        using var playerBorder = new Pen(Color.White, 2.5f / uniformScale);
        g.FillEllipse(playerBrush, _transform.Offset.X - pRadius, _transform.Offset.Z - pRadius, pRadius * 2, pRadius * 2);
        g.DrawEllipse(playerBorder, _transform.Offset.X - pRadius, _transform.Offset.Z - pRadius, pRadius * 2, pRadius * 2);

        // Player heading ray
        if (RSBot.Core.Game.Player?.Movement != null)
        {
            float angle = RSBot.Core.Game.Player.Movement.Angle;
            float headLen = 28f;
            float hx = _transform.Offset.X + MathF.Sin(angle) * headLen;
            float hz = _transform.Offset.Z - MathF.Cos(angle) * headLen;
            using var headPen = new Pen(Color.Yellow, 3f / uniformScale);
            g.DrawLine(headPen, _transform.Offset.X, _transform.Offset.Z, hx, hz);
        }

        var localMouseOffset = RID.Transform(_mouseTransform.Offset, _transform.Region, _mouseTransform.Region);
        if (_raycastVisualizer)
        {
            g.DrawLine(Pens.White, _transform.Offset.X, _transform.Offset.Z, localMouseOffset.X, localMouseOffset.Z);
            if (_hit != null)
            {
                var localHitOffset = RID.Transform(_hit.Position, _transform.Region, _hit.Region);
                g.DrawLine(Pens.Red, _transform.Offset.X, _transform.Offset.Z, localHitOffset.X, localHitOffset.Z);
            }
        }

        matrix.Invert();
        g.MultiplyTransform(matrix);

        var textBrush = new SolidBrush(SDUI.ColorScheme.ForeColor);

        g.DrawString($"Player: {_transform}", DefaultFont, textBrush, 0, 0);
        g.DrawString($"Cursor: {_mouseTransform}", DefaultFont, textBrush, 0, 12);
        if (_hit != null)
            g.DrawString($"Hit: {_hit}", DefaultFont, Brushes.Red, 0, 24);
    }

    private void DrawNavMeshDungeon(HashSet<int> set, Graphics g, NavMeshDungeon dungeon)
    {
        if (_transform.Instance is not NavMeshInstBlock currentBlock)
            return;

        foreach (var block in dungeon.Blocks.Where(b => b.FloorIndex == currentBlock.FloorIndex))
        {
            var blockMatrix = new Matrix();
            blockMatrix.RotateAt(block.Yaw * (180.0f / MathF.PI), block.LocalPosition.ToPointF());
            blockMatrix.Translate(block.LocalPosition.X, block.LocalPosition.Z);

            g.MultiplyTransform(blockMatrix);

            this.DrawNavMeshObj(g, block.ID, block.NavMeshObj);

            blockMatrix.Invert();
            g.MultiplyTransform(blockMatrix);

            set.Add(block.ID);

            foreach (var obj in block.ObjectList)
                g.DrawCircle(Pens.Red, obj.Circle);
        }
    }

    private void DrawTerrain(HashSet<int> set, Graphics g, NavMeshTerrain terrain)
    {
        var dx = (terrain.Region.X - _transform.Region.X) * RID.Width;
        var dz = (terrain.Region.Z - _transform.Region.Z) * RID.Length;

        var matrix = new Matrix();
        matrix.Translate(dx, dz);

        g.MultiplyTransform(matrix);

        if (_drawRegionBorder)
            g.DrawRectangle(Pens.Magenta, 0, 0, 1920 - 1, 1920 - 1);

        if (_drawTerrainCellID)
        {
            foreach (var cell in terrain.Cells)
                g.DrawString($"{cell}", _smallFont, Brushes.White, cell.RectangleF.Center.ToPointF());
        }

        if (_drawTerrainGlobalEdges)
        {
            foreach (var edge in terrain.GlobalEdges)
            {
                var pen = edge.Flag.ToPen();
                g.DrawLine(pen, edge.Line);

                if (_drawTerrainGlobalEdgeID)
                    g.DrawString($"{edge}", _smallFont, Brushes.White, edge.Line.Center.ToPointF());
            }
        }

        if (_drawTerrainInternalEdges)
        {
            foreach (var edge in terrain.InternalEdges)
            {
                var pen = edge.Flag.ToPen();
                g.DrawLine(pen, edge.Line);

                if (_drawTerrainInternalEdgeID)
                    g.DrawString($"{edge}", _smallFont, Brushes.White, edge.Line.Center.ToPointF());
            }
        }

        foreach (var obj in terrain.Instances)
        {
            if (set.Contains(obj.WorldUID))
                continue;

            var objMatrix = new Matrix();
            objMatrix.RotateAt(obj.Yaw * (180.0f / MathF.PI), obj.LocalPosition.ToPointF());
            objMatrix.Translate(obj.LocalPosition.X, obj.LocalPosition.Z);

            g.MultiplyTransform(objMatrix);

            this.DrawNavMeshObj(g, obj.WorldUID, obj.NavMeshObj);

            objMatrix.Invert();
            g.MultiplyTransform(objMatrix);

            set.Add(obj.WorldUID);
        }

        if (_drawRegionId)
            g.DrawString($"{terrain.Region}", _font, Brushes.White, 0, 0);

        matrix.Invert();
        g.MultiplyTransform(matrix);
    }

    private void DrawNavMeshObj(Graphics g, int id, NavMeshObj obj)
    {
        if (_drawObjectGround)
        {
            var brush = new SolidBrush(id.ToColor());
            foreach (var cell in obj.Cells)
            {
                g.FillTriangleF(brush, cell.Triangle);

                if (_drawObjectCellID)
                {
                    string label;
                    if (cell.EventZone == NavMeshEventZone.Empty)
                        label = $"{cell}";
                    else
                        label = $"{cell} [{obj.Events[cell.EventZone.Index]}, {cell.EventZone.Flag}]";
                    g.DrawString(label, _smallFont, Brushes.White, cell.Triangle.Center.ToPointF());
                }
            }
        }

        if (_drawObjectGlobalEdges)
        {
            foreach (var edge in obj.GlobalEdges)
            {
                g.DrawLine(edge.Flag.ToPen(), edge.Line);

                if (_drawObjectGlobalEdgeID)
                {
                    string label;
                    if (edge.EventZone == NavMeshEventZone.Empty)
                        label = $"{edge}";
                    else
                        label = $"{edge} [{obj.Events[edge.EventZone.Index]}, {edge.EventZone.Flag}]";
                    g.DrawString(label, _smallFont, Brushes.White, edge.Line.Center.ToPointF());
                }

            }

        }

        if (_drawObjectInternalEdges)
        {
            foreach (var edge in obj.InternalEdges)
            {
                g.DrawLine(edge.Flag.ToPen(), edge.Line);

                if (_drawObjectInternalEdgeID)
                {
                    string label;
                    if (edge.EventZone == NavMeshEventZone.Empty)
                        label = $"{edge}";
                    else
                        label = $"{edge} [{obj.Events[edge.EventZone.Index]}, {edge.EventZone.Flag}]";
                    g.DrawString(label, _smallFont, Brushes.White, edge.Line.Center.ToPointF());
                }
            }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (_raycastVisualizer)
            return;

        _zoom += e.Delta * 0.001f;

        if (_zoom < 0.5)
            _zoom = 0.5f;

        if (_zoom > 1.5)
            _zoom = 2;

        this.Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_transform == null || _transform.Region == 0u)
            return;

        float uniformScale = Math.Max(0.01f, (Math.Min(this.Width, this.Height) / 1920.0f) * _zoom);

        if (_isDragging)
        {
            var dx = _dragPosition.X - e.X;
            var dy = _dragPosition.Y - e.Y;
            _dragPosition = e.Location;

            _transform.Offset += new Vector3(dx / uniformScale, 0, dy / uniformScale);
            _transform.Normalize();
            _hit = null;
        }

        float worldMouseX = _transform.Offset.X + (e.X - this.Width / 2.0f) / uniformScale;
        float worldMouseZ = _transform.Offset.Z + (e.Y - this.Height / 2.0f) / uniformScale;

        _mouseTransform.Region = _transform.Region;
        _mouseTransform.Offset = new Vector3(worldMouseX, 0.0f, worldMouseZ);
        _mouseTransform.Normalize();

        NavMeshManager.ResolveCellAndHeight(_transform);
        NavMeshManager.ResolveCellAndHeight(_mouseTransform);

        this.Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left && _raycastVisualizer)
        {
            try
            {
                this.Raycast();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(),
                    LanguageManager.GetLangBySpecificKey("RSBot.Map", "RSBot.Map.Exception", "Exception"),
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        if (e.Button == MouseButtons.Right)
            contextRenderSettings.Show(this, e.Location);

        if (e.Button == MouseButtons.Middle)
        {
            _dragPosition = e.Location;
            _isDragging = true;
        }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Middle)
            _isDragging = false;
    }

    private void Raycast()
    {
        _hit = null;
        var source = new NavMeshTransform(_transform);
        var destination = new NavMeshTransform(_mouseTransform);

        if (!NavMeshManager.Raycast(source, destination, NavMeshRaycastType.Move, out NavMeshRaycastHit hit) && hit != null)
        {
            _hit = hit;
            return;
        }
    }

    private void regionIDToolStripMenuItem_CheckedChange(object sender, EventArgs e)
    {
        _drawRegionId = regionIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void regionBorderToolStripMenuItem_CheckedChange(object sender, EventArgs e)
    {
        _drawRegionBorder = regionBorderToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void globalEdgesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        _drawTerrainGlobalEdges = terrainGlobalEdgesToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void internalEdgesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        _drawTerrainInternalEdges = terrainInternalEdgesToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void globalEdgesToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
    {
        _drawObjectGlobalEdges = objectGlobalEdgesToolStripMenuItem1.Checked;
        this.Invalidate();
    }

    private void internalEdgesToolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
    {
        _drawObjectInternalEdges = objectInternalEdgesToolStripMenuItem1.Checked;
        this.Invalidate();
    }

    private void groundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        _drawObjectGround = objectGroundToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void terrainCellIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawTerrainCellID = terrainCellIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void terrainGlobalEdgeIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawTerrainGlobalEdgeID = terrainGlobalEdgeIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void terrainInternalEdgeIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawTerrainInternalEdgeID = terrainInternalEdgeIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void objectCellIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawObjectCellID = objectCellIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void objectGlobalEdgeIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawObjectGlobalEdgeID = objectGlobalEdgeIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void objectInternalEdgeIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _drawObjectInternalEdgeID = objectInternalEdgeIDToolStripMenuItem.Checked;
        this.Invalidate();
    }

    private void raycastMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        _raycastVisualizer = raycastMenuItem.Checked;
        _zoom = 1;

        this.Invalidate();
    }
}
