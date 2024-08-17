using System.Collections.Generic;

namespace GMTK24.Model;

public class PlannedStructureBuilder
{
    private readonly HashSet<Cell> _pendingCells = new();

    public PlannedStructureBuilder AddCell(int x, int y)
    {
        _pendingCells.Add(new Cell(x, y));
        return this;
    }

    public PlannedStructure BuildPlan(StructureSettings settings)
    {
        return new PlannedStructure(_pendingCells, settings);
    }
}