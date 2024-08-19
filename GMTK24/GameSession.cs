using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using GMTK24.Config;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GMTK24;

public class GameSession : ISession
{
    private readonly Camera _camera;
    private readonly ErrorMessage _errorMessage;
    private readonly Inventory _inventory = new();
    private readonly HoverState _isHovered = new();
    private readonly List<Level> _levels;
    private readonly OverlayScreen _overlayScreen;
    private readonly List<Particle> _particles = new();
    private readonly List<Structure> _replayStructures = new();
    private readonly RectangleF _startingCamera;
    private readonly World _world = new();
    private Cutscene? _currentCutscene;
    private int? _currentLevelIndex;
    private Objective? _currentObjective;
    private PersistentToast? _currentToast;
    private float _elapsedTime;
    private bool _hasPlayedEndCutscene;
    private bool _isAwaitingScreenshot;
    private bool _isPanning;
    private Vector2? _mousePosition;
    private Vector2 _panVector;
    private Point _screenSize;
    private Ui? _ui;

    public GameSession(Point screenSize)
    {
        _screenSize = screenSize;

        _inventory.AddResource(new Resource("ICONS_Social", "ICONS_Social00", "Population", false));
        _inventory.AddResource(new Resource("ICONS_Insp", "ICONS_Insp00", "Inspiration", true, 75));
        _inventory.AddResource(new Resource("ICONS_Food", "ICONS_Food00", "Food", false, 15));

        _overlayScreen = new OverlayScreen(_inventory);

        _levels = JsonFileReader.Read<LevelSequence>(Client.Debug.RepoFileSystem.GetDirectory("Resource"), "levels")
            .Levels;
        StartNextLevel();

        var zoomLevel = 0.5f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
        _world.MainLayer.AddStructureToLayer(new Cell(0, -2), JsonFileReader.ReadPlan("plan_foundation"),
            new Blueprint());

        var average = Vector2.Zero;
        var allCells = _world.AllStructures().SelectMany(a => a.OccupiedCells).ToList();
        foreach (var cell in allCells)
        {
            average += Grid.CellToPixel(cell);
        }

        _camera.CenterPosition = average / allCells.Count + new Vector2(0, -100);

        _startingCamera = _camera.ViewBounds;

        _errorMessage = new ErrorMessage(screenSize);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.Escape).WasPressed)
            {
                RequestEditorSession?.Invoke();
            }

            if (input.Keyboard.GetButton(Keys.Q).WasPressed)
            {
                StartNextLevel();
            }

            if (input.Keyboard.GetButton(Keys.W).WasPressed)
            {
                _inventory.GetResource("Food").Add(100000);
            }

            if (input.Keyboard.GetButton(Keys.E).WasPressed)
            {
                _inventory.GetResource("Inspiration").Add(100000);
            }

            if (input.Keyboard.GetButton(Keys.R).WasPressed)
            {
                PlayCutscene(EndCutscene());
            }

            if (input.Keyboard.GetButton(Keys.T).WasPressed)
            {
                ShowScreenshotToast();
            }
        }

        _panVector = new Vector2(0, 0);

        if (_overlayScreen.HasOverlay())
        {
            _overlayScreen.UpdateInput(input, hitTestStack, _screenSize);

            if (_overlayScreen.IsCurrentOverlayClosed())
            {
                _overlayScreen.ClearCurrentOverlay();

                if (_currentCutscene == null)
                {
                    _ui?.FadeIn();
                }
            }

            _isHovered.Unset();
            return;
        }

        if (input.Keyboard.GetButton(Keys.Enter).WasPressed)
        {
            TakeScreenshot();
        }

        if (_currentCutscene != null)
        {
            _isHovered.Unset();
            return;
        }

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
                var plannedBlueprint = GetPlannedBlueprint();

                if (plannedBuildPosition.HasValue && plannedStructure != null && plannedBlueprint != null)
                {
                    var result = _world.CanBuild(plannedBuildPosition.Value, plannedStructure, _inventory,
                        plannedBlueprint);

                    if (result == BuildResult.Success)
                    {
                        var structure = _world.AddStructure(plannedBuildPosition.Value, plannedStructure,
                            plannedBlueprint);

                        _replayStructures.Add(structure);

                        var soundName = Client.Random.Dirty.GetRandomElement(structure.Blueprint.Stats().Sounds);
                        ResourceAssets.Instance.PlaySound("sounds/" + soundName, new SoundEffectSettings());

                        var onConstructDelta = structure.Blueprint.Stats().OnConstructDelta;

                        foreach (var constructDelta in onConstructDelta)
                        {
                            if (!constructDelta.AffectCapacity)
                            {
                                SpawnParticleBurst(structure.PixelCenter(),
                                    ResourceAssets.Instance.Textures[
                                        _inventory.GetResource(constructDelta.ResourceName).IconNameNoBacker!],
                                    constructDelta.Amount);
                            }
                        }

                        structure.Blueprint.IncrementStructure();
                        _inventory.ApplyDeltas(onConstructDelta);
                        _inventory.ApplyDeltas(structure.Blueprint.Stats().Cost, -1);

                        if (_ui != null)
                        {
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

            if (input.Keyboard.GetButton(Keys.W).IsDown)
            {
                _panVector += new Vector2(0, -1);
            }

            if (input.Keyboard.GetButton(Keys.A).IsDown)
            {
                _panVector += new Vector2(-1, 0);
            }

            if (input.Keyboard.GetButton(Keys.S).IsDown)
            {
                _panVector += new Vector2(0, 1);
            }

            if (input.Keyboard.GetButton(Keys.D).IsDown)
            {
                _panVector += new Vector2(1, 0);
            }

            if (input.Keyboard.GetButton(Keys.Space).WasPressed)
            {
                PlayCutscene(AlignToGridCutscene());
            }

            if (_panVector != Vector2.Zero)
            {
                _ui?.SetHasPanned();
            }

            _isPanning = input.Mouse.GetButton(MouseButton.Middle).IsDown ||
                         input.Mouse.GetButton(MouseButton.Right).IsDown;

            if (_isPanning)
            {
                _ui?.SetHasPanned();
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
                    if (_camera.ViewBounds.Width < _screenSize.X || _hasPlayedEndCutscene)
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
        painter.Clear(GameplayConstants.SkyColor);

        DrawWorldBackground(painter);

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        DrawMousePositionOverlay(painter);

        DrawParticles(painter);

        DrawWorldForeground(painter);

        _errorMessage.Draw(painter);

        _ui?.Draw(painter, _inventory, _overlayScreen.HasOverlay() || _currentCutscene != null);

        _overlayScreen.Draw(painter, _screenSize);

        DrawCurrentToast(painter);
    }

    public void Update(float dt)
    {
        _elapsedTime += dt;
        _camera.CenterPosition += _panVector * dt * 60 * 10;
        _ui?.Update(dt);

        _particles.RemoveAll(a => a.IsExpired());

        foreach (var particle in _particles)
        {
            particle.Update(dt);
        }

        _overlayScreen.Update(dt);
        _currentCutscene?.Update(dt);
        _currentToast?.Update(dt);

        if (_currentCutscene?.IsDone() == true)
        {
            _currentCutscene = null;
        }

        _errorMessage.Update(dt);

        foreach (var structure in _world.AllStructures())
        {
            _inventory.ApplyDeltas(structure.Blueprint.Stats().OnSecondDelta, dt);
            structure.Lifetime += dt;
        }

        _inventory.ResourceUpdate(dt);
    }

    private Cutscene AlignToGridCutscene()
    {
        var cutscene = new Cutscene(_overlayScreen);

        var newViewBounds = RectangleF.FromCenterAndSize(_camera.ViewBounds.Center, _screenSize.ToVector2());
        newViewBounds.Location = newViewBounds.Location.ToPoint().ToVector2();

        cutscene.PanCamera(_camera, newViewBounds, 0.25f, Ease.CubicFastSlow);

        return cutscene;
    }

    private void TakeScreenshot()
    {
        _isAwaitingScreenshot = false;

        var canvas = new Canvas(_screenSize);

        var painter = Client.Graphics.Painter;
        Client.Graphics.PushCanvas(canvas);
        painter.Clear(GameplayConstants.SkyColor);
        DrawWorldBackground(painter);
        DrawWorldForeground(painter);

        try
        {
            var currentTime = DateTime.Now;
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Join(homeDirectory, GameplayConstants.Title);
            Directory.CreateDirectory(directory);
            var screenshotFilePath = Path.Join(directory, $"{currentTime.ToFileTimeUtc()}.png");
            using var stream = File.Create(screenshotFilePath);
            var texture = canvas.Texture;
            texture.SaveAsPng(stream, texture.Width, texture.Height);

            ShowToast($"Screenshot saved at\n{screenshotFilePath}", 5);
        }
        catch
        {
            ShowToast("Screenshot capture failed :(", 3);
        }

        Client.Graphics.PopCanvas();
    }

    private void ShowScreenshotToast()
    {
        _isAwaitingScreenshot = true;
        ShowToast("Press Enter to take a Screenshot", () => _isAwaitingScreenshot == false);
    }

    private void ShowToast(string text, float duration)
    {
        _currentToast = new PersistentToast(text, duration);
    }

    private void ShowToast(string text, Func<bool> vanishCriteria)
    {
        _currentToast = new PersistentToast(text, vanishCriteria);
    }

    private void DrawCurrentToast(Painter painter)
    {
        if (_currentToast != null)
        {
            painter.BeginSpriteBatch();
            var outerRectangle = _screenSize.ToRectangleF().Inflated(0, -120);

            var toastFont = Client.Assets.GetFont("gmtk/GameFont", 32);
            var toastRectangle = RectangleF.FromSizeAlignedWithin(outerRectangle,
                toastFont.MeasureString(_currentToast.Text) + new Vector2(1), Alignment.TopCenter);

            toastRectangle.Location = new Vector2(toastRectangle.X,
                toastRectangle.Y - (toastRectangle.Y + toastRectangle.Height * 2f) *
                (1 - _currentToast.VisiblePercent));

            toastRectangle = toastRectangle.Inflated(20, 20);

            painter.DrawRectangle(toastRectangle, new DrawSettings
            {
                Color = Color.Black.WithMultipliedOpacity(0.25f),
                Depth = Depth.Back
            });

            painter.DrawStringWithinRectangle(toastFont, _currentToast.Text, toastRectangle, Alignment.Center,
                new DrawSettings());
            painter.EndSpriteBatch();
        }
    }

    private void DrawMousePositionOverlay(Painter painter)
    {
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
    }

    private void DrawParticles(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var particle in _particles)
        {
            painter.DrawAtPosition(particle.Texture, particle.Position, Scale2D.One,
                new DrawSettings
                {
                    Angle = particle.Angle, Origin = DrawOrigin.Center,
                    Color = Color.White.WithMultipliedOpacity(particle.Opacity)
                });
        }

        painter.EndSpriteBatch();
    }

    private void DrawWorldBackground(Painter painter)
    {
        DrawWater(painter, Color.CornflowerBlue.DimmedBy(0.1f), new Vector2(Grid.CellSize / 2f, -Grid.CellSize),
            MathF.PI / 2f, 0.75f);

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var scaffoldCell in _world.MainLayer.ScaffoldCells())
        {
            DrawScaffold(painter, scaffoldCell);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var structure in _world.MainLayer.Structures)
        {
            Structure.DrawStructure(painter, structure, _elapsedTime);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var structure in _world.DecorationLayer.Structures)
        {
            Structure.DrawStructure(painter, structure, _elapsedTime);
        }

        painter.EndSpriteBatch();
    }

    private void DrawWorldForeground(Painter painter)
    {
        DrawWater(painter, Color.CornflowerBlue, Vector2.Zero, 0, 1f);

        DrawWater(painter, Color.CornflowerBlue.BrightenedBy(0.1f), new Vector2(0, Grid.CellSize), MathF.PI / 3, 1.5f);

        DrawWater(painter, Color.CornflowerBlue.BrightenedBy(0.2f), new Vector2(0, Grid.CellSize * 4), MathF.PI / 4,
            2f);
    }

    private Cutscene EndCutscene()
    {
        _currentToast = null;
        var cutscene = new Cutscene(_overlayScreen);

        var lastStructure = _replayStructures.LastOrDefault();

        if (lastStructure != null)
        {
            var buildingCameraPosition = lastStructure.PixelCenter();

            cutscene.PanCamera(_camera, RectangleF.FromCenterAndSize(buildingCameraPosition, _camera.ViewBounds.Size),
                1.25f, Ease.CubicFastSlow);
        }

        cutscene.DisplayMessage(_ui, "We are a city.");

        var allStructuresRectangle = AllStructuresRectangle();
        var skyCameraPosition =
            new Vector2(_camera.ViewBounds.X, allStructuresRectangle.Top - _camera.ViewBounds.Height * 5);
        var skyViewBounds = new RectangleF(skyCameraPosition, _camera.ViewBounds.Size);

        cutscene.PanCamera(_camera, skyViewBounds, 1, Ease.CubicSlowFast);

        cutscene.Callback(() =>
        {
            foreach (var structure in _replayStructures)
            {
                structure.Hide();
            }
        });

        cutscene.PanCamera(_camera, _startingCamera, 1, Ease.CubicFastSlow);
        cutscene.DisplayMessage(_ui, "From humble beginnings.");

        var allStructuresViewBounds =
            RectangleF.FromCenterAndSize(allStructuresRectangle.Center, _screenSize.ToVector2() / 4);

        var iterations = 0;
        while (!allStructuresViewBounds.Envelopes(allStructuresRectangle))
        {
            allStructuresViewBounds = allStructuresViewBounds.InflatedMaintainAspectRatio(50);
            iterations++;

            if (iterations > 1000)
            {
                break;
            }
        }

        // extra 100px margin
        allStructuresViewBounds = allStructuresViewBounds.InflatedMaintainAspectRatio(100);

        cutscene.PanCamera(_camera, allStructuresViewBounds, 2, Ease.CubicFastSlow);

        cutscene.DisplayMessage(_ui, "We built and scaled.");

        var buildDuration = 4f;
        var totalStructures = _replayStructures.Count;
        var delay = 1f / totalStructures * buildDuration;
        foreach (var structure in _replayStructures)
        {
            cutscene.Delay(delay);
            cutscene.Callback(() => { structure.Show(); });
        }

        cutscene.Delay(0.5f);

        cutscene.Callback(() => { ShowScreenshotToast(); });
        cutscene.WaitUntil(() => _isAwaitingScreenshot == false);
        cutscene.Delay(2f);

        cutscene.DisplayMessage(_ui,
            // "Press [Enter] to take a screenshot.",
            GameplayConstants.Title,
            "Made in 96 hours for the GMTK Game Jam",
            "Visual Art by isawiitch",
            "Sound Design by quarkimo",
            "Programming and Game Design by NotExplosive",
            "Thank you for playing.",
            "You can stay for as long as you like."
        );

        cutscene.Callback(() =>
        {
            _ui?.FadeIn();
            ShowToast("Press Space to lock camera to whole pixels", 5);
        });

        return cutscene;
    }

    private RectangleF AllStructuresRectangle()
    {
        var rectangle = new RectangleF();
        foreach (var cell in _world.AllStructures().SelectMany(a => a.OccupiedCells))
        {
            rectangle = RectangleF.Union(rectangle, Grid.CellToPixelRectangle(cell));
        }

        return rectangle;
    }

    private void PlayCutscene(Cutscene cutscene)
    {
        _currentCutscene = cutscene;
    }

    private void SpawnParticleBurst(Vector2 startingPosition, Texture2D texture, int amount)
    {
        var startingAngle = Client.Random.Dirty.NextFloat() * MathF.PI * 2f;
        for (var i = 0; i < amount; i++)
        {
            var angle = startingAngle + MathF.PI * 2 / amount * (i - 1);

            _particles.Add(new Particle(startingPosition, texture, Vector2Extensions.Polar(400, angle)));
        }
    }

    private void DrawWater(Painter painter, Color waterColor, Vector2 offset, float phaseOffset, float intensity)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        var waterLevel = Grid.CellSize + offset.Y;

        if (_camera.ViewBounds.Bottom > waterLevel)
        {
            var waterRect = RectangleF.FromCorners(
                new Vector2(_camera.ViewBounds.Left, waterLevel),
                new Vector2(_camera.ViewBounds.Right, _camera.ViewBounds.Bottom)
            );

            painter.DrawRectangle(waterRect, new DrawSettings {Color = waterColor});

            var currentCell = Grid.PixelToCell(waterRect.TopLeft) - new Cell(1, 0);

            while (Grid.CellToPixel(currentCell).X < waterRect.Right)
            {
                var pixelPosition = Grid.CellToPixel(currentCell) + offset;

                var waveHeight = 5 * intensity;
                var waveSpeed = 1f;
                var phase = _elapsedTime * waveSpeed + phaseOffset;
                var sine = new Vector2(0, MathF.Sin(phase) * waveHeight);
                var cos = new Vector2(0, MathF.Cos(phase) * waveHeight);

                painter.DrawAsRectangle(
                    ResourceAssets.Instance.Textures["circle"],
                    new RectangleF(new Vector2(pixelPosition.X, waterLevel) + sine, new Vector2(Grid.CellSize)),
                    new DrawSettings {Color = waterColor, Origin = DrawOrigin.Center});
                painter.DrawAsRectangle(
                    ResourceAssets.Instance.Textures["circle"],
                    new RectangleF(new Vector2(pixelPosition.X + Grid.CellSize / 2f, waterLevel) + cos,
                        new Vector2(Grid.CellSize)),
                    new DrawSettings {Color = waterColor, Origin = DrawOrigin.Center});
                currentCell += new Cell(1, 0);
            }
        }

        painter.EndSpriteBatch();
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

        _currentToast?.FadeOut();

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
            _overlayScreen.OpenOverlay(_ui, new DialogueOverlay(_inventory,
                level.IntroDialogue.Select(text => new DialoguePage {Text = text}).ToList(),
                () =>
                {
                    _ui = BuildUi(uiBuilder);

                    if (_currentLevelIndex == 1)
                    {
                        ShowToast("Use Right or Middle Mouse Buttons to pan the camera\nYou can also use WASD",
                            () => _ui?.CurrentFtueState == FtueState.None);
                    }
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
        if(_currentLevelIndex.HasValue){
            ui.SetFtueState(_currentLevelIndex.Value);
        }
        return ui;
    }

    private void WinGame()
    {
        if (!_hasPlayedEndCutscene)
        {
            _hasPlayedEndCutscene = true;
            PlayCutscene(EndCutscene());
        }
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
