using System.Collections.Generic;
using System.Linq;

namespace GMTK24.Model;

public class Layer
{
    private readonly List<Structure> _structures = new();

    public IEnumerable<Structure> Structures => _structures;

    public void AddStructure(Cell centerCell, PlannedStructure plan)
    {
        if(CanFit(centerCell, plan))
        {
            _structures.Add(plan.BuildReal(centerCell));
        }
    }

    public bool CanFit(Cell centerCell, PlannedStructure plan)
    {
        var structure = plan.BuildReal(centerCell);
        foreach (var newCell in structure.OccupiedCells)
        {
            foreach (var existingCell in ExistingCells())
            {
                if (newCell == existingCell)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerable<Cell> ExistingCells()
    {
        return _structures.SelectMany(existingCell => existingCell.OccupiedCells);
    }
}
