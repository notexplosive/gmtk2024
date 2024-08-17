using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace GMTK24;

public class PlanEditorSession : ISession
{
    private readonly Camera _camera;
    private readonly List<Tuple<string, PlannedStructure>> _plans = new();
    private Cell? _hoveredCell;
    private int _planIndex;

    private PlannedStructure CurrentPlan => _plans[_planIndex].Item2;
    
    public PlanEditorSession()
    {
        ReadPlans();
        _planIndex = 0;

        var screenSize = new Point(1920, 1080);
        var zoomLevel = 0.25f;
        _camera = new Camera(RectangleF.FromCenterAndSize(Vector2.Zero, screenSize.ToVector2() * zoomLevel),
            screenSize);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
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
                var currentPlan = CurrentPlan;
                if (!currentPlan.PendingCells.Add(_hoveredCell.Value))
                {
                    currentPlan.PendingCells.Remove(_hoveredCell.Value);
                }

                SaveCurrent();
            }
        }

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

    public void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        GameSession.DrawStructure(painter, CurrentPlan.BuildReal(Cell.Origin));
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        foreach (var cell in GetAllCellsExtended())
        {
            var rectangle = Grid.CellToPixelRectangle(cell).Inflated(-1, -1);

            if (CurrentPlan.PendingCells.Contains(cell))
            {
                var color = Color.Orange;
                if (cell == Cell.Origin)
                {
                    color = Color.Red;
                }

                painter.DrawRectangle(rectangle, new DrawSettings {Color = color.WithMultipliedOpacity(0.5f)});
            }

            if (_hoveredCell == cell)
            {
                painter.DrawLineRectangle(rectangle, new LineDrawSettings {Color = Color.White});
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();

        var fontSize = 128;
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/console-font", fontSize), _plans[_planIndex].Item1,
            new Vector2(0, 1080 - fontSize), new DrawSettings());

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
        var cells = new HashSet<Cell>(CurrentPlan.PendingCells);

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
            var result = JsonConvert.DeserializeObject<PlannedStructure>(planFiles.ReadFile(fileName));
            if (result != null)
            {
                _plans.Add(new Tuple<string, PlannedStructure>(fileName, result));
            }
        }
    }
}
