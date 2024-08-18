using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Config;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GMTK24;

public class GameSession : ISession
{
    private readonly Camera _camera;
    private readonly ErrorMessage _errorMessage;
    private readonly Inventory _inventory = new();
    private readonly HoverState _isHovered = new();
    private readonly List<Level> _levels;
    private readonly FormattedTextOverlay _rulesOverlay;
    private readonly Point _screenSize;
    private readonly World _world = new();
    private int? _currentLevelIndex;
    private Objective? _currentObjective;
    private Overlay? _currentOverlay;
    private bool _isPanning;
    private Vector2? _mousePosition;
    private Ui? _ui;

    public GameSession(Point screenSize)
    {
        _screenSize = screenSize;

        _inventory.AddResource(new Resource("resource_icon_population", "Population", false));
        _inventory.AddResource(new Resource("resource_icon_inspiration", "Inspiration", true, 75));
        _inventory.AddResource(new Resource("resource_icon_food", "Food", false, 15));

        _levels = JsonFileReader.Read<LevelSequence>(Client.Debug.RepoFileSystem.GetDirectory("Resource"), "levels")
            .Levels;
        StartNextLevel();

        _rulesOverlay =
            new FormattedTextOverlay(
                FormattedText.FromFormatString(new IndirectFont("gmtk/GameFont", 50), Color.White,
                    _inventory.DisplayRules(), GameplayConstants.FormattedTextParser));

        var zoomLevel = 0.5f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
        _world.MainLayer.AddStructureToLayer(new Cell(0, 0), JsonFileReader.ReadPlan("plan_foundation"),
            new Blueprint());
        _errorMessage = new ErrorMessage(screenSize);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_currentOverlay != null)
        {
            _currentOverlay.UpdateInput(input, hitTestStack, _screenSize);

            if (_currentOverlay.IsClosed)
            {
                _currentOverlay = null;
                _ui?.FadeIn();
            }

            _isHovered.Unset();
            return;
        }

