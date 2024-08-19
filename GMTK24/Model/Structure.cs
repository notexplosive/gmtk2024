using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GMTK24.Config;

namespace GMTK24.Model;

public class Structure
{
    public PlanSettings Settings { get; }
    public Blueprint Blueprint { get; }

    /// <summary>
    ///     Occupied Cells in the world
    /// </summary>
    private readonly List<Cell> _occupiedWorldSpace = new();

    private readonly List<Cell> _scaffoldAnchorPoints = new();
    private readonly List<Cell> _cellsProvidingSupport = new();

    /// <summary>
    ///     Use builder instead!
    /// </summary>
    public Structure(Cell worldCenter, StructurePlan plan, Blueprint blueprint)
    {
        Settings = plan.Settings;
        Blueprint = blueprint;

        ApplyCells(worldCenter, plan.OccupiedCells, _occupiedWorldSpace);
        ApplyCells(worldCenter, plan.ScaffoldAnchorPoints, _scaffoldAnchorPoints);
        ApplyCells(worldCenter, plan.ProvidesStructureCells, _cellsProvidingSupport);
        
        Center = worldCenter;
    }

    private static void ApplyCells(Cell worldCenter, HashSet<Cell> localCells, List<Cell> occupiedWorldSpace)
    {
        foreach (var localCell in localCells)
        {
            occupiedWorldSpace.Add(worldCenter + localCell);
        }
    }

    public IEnumerable<Cell> OccupiedCells => _occupiedWorldSpace;
    public IEnumerable<Cell> ScaffoldAnchorPoints => _scaffoldAnchorPoints;
    public IEnumerable<Cell> CellsProvidingSupport => _cellsProvidingSupport;
    public Cell Center { get; }

    public IEnumerable<Cell> BottomCells()
    {
        var bottomY = OccupiedCells.MaxBy(a => a.Y).Y;
        return OccupiedCells.Where(a => a.Y == bottomY);
    }
}
