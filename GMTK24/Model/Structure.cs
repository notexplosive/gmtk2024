using System.Collections.Generic;

namespace GMTK24.Model;

public class Structure
{
    public StructureDrawDescription DrawDescription { get; }

    /// <summary>
    ///     Occupied Cells in the world
    /// </summary>
    private readonly List<Cell> _occupiedWorldSpace = new();

    /// <summary>
    ///     Use builder instead!
    /// </summary>
    public Structure(Cell worldCenter, IEnumerable<Cell> localCells, StructureDrawDescription drawDescription)
    {
        DrawDescription = drawDescription;
        foreach (var localCell in localCells)
        {
            _occupiedWorldSpace.Add(worldCenter + localCell);
        }

        Center = worldCenter;
    }

    public IEnumerable<Cell> OccupiedCells => _occupiedWorldSpace;
    public Cell Center { get; }
}
