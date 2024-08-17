using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace GMTK24;

public class GameSession : ISession
{
    private readonly Camera _camera;
    private readonly HoverState _isHovered = new();
    private readonly Ui _ui;
    private readonly World _world;
    private bool _isPanning;
    private Vector2? _mousePosition;

    public GameSession()
    {
        var layoutBuilder = new UiLayoutBuilder();

        var house = ReadPlan("house.json");
        var house2 = ReadPlan("house2.json");
        var house3 = ReadPlan("house3.json");
        var tree =  ReadPlan("tree.json");
        var platform = ReadPlan("platform.json");

        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(new List<PlannedStructure> {house, house2, house3})));
        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(new List<PlannedStructure> {tree})));
        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(new List<PlannedStructure> {platform})));
        _ui = layoutBuilder.Build();

        var screenSize = new Point(1920, 1080);
        var zoomLevel = 0.25f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
        _world = new World();

        _world.MainLayer.AddStructure(new Cell(0, 0), platform);
    }

    private static PlannedStructure ReadPlan(string planFileName)
    {
        var planFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource/plans");
        var result = JsonConvert.DeserializeObject<PlannedStructure>(planFiles.ReadFile(planFileName));

        if (result == null)
        {
            throw new Exception($"Deserialize failed for {planFileName}");
        }

        return result;
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var worldLayer = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Back);
        worldLayer.AddZone(_camera.ViewBounds, Depth.Back, _isHovered);

        if (_isHovered)
        {
            _mousePosition = input.Mouse.Position(hitTestStack.WorldMatrix * _camera.ScreenToCanvas);

            if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
            {
                input.Mouse.Consume(MouseButton.Left);

                var plannedBuildPosition = GetPlannedBuildPosition();
                var plannedStructure = GetPlannedStructure();

                if (plannedBuildPosition.HasValue && plannedStructure != null)
                {
                    _world.MainLayer.AddStructure(plannedBuildPosition.Value, plannedStructure);
                    _ui.State.IncrementSelectedBlueprint();
                }
            }

            _isPanning = input.Mouse.GetButton(MouseButton.Middle).IsDown;
            if (_isPanning)
            {
                var delta = input.Mouse.Delta(hitTestStack.WorldMatrix * _camera.ScreenToCanvas);
                _camera.CenterPosition -= delta;
            }

            var scrollDelta = input.Mouse.ScrollDelta();

            if (scrollDelta != 0 && !_isPanning)
            {
                var normalizedScrollDelta = scrollDelta / 120f;

                var zoomStrength = 20;

                var previousCameraCenter = _camera.CenterPosition;

                if (normalizedScrollDelta < 0)
                {
                    if (_camera.ViewBounds.Width < 1920)
                    {
                        _camera.ZoomOutFrom((int) (normalizedScrollDelta * -zoomStrength), _mousePosition.Value);
                    }
                }
                else
                {
                    if (_camera.ViewBounds.Width > 192)
                    {
                        _camera.ZoomInTowards((int) (normalizedScrollDelta * zoomStrength), _mousePosition.Value);
                    }
                }
            }
        }
        else
        {
            _mousePosition = null;
            _isPanning = false;
        }

        _ui.UpdateInput(input, hitTestStack);
    }

    public void Draw(Painter painter)
    {
        painter.Clear(Color.SkyBlue);
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var scaffoldCell in _world.MainLayer.ScaffoldCells())
        {
            DrawScaffold(painter, scaffoldCell);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var structure in _world.MainLayer.Structures)
        {
            DrawStructure(painter, structure);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        if (_mousePosition.HasValue && !_isPanning)
        {
            var buildPosition = GetPlannedBuildPosition();
            var structure = GetPlannedStructure();

            if (structure != null && buildPosition.HasValue)
            {
                foreach (var cell in structure.BuildReal(buildPosition.Value).OccupiedCells)
                {
                    var rectangle = Grid.CellToPixelRectangle(cell);
                    var color = Color.Yellow;
                    if (_world.MainLayer.IsOccupiedAt(cell))
                    {
                        color = Color.Red;
                    }

                    painter.DrawRectangle(rectangle, new DrawSettings {Color = color});
                }
            }
        }

        painter.EndSpriteBatch();

        _ui.Draw(painter);
    }

    public void Update(float dt)
    {
    }

    private void DrawScaffold(Painter painter, Cell anchorPoint)
    {
        var rectangle = new RectangleF(Grid.CellToPixel(anchorPoint), new Vector2(Grid.CellSize));
        painter.DrawAsRectangle(ResourceAssets.Instance.Textures["scaffold"],
            rectangle, new DrawSettings {SourceRectangle = rectangle.ToRectangle()});
    }

    public static void DrawStructure(Painter painter, Structure structure)
    {
        if (structure.Settings.DrawDescription.TextureName == null)
        {
            return;
        }

        var graphicsTopLeft = structure.Center + structure.Settings.DrawDescription.GraphicTopLeft;
        painter.DrawAtPosition(ResourceAssets.Instance.Textures[structure.Settings.DrawDescription.TextureName],
            Grid.CellToPixel(graphicsTopLeft), Scale2D.One, new DrawSettings());
    }

    private Cell? GetPlannedBuildPosition()
    {
        if (!_mousePosition.HasValue)
        {
            return null;
        }

        return Grid.PixelToCell(_mousePosition.Value);
    }

    private PlannedStructure? GetPlannedStructure()
    {
        return _ui.State.CurrentStructure();
    }
}
