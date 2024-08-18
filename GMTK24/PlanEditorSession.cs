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
    private readonly Point _screenSize;
    private readonly Camera _camera;
    private readonly List<Tuple<string, StructurePlan>> _plans = new();
    private Cell? _hoveredCell;
    private int _planIndex;

    public PlanEditorSession(Point screenSize)
    {
        _screenSize = screenSize;
        ReadPlans();
        _planIndex = 0;
        var zoomLevel = 0.25f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
    }

    private StructurePlan CurrentPlan => _plans[_planIndex].Item2;

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.GetButton(Keys.Escape).WasPressed)
        {
            RequestPlayMode?.Invoke();
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

        var worldHitTestLayer = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Middle);

        foreach (var cell in GetAllCellsExtended())
        {
            var rectangle = Grid.CellToPixelRectangle(cell);
            worldHitTestLayer.AddZone(rectangle, Depth.Middle, () => { _hoveredCell = null; },
                () => { _hoveredCell = cell; });
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            if (_hoveredCell.HasValue)
            {
                // Toggle cell
                if (!CurrentPlan.Cells.Add(_hoveredCell.Value))
                {
                    CurrentPlan.Cells.Remove(_hoveredCell.Value);
                }

                SaveCurrent();
            }
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasPressed)
        {
            if (_hoveredCell.HasValue)
            {
                // Toggle cell
                if (!CurrentPlan.ScaffoldAnchorPoints.Add(_hoveredCell.Value))
                {
                    CurrentPlan.ScaffoldAnchorPoints.Remove(_hoveredCell.Value);
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
                CurrentPlan.Settings.CreatesScaffold = !CurrentPlan.Settings.CreatesScaffold;
                SaveCurrent();
            }

            if (input.Keyboard.GetButton(Keys.W).WasPressed)
            {
                CurrentPlan.Settings.ProvidesSupport = !CurrentPlan.Settings.ProvidesSupport;
                SaveCurrent();
            }
            
            if (input.Keyboard.GetButton(Keys.E).WasPressed)
            {
                var nextLayerInt = (int) CurrentPlan.Settings.StructureLayer;
                nextLayerInt++;
                nextLayerInt %= Enum.GetValues<StructureLayer>().Length;
                
                CurrentPlan.Settings.StructureLayer = (StructureLayer)nextLayerInt;
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

    public void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        GameSession.DrawStructure(painter, CurrentPlan.BuildReal(Cell.Origin, new Blueprint()));
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        foreach (var cell in GetAllCellsExtended())
        {
            var rectangle = Grid.CellToPixelRectangle(cell).Inflated(-1, -1);

            if (CurrentPlan.ScaffoldAnchorPoints.Contains(cell))
            {
                painter.DrawRectangle(rectangle.Inflated(-2, -2),
                    new DrawSettings {Color = Color.White, Depth = Depth.Middle - 1});
            }

            if (CurrentPlan.Cells.Contains(cell))
            {
                var color = Color.Orange;
                if (cell == Cell.Origin)
                {
                    color = Color.Red;
                }

                painter.DrawRectangle(rectangle,
                    new DrawSettings {Color = color.WithMultipliedOpacity(0.5f), Depth = Depth.Middle});
            }

            if (_hoveredCell == cell)
            {
                painter.DrawLineRectangle(rectangle, new LineDrawSettings {Color = Color.White});
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();

        var bigFont = 128;
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/console-font", bigFont), _plans[_planIndex].Item1,
            new Vector2(0,  - bigFont), new DrawSettings());

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"(Q){nameof(CurrentPlan.Settings.CreatesScaffold)}={CurrentPlan.Settings.CreatesScaffold}");
        messageBuilder.AppendLine($"(W){nameof(CurrentPlan.Settings.ProvidesSupport)}={CurrentPlan.Settings.ProvidesSupport}");
        messageBuilder.AppendLine($"(E){nameof(CurrentPlan.Settings.StructureLayer)}={CurrentPlan.Settings.StructureLayer}");
        messageBuilder.AppendLine($"(+/-){nameof(CurrentPlan.Settings.RequiredSupports)}={CurrentPlan.Settings.RequiredSupports}");
        var message = messageBuilder.ToString();
        var smallFont = Client.Assets.GetFont("engine/console-font", 32);
        painter.DrawStringAtPosition(smallFont, message,
            new Vector2(0, _screenSize.Y - bigFont - smallFont.MeasureString(message).Y), new DrawSettings());

        painter.EndSpriteBatch();
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

    private HashSet<Cell> GetAllCellsExtended()
    {
        var cells = new HashSet<Cell>(CurrentPlan.Cells);

        cells = cells.Concat(CurrentPlan.ScaffoldAnchorPoints).ToHashSet();

        foreach (var cell in cells.ToList())
        {
            cells.Add(cell + new Cell(0, 1));
            cells.Add(cell + new Cell(1, 0));
            cells.Add(cell + new Cell(-1, 0));
            cells.Add(cell + new Cell(0, -1));

            cells.Add(cell + new Cell(-1, -1));
            cells.Add(cell + new Cell(1, 1));
            cells.Add(cell + new Cell(1, -1));
            cells.Add(cell + new Cell(-1, 1));
        }

        if (cells.Count == 0)
        {
            cells.Add(Cell.Origin);
        }

        return cells;
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
}
