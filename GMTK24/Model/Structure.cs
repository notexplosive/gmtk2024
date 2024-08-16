using System.Collections.Generic;

namespace GMTK24.Model;

public class Structure
{
    /// <summary>
    ///     Occupied Cells in the world
    /// </summary>
    private readonly List<Cell> _occupiedWorldSpace = new();
    
    /// <summary>
    /// Use builder instead!
    /// </summary>
    public Structure(Cell worldCenter, IEnumerable<Cell> localCells)
    {
        foreach (var localCell in localCells)
        {
            _occupiedWorldSpace.Add(worldCenter + localCell);
        }
    }

    public IEnumerable<Cell> OccupiedCells => _occupiedWorldSpace;
}