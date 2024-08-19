using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Config;
using GMTK24.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace GMTK24;

public class PlanEditorSession : ISession
{
    private readonly Camera _camera;
    private readonly List<Tuple<string, StructurePlan>> _plans = new();
    private readonly Point _screenSize;
    private Cell? _hoveredCell;
    private int _planIndex;
    private EditorTool _tool;

    public PlanEditorSession(Point screenSize)
    {
        _screenSize = screenSize;
        ReadPlans();
        _planIndex = 0;
        var zoomLevel = 0.5f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
    }

    private StructurePlan CurrentPlan => _plans[_planIndex].Item2;

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var delta = input.Mouse.ScrollDelta();
        if (delta != 0)
        {
            if (delta < 0)
            {
                _camera.ViewBounds = _camera.ViewBounds.InflatedMaintainAspectRatio(10);
            }
            else
            {
                _camera.ViewBounds = _camera.ViewBounds.InflatedMaintainAspectRatio(-10);
            }
        }
        
        if (input.Keyboard.GetButton(Keys.Escape).WasPressed)
        {
            RequestPlayMode?.Invoke();
        }

        if (input.Keyboard.GetButton(Keys.Tab).WasPressed)
        {
            if (input.Keyboard.Modifiers.Shift)
            {
                _tool--;

                if (_tool < 0)
                {
                    _tool = 0;
                }
            }
            else
            {
                _tool++;
                _tool = (EditorTool) ((int) _tool % Enum.GetValues<EditorTool>().Length);
            }
        }

        if (input.Keyboard.GetButton(Keys.Left).WasPressed)
        {
            _planIndex--;

            if (_planIndex < 0)
            {
                _planIndex = 0;
            }
        }

        if (input.Keyboard.GetButton(Keys.Right).WasPressed)
        {
            _planIndex++;

            if (_planIndex > _plans.Count - 1)
            {
                _planIndex = _plans.Count - 1;
            }
        }

        if (input.Keyboard.GetButton(Keys.R).WasPressed)
        {
            ReadPlans();
        }

