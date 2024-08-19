using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using GMTK24.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24.Model;

public class Structure
{
    private readonly List<Cell> _cellsNeedingSupport = new();
    private readonly List<Cell> _cellsProvidingSupport = new();
    private readonly List<Cell> _occupiedWorldSpace = new();
    private readonly List<Cell> _scaffoldAnchorPoints = new();
    public bool IsVisible { get; private set; } = true;

    public Structure(Cell worldCenter, StructurePlan plan, Blueprint blueprint)
    {
        Settings = plan.Settings;
        Blueprint = blueprint;

        ApplyCells(worldCenter, plan.OccupiedCells, _occupiedWorldSpace);
        ApplyCells(worldCenter, plan.ScaffoldAnchorPoints, _scaffoldAnchorPoints);
        ApplyCells(worldCenter, plan.ProvidesStructureCells, _cellsProvidingSupport);
        ApplyCells(worldCenter, plan.RequiresSupportCells, _cellsNeedingSupport);

        Center = worldCenter;
    }

    public PlanSettings Settings { get; }
    public Blueprint Blueprint { get; }
    public IEnumerable<Cell> OccupiedCells => _occupiedWorldSpace;
    public IEnumerable<Cell> ScaffoldAnchorPoints => _scaffoldAnchorPoints;
    public IEnumerable<Cell> CellsProvidingSupport => _cellsProvidingSupport;
    public IEnumerable<Cell> CellsNeedingSupport => _cellsNeedingSupport;
    public Cell Center { get; }
    public float Lifetime { get; set; }

    private static void ApplyCells(Cell worldCenter, HashSet<Cell> localCells, List<Cell> occupiedWorldSpace)
    {
        foreach (var localCell in localCells)
        {
            occupiedWorldSpace.Add(worldCenter + localCell);
        }
    }

    public Vector2 PixelCenter()
    {
        var average = Vector2.Zero;
        foreach (var cell in _occupiedWorldSpace)
        {
            average += Grid.CellToPixel(cell);
        }

        return average / _occupiedWorldSpace.Count;
    }

    public void Hide()
    {
        IsVisible = false;
        VisibilityChanged?.Invoke();
    }

    public void Show()
    {
        Lifetime = 0;
        IsVisible = true;
        VisibilityChanged?.Invoke();
    }

    public event Action? VisibilityChanged;
    
    
    public static void DrawStructure(Painter painter, Structure structure, float lifetime)
    {
        var textureName = structure.Settings.DrawDescription.TextureName;
        var frameNames = structure.Settings.DrawDescription.FrameNames;
        if (textureName == null && frameNames == null)
        {
            return;
        }

        if (!structure.IsVisible)
        {
            return;
        }

        var animationDuration = 0.25f;
        var scaleVector = Vector2.One;

        Texture2D? texture = null;
        
        if (textureName != null)
        {
            texture = ResourceAssets.Instance.Textures[textureName];
        }

        if (frameNames != null)
        {
            var framesPerSecond = 12;
            var normalizedFrame = (lifetime * framesPerSecond) % frameNames.Count;
            var frameIndex = (int) normalizedFrame;
            texture = ResourceAssets.Instance.Textures[frameNames[frameIndex]];
        }

        if (texture != null)
        {
            var graphicsTopLeft = structure.Center + structure.Settings.DrawDescription.GraphicTopLeft;
            var originOffset = new Vector2(texture.Width / 2f, texture.Height);
            var origin = new DrawOrigin(originOffset);

            if (structure.Lifetime < animationDuration)
            {
                var oscillationsPerSecond = 40;
                var intensity = 0.25f;
                var wiggle = MathF.Sin(structure.Lifetime * oscillationsPerSecond) *
                             Math.Max(0, animationDuration - structure.Lifetime) * intensity;

                scaleVector = new Vector2(1 - wiggle, 1 + wiggle);
            }

            painter.DrawAtPosition(texture,
                Grid.CellToPixel(graphicsTopLeft) + originOffset, new Scale2D(scaleVector),
                new DrawSettings {Depth = Depth.Front - structure.Center.Y, Origin = origin});
        }
    }
}
