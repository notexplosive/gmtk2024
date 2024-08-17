using System.Collections.Generic;

namespace GMTK24.Model;

public class StructureBuilder
{
    private readonly HashSet<Cell> _pendingCells = new();

    public StructureBuilder AddCell(int x, int y)
    {
        _pendingCells.Add(new Cell(x, y));
        return this;
    }

    public PlannedStructure BuildPlan(StructureDrawDescription drawDescription)
    {
        return new PlannedStructure(_pendingCells, drawDescription);
    }
}