        if (_camera.ViewBounds.Contains(input.Mouse.Position(_camera.ScreenToCanvas)))
        {
            _hoveredCell = Grid.PixelToCell(input.Mouse.Position(_camera.ScreenToCanvas));
        }
        else
        {
            _hoveredCell = null;
        }
        
        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            if (_hoveredCell.HasValue)
            {
                switch (_tool)
                {
                    case EditorTool.MainCells:
                        ToggleCell(_hoveredCell.Value,CurrentPlan.OccupiedCells);
                        break;
                    case EditorTool.SpawnScaffold:
                        ToggleCell(_hoveredCell.Value,CurrentPlan.ScaffoldAnchorPoints);
                        break;
                    case EditorTool.ProvidesSupport:
                        ToggleCell(_hoveredCell.Value,CurrentPlan.ProvidesStructureCells);
                        break;
                    case EditorTool.RequiresSupport:
                        ToggleCell(_hoveredCell.Value,CurrentPlan.RequiresSupportCells);
                        break;
                }

                SaveCurrent();
            }
        }

        if (input.Keyboard.Modifiers.Shift)
        {
            if (input.Keyboard.GetButton(Keys.W).WasPressed)
            {
                CurrentPlan.Settings.DrawDescription.GraphicTopLeft += new Cell(0, -1);
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.A).WasPressed)
            {
                CurrentPlan.Settings.DrawDescription.GraphicTopLeft += new Cell(-1, 0);
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.D).WasPressed)
            {
                CurrentPlan.Settings.DrawDescription.GraphicTopLeft += new Cell(1, 0);
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.S).WasPressed)
            {
                CurrentPlan.Settings.DrawDescription.GraphicTopLeft += new Cell(0, 1);
                SaveCurrent();
            }
        }

        if (input.Keyboard.Modifiers.None)
        {
            if (input.Keyboard.GetButton(Keys.Q).WasPressed)
            {
                CurrentPlan.Settings.BlockScaffoldRaycasts = !CurrentPlan.Settings.BlockScaffoldRaycasts;
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.E).WasPressed)
            {
                var nextLayerInt = (int) CurrentPlan.Settings.StructureLayer;
                nextLayerInt++;
                nextLayerInt %= Enum.GetValues<StructureLayer>().Length;

                CurrentPlan.Settings.StructureLayer = (StructureLayer) nextLayerInt;
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.OemPlus).WasPressed)
            {
                CurrentPlan.Settings.RequiredSupports++;
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.OemMinus).WasPressed)
            {
                CurrentPlan.Settings.RequiredSupports--;
                SaveCurrent();
            }
        }
    }

    private void ToggleCell(Cell cellToToggle, HashSet<Cell> cells)
    {
        if (!cells.Add(cellToToggle))
        {
            cells.Remove(cellToToggle);
        }
    }

    public void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        Structure.DrawStructure(painter, CurrentPlan.BuildReal(Cell.Origin, new Blueprint()));
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        var cells = _tool switch
        {
            EditorTool.SpawnScaffold => CurrentPlan.ScaffoldAnchorPoints,
            EditorTool.ProvidesSupport => CurrentPlan.ProvidesStructureCells,
            EditorTool.RequiresSupport => CurrentPlan.RequiresSupportCells,
            _ => CurrentPlan.OccupiedCells
        };
        
        painter.DrawLineRectangle(Grid.CellToPixelRectangle(Cell.Origin).Inflated(-1, -1), new LineDrawSettings{Color = Color.White.WithMultipliedOpacity(0.5f)});

        PreviewCells(painter, cells, CurrentPlan.OccupiedCells, Color.DarkBlue.WithMultipliedOpacity(0.25f), Depth.Middle + 1);
        PreviewCells(painter, cells, CurrentPlan.RequiresSupportCells, Color.DarkRed.WithMultipliedOpacity(0.5f), Depth.Middle + 2);
        PreviewCells(painter, cells, CurrentPlan.ProvidesStructureCells, Color.DarkGreen.WithMultipliedOpacity(0.5f), Depth.Middle + 2);
        
        foreach (var cell in cells)
        {
            var rectangle = Grid.CellToPixelRectangle(cell).Inflated(-1, -1);

            // if this cell is not contained in the overall body cells, highlight that!
            var color = Color.Orange;
            if (!CurrentPlan.OccupiedCells.Contains(cell))
            {
                color = Color.HotPink;
            }

            painter.DrawRectangle(rectangle,
                    new DrawSettings {Color = color.WithMultipliedOpacity(0.75f), Depth = Depth.Middle});
        }

        if (_hoveredCell.HasValue)
        {
            painter.DrawLineRectangle(Grid.CellToPixelRectangle(_hoveredCell.Value), new LineDrawSettings());
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();

        var bigFont = 128;
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/console-font", bigFont), _plans[_planIndex].Item1,
            new Vector2(0, _screenSize.Y - bigFont), new DrawSettings());

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"Tool: {_tool}");
        messageBuilder.AppendLine(
            $"(Q){nameof(CurrentPlan.Settings.BlockScaffoldRaycasts)}={CurrentPlan.Settings.BlockScaffoldRaycasts}");
        messageBuilder.AppendLine(
            $"(E){nameof(CurrentPlan.Settings.StructureLayer)}={CurrentPlan.Settings.StructureLayer}");
        messageBuilder.AppendLine(
            $"(+/-){nameof(CurrentPlan.Settings.RequiredSupports)}={CurrentPlan.Settings.RequiredSupports}");
        var message = messageBuilder.ToString();
        var smallFont = Client.Assets.GetFont("engine/console-font", 32);
        painter.DrawStringAtPosition(smallFont, message,
            new Vector2(0, _screenSize.Y - bigFont - smallFont.MeasureString(message).Y), new DrawSettings());

        painter.EndSpriteBatch();
    }

    private static void PreviewCells(Painter painter, HashSet<Cell> selectedCells, HashSet<Cell> cellsToPreview, Color color, Depth depth)
    {
        if (selectedCells != cellsToPreview)
        {
            foreach (var cell in cellsToPreview)
            {
                var rectangle = Grid.CellToPixelRectangle(cell).Inflated(-3, -3);
                painter.DrawRectangle(rectangle,
                    new DrawSettings {Color = color, Depth = depth});
            }
        }
    }

    public void Update(float dt)
    {
    }

    private void SaveCurrent()
    {
        var planFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource/Plans");
        var currentPlan = _plans[_planIndex];
        planFiles.WriteToFile(currentPlan.Item1, JsonConvert.SerializeObject(currentPlan.Item2, Formatting.Indented));
    }

    private void ReadPlans()
    {
        _plans.Clear();
        var planFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource/Plans");
        foreach (var fileName in planFiles.GetFilesAt("."))
        {
            var result = JsonConvert.DeserializeObject<StructurePlan>(planFiles.ReadFile(fileName));
            if (result != null)
            {
                _plans.Add(new Tuple<string, StructurePlan>(fileName, result));
            }
        }
    }

    public event Action? RequestPlayMode;

    private enum EditorTool
    {
        MainCells,
        SpawnScaffold,
        ProvidesSupport,
        RequiresSupport
    }
}