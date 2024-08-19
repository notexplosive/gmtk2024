using System.Collections.Generic;
using ExTween;
using GMTK24.Config;

namespace GMTK24.Model;

public class Structure
{
    private readonly List<Cell> _cellsNeedingSupport = new();
    private readonly List<Cell> _cellsProvidingSupport = new();
    private readonly List<Cell> _occupiedWorldSpace = new();
    private readonly List<Cell> _scaffoldAnchorPoints = new();

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
}