        var worldLayer = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Back);
        worldLayer.AddZone(_camera.ViewBounds, Depth.Back, _isHovered);

        if (input.Keyboard.GetButton(Keys.Escape).WasPressed && Client.Debug.IsPassiveOrActive)
        {
            RequestEditorSession?.Invoke();
        }

        if (_isHovered)
        {
            _mousePosition = input.Mouse.Position(hitTestStack.WorldMatrix * _camera.ScreenToCanvas);

            if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
            {
                input.Mouse.Consume(MouseButton.Left);

                var plannedBuildPosition = GetPlannedBuildPosition();
                var plannedStructure = GetPlannedStructure();
                var plannedBlueprint = GetPlannedBlueprint();

                if (plannedBuildPosition.HasValue && plannedStructure != null && plannedBlueprint != null)
                {
                    var result = _world.CanBuild(plannedBuildPosition.Value, plannedStructure, _inventory,
                        plannedBlueprint);

                    if (result == BuildResult.Success)
                    {
                        _world.AddStructure(plannedBuildPosition.Value, plannedStructure, plannedBlueprint);
                        if (_ui != null)
                        {
                            _ui.State.IncrementSelectedBlueprint();
                            _inventory.ApplyDeltas(_ui.State.SelectedButton!.Blueprint.Stats().OnConstructDelta);
                            _inventory.ApplyDeltas(_ui.State.SelectedButton!.Blueprint.Stats().Cost, -1);

                            if (_currentObjective?.IsComplete(_ui, _inventory, _world) == true)
                            {
                                StartNextLevel();
                            }
                        }
                    }

                    if (result == BuildResult.FailedBecauseOfFit)
                    {
                        _errorMessage.Display("Not Enough Space");
                    }

                    if (result == BuildResult.FailedBecauseOfStructure)
                    {
                        _errorMessage.Display("Needs Structural Support");
                    }

                    if (result == BuildResult.FailedBecauseOfCost)
                    {
                        _errorMessage.Display("More Resources Required");
                    }
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

                if (normalizedScrollDelta < 0)
                {
                    if (_camera.ViewBounds.Width < _screenSize.X)
                    {
                        _camera.ZoomOutFrom((int) (normalizedScrollDelta * -zoomStrength), _mousePosition.Value);
                    }
                }
                else
                {
                    if (_camera.ViewBounds.Width > (float) _screenSize.X / 10 * 2)
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

        _ui?.UpdateInput(input, hitTestStack);
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
        foreach (var structure in _world.DecorationLayer.Structures)
        {
            DrawStructure(painter, structure);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        if (_mousePosition.HasValue && !_isPanning && _isHovered)
        {
            var buildPosition = GetPlannedBuildPosition();
            var plannedStructure = GetPlannedStructure();
            var plannedBlueprint = GetPlannedBlueprint();

            if (plannedStructure != null && buildPosition.HasValue && plannedBlueprint != null)
            {
                var realStructure = plannedStructure.BuildReal(buildPosition.Value, plannedBlueprint);
                var defaultColor = Color.Green;

                var buildResult = _world.CanBuild(buildPosition.Value, plannedStructure, _inventory, plannedBlueprint);

                if (buildResult == BuildResult.FailedBecauseOfCost)
                {
                    defaultColor = Color.White;
                }

                if (buildResult == BuildResult.FailedBecauseOfFit)
                {
                    defaultColor = Color.Yellow;
                }

                if (buildResult == BuildResult.FailedBecauseOfStructure)
                {
                    defaultColor = Color.Black;
                }

                var placingLayer = _world.MainLayer;
                if (plannedStructure.Settings.StructureLayer == StructureLayer.Decoration)
                {
                    placingLayer = _world.DecorationLayer;
                }

                foreach (var cell in realStructure.OccupiedCells)
                {
                    var rectangle = Grid.CellToPixelRectangle(cell);

                    var color = defaultColor;
                    if (placingLayer.IsOccupiedAt(cell))
                    {
                        color = Color.OrangeRed;
                    }

                    painter.DrawRectangle(rectangle, new DrawSettings {Color = color.WithMultipliedOpacity(0.5f)});
                }
            }
        }

        painter.EndSpriteBatch();

        _errorMessage.Draw(painter);

        _ui?.Draw(painter, _inventory);

        _currentOverlay?.Draw(painter, _screenSize);
    }

    public void Update(float dt)
    {
        _ui?.Update(dt);

        if (_currentOverlay != null)
        {
            _currentOverlay.Update(dt);
        }

        _errorMessage.Update(dt);

        foreach (var structure in _world.AllStructures())
        {
            _inventory.ApplyDeltas(structure.Blueprint.Stats().OnSecondDelta, dt);
        }

        _inventory.ResourceUpdate(dt);
    }

    private void StartNextLevel()
    {
        if (_currentLevelIndex == null)
        {
            _currentLevelIndex = 0;
        }
        else
        {
            _currentLevelIndex = _currentLevelIndex.Value + 1;
        }

        if (!_levels.IsValidIndex(_currentLevelIndex.Value))
        {
            WinGame();
            return;
        }

        var level = _levels[_currentLevelIndex.Value];
        var uiBuilder = new UiLayoutBuilder();

        foreach (var resource in _inventory.AllResources())
        {
            uiBuilder.AddResource(resource);
        }

        var blueprintFolder = Client.Debug.RepoFileSystem.GetDirectory("Resource/blueprints");
        foreach (var levelBlueprint in level.Blueprints)
        {
            var blueprint = JsonFileReader.Read<Blueprint>(blueprintFolder, levelBlueprint.Name);
            uiBuilder.AddBlueprint(levelBlueprint.Name, blueprint, levelBlueprint.IsLocked);
        }

        _currentObjective = new Objective(level.CompletionCriteria);

        if (level.IntroDialogue.Count > 0)
        {
            OpenOverlay(new DialogueOverlay(_inventory,
                level.IntroDialogue.Select(text => new DialoguePage {Text = text}).ToList(), () =>
                {
                    _ui = BuildUi(uiBuilder);
                }));
        }
        else
        {
            _ui = BuildUi(uiBuilder);
            _ui.FadeIn();
        }
    }

    private Ui BuildUi(UiLayoutBuilder uiBuilder)
    {
        var ui = uiBuilder.Build(_screenSize);
        ui.RequestRules += ShowRules;
        return ui;
    }

    private void WinGame()
    {
        Client.Debug.Log("You win!");
    }

    private void ShowRules()
    {
        OpenOverlay(_rulesOverlay);
    }

    private void OpenOverlay(Overlay newOverlay)
    {
        newOverlay.Reset();
        _currentOverlay = newOverlay;
        _ui?.FadeOut();
    }

    public event Action? RequestEditorSession;

    private void DrawScaffold(Painter painter, ScaffoldCell scaffoldCell)
    {
        var rectangle = new RectangleF(Grid.CellToPixel(scaffoldCell.Cell), new Vector2(Grid.CellSize));

        var texture = ResourceAssets.Instance.Textures["LVL01_PILLAR"];

        if (scaffoldCell.PointType == ScaffoldPointType.Middle)
        {
            var sourceRectangle = rectangle.ToRectangle();
            sourceRectangle.Location = new Point(0, sourceRectangle.Location.Y);
            painter.DrawAsRectangle(texture,
                rectangle, new DrawSettings {SourceRectangle = sourceRectangle});
        }
        else
        {
            if (scaffoldCell.PointType == ScaffoldPointType.Bottom)
            {
                texture = ResourceAssets.Instance.Textures["LVL01_PILLAR_BookEndBottom"];
            }

            if (scaffoldCell.PointType == ScaffoldPointType.Top)
            {
                texture = ResourceAssets.Instance.Textures["LVL01_PILLAR_BookEndTop"];
            }

            painter.DrawAsRectangle(texture, rectangle, new DrawSettings());
        }
    }

    public static void DrawStructure(Painter painter, Structure structure)
    {
        if (structure.Settings.DrawDescription.TextureName == null)
        {
            return;
        }

        var graphicsTopLeft = structure.Center + structure.Settings.DrawDescription.GraphicTopLeft;
        painter.DrawAtPosition(ResourceAssets.Instance.Textures[structure.Settings.DrawDescription.TextureName],
            Grid.CellToPixel(graphicsTopLeft), Scale2D.One,
            new DrawSettings {Depth = Depth.Front - structure.Center.Y});
    }

    private Cell? GetPlannedBuildPosition()
    {
        if (!_mousePosition.HasValue)
        {
            return null;
        }

        return Grid.PixelToCell(_mousePosition.Value);
    }

    private StructurePlan? GetPlannedStructure()
    {
        return _ui?.State.CurrentStructure();
    }

    public Blueprint? GetPlannedBlueprint()
    {
        return _ui?.State.CurrentBlueprint();
    }
}
