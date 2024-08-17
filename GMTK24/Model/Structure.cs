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

    /// <summary>
    ///     Use builder instead!
    /// </summary>
    public Structure(Cell worldCenter, HashSet<Cell> localCells, IEnumerable<Cell> localAnchorPoints,
        PlanSettings settings, Blueprint blueprint)
    {
        Settings = settings;
        Blueprint = blueprint;
        foreach (var localCell in localCells)
        {
            _occupiedWorldSpace.Add(worldCenter + localCell);
        }

        foreach (var anchorPoint in localAnchorPoints)
        {
            _scaffoldAnchorPoints.Add(worldCenter + anchorPoint);
        }

        Center = worldCenter;
    }

    public IEnumerable<Cell> OccupiedCells => _occupiedWorldSpace;
    public IEnumerable<Cell> ScaffoldAnchorPoints => _scaffoldAnchorPoints;
    public Cell Center { get; }

    public IEnumerable<Cell> BottomCells()
    {
        var bottomY = OccupiedCells.MaxBy(a => a.Y).Y;
        return OccupiedCells.Where(a => a.Y == bottomY);
    }
}
