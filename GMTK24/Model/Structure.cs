using System.Collections.Generic;

namespace GMTK24.Model;

public class Structure
{
    public StructureSettings Settings { get; }

    /// <summary>
    ///     Occupied Cells in the world
    /// </summary>
    private readonly List<Cell> _occupiedWorldSpace = new();

    private readonly List<Cell> _scaffoldAnchorPoints = new();

    /// <summary>
    ///     Use builder instead!
    /// </summary>
    public Structure(Cell worldCenter, HashSet<Cell> localCells, IEnumerable<Cell> localAnchorPoints,
        StructureSettings settings)
    {
        Settings = settings;
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
}