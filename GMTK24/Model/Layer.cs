using System.Collections.Generic;
using System.Linq;

namespace GMTK24.Model;

public enum BuildResult
{
    Success,
    FailedBecauseOfFit,
    FailedBecauseOfStructure
}

public class Layer
{
    private readonly List<Structure> _structures = new();
    private readonly Dictionary<Cell, Structure> _cellToStructure = new();
    private List<Cell>? _scaffoldCache;

    public IEnumerable<Structure> Structures => _structures;

    public BuildResult CanBuild(Cell centerCell, PlannedStructure plan)
    {
        if (!CanFit(centerCell, plan))
        {
            return BuildResult.FailedBecauseOfFit;
        }

        if (!IsStructurallySupported(centerCell, plan))
        {
            return BuildResult.FailedBecauseOfStructure;
        }

        return BuildResult.Success;
    }

    public void AddStructure(Cell centerCell, PlannedStructure plan)
    {
        var realStructure = plan.BuildReal(centerCell);
        _structures.Add(realStructure);
        _scaffoldCache = null;

        foreach (var cell in realStructure.OccupiedCells)
        {
            _cellToStructure.Add(cell, realStructure);
        }
    }

    private bool IsStructurallySupported(Cell centerCell, PlannedStructure plan)
    {
        var structure = plan.BuildReal(centerCell);
        var actualSupports = 0;

        foreach (var bottomCell in structure.BottomCells())
        {
            var belowCell = bottomCell + new Cell(0, 1);
            if (GetStructureAt(belowCell)?.Settings.ProvidesSupport == true)
            {
                actualSupports++;
            }
        }

        return plan.Settings.RequiredSupports <= actualSupports;
    }

    private bool CanFit(Cell centerCell, PlannedStructure plan)
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
        foreach (var structure in _structures.Where(a => a.Settings.CreatesScaffold))
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
