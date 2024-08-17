using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;

namespace GMTK24;

public class GameSession : ISession
{
    private readonly Camera _camera;
    private readonly HoverState _isHovered = new();
    private readonly Ui _ui;
    private readonly World _world;
    private Vector2? _mousePosition;
    private bool _isPanning;

    public GameSession()
    {
        var layoutBuilder = new UiLayoutBuilder();

        var house = new StructureBuilder()
                .AddCell(-1, -1)
                .AddCell(0, -1)
                .AddCell(1, -1)
                .AddCell(2, -1)
                .AddCell(-1, 0)
                .AddCell(0, 0)
                .AddCell(1, 0)
                .AddCell(2, 0)
                .AddCell(-1, 1)
                .AddCell(0, 1)
                .AddCell(1, 1)
                .AddCell(2, 1)
                .AddCell(-1, 2)
                .AddCell(0, 2)
                .AddCell(1, 2)
                .AddCell(2, 2)
                .BuildPlan(new StructureDrawDescription {TextureName = "house", GraphicTopLeft = new Cell(-1, -1)})
            ;

        var plant = new StructureBuilder()
                .AddCell(0, 2)
                .AddCell(0, 1)
                .AddCell(0, 0)
                .AddCell(0, -1)
                .AddCell(0, -2)
                .AddCell(1, -2)
                .AddCell(-1, -2)
                .AddCell(-1, -1)
                .AddCell(1, -1)
                .BuildPlan(new StructureDrawDescription())
            ;

        var platform = new StructureBuilder()
                .AddCell(1, 0)
                .AddCell(0, 0)
                .AddCell(-1, 0)
                .BuildPlan(new StructureDrawDescription())
            ;

        var foundation = new StructureBuilder()
                .AddCell(2, 0)
                .AddCell(1, 0)
                .AddCell(0, 0)
                .AddCell(-1, 0)
                .AddCell(-2, 0)
                .BuildPlan(new StructureDrawDescription())
            ;

        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(house)));
        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(plant)));
        layoutBuilder.AddBuildAction(new BuildAction(new Blueprint(platform)));
        _ui = layoutBuilder.Build();

        var screenSize = new Point(1920, 1080);
        var zoomLevel = 0.25f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
        _world = new World();

        _world.MainLayer.AddStructure(new Cell(0, 0), foundation);
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
                    var rectangle = new RectangleF(Grid.CellToPixel(cell), new Vector2(Grid.CellSize));
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

    private void DrawStructure(Painter painter, Structure structure)
    {
        if (structure.DrawDescription.TextureName == null)
        {
            return;
        }

        var graphicsTopLeft = structure.Center + structure.DrawDescription.GraphicTopLeft;
        painter.DrawAtPosition(ResourceAssets.Instance.Textures[structure.DrawDescription.TextureName],
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
