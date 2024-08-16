using System.Collections.Generic;

namespace GMTK24.Model;

public class PlannedStructure
{
    private readonly HashSet<Cell> _pendingCells;

    public PlannedStructure(HashSet<Cell> pendingCells)
    {
        _pendingCells = pendingCells;
    }

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, _pendingCells);
    }
}
