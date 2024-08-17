using System.Collections.Generic;

namespace GMTK24.Model;

public class PlannedStructure
{
    private readonly HashSet<Cell> _pendingCells;
    private readonly StructureDrawDescription _drawDescription;

    public PlannedStructure(HashSet<Cell> pendingCells, StructureDrawDescription drawDescription)
    {
        _pendingCells = pendingCells;
        _drawDescription = drawDescription;
    }

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, _pendingCells, _drawDescription);
    }
}