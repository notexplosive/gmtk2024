using System.Collections.Generic;
using System.Linq;

namespace GMTK24.Model;

public class Layer
{
    private readonly List<Structure> _structures = new();
    private List<Cell>? _scaffoldCache;
    private Dictionary<Cell, Structure> _cellToStructure = new();

    public IEnumerable<Structure> Structures => _structures;

    public bool AddStructure(Cell centerCell, PlannedStructure plan)
    {
        if(CanFit(centerCell, plan))
        {
            var realStructure = plan.BuildReal(centerCell);
            _structures.Add(realStructure);
            _scaffoldCache = null;

            foreach (var cell in realStructure.OccupiedCells)
            {
                _cellToStructure.Add(cell, realStructure);
            }

            return true;
        }

        return false;
    }

    public bool CanFit(Cell centerCell, PlannedStructure plan)
    {
        var structure = plan.BuildReal(centerCell);
        foreach (var newCell in structure.OccupiedCells)
        {
            foreach (var existingCell in OccupiedCells())
            {
                if (newCell == existingCell)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerable<Cell> OccupiedCells()
    {
        return _cellToStructure.Keys;
    }

    public bool IsOccupiedAt(Cell cell)
    {
        return OccupiedCells().Contains(cell);
    }

    public Structure? GetStructureAt(Cell cell)
    {
        foreach (var (structureCell, structure) in _cellToStructure)
        {
            if (structureCell == cell)
            {
                return structure;
            }
        }

        return null;
    }

    private IEnumerable<Cell> GenerateScaffoldCells()
    {
        foreach (var structure in _structures.Where(a=>a.Settings.CreatesScaffold))
        {
            foreach (var startingAnchorPoint in structure.ScaffoldAnchorPoints)
            {
                var anchorPoint = startingAnchorPoint + new Cell(0, 1);
                while (anchorPoint.Y < 0)
                {
                    var foundStructure = GetStructureAt(anchorPoint);
                    if (foundStructure == null || !foundStructure.Settings.CreatesScaffold)
                    {
                        yield return anchorPoint;
                    }
                    else
                    {
                        break;
                    }

                    anchorPoint += new Cell(0, 1);
                }
            }
        }
    }

    public IEnumerable<Cell> ScaffoldCells()
    {
        if (_scaffoldCache == null)
        {
            _scaffoldCache = GenerateScaffoldCells().ToList();
        }
        return _scaffoldCache;
    }
}
