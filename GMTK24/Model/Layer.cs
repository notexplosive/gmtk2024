using System.Collections.Generic;
using System.Linq;
using GMTK24.Config;

namespace GMTK24.Model;

public enum BuildResult
{
    Success,
    FailedBecauseOfFit,
    FailedBecauseOfStructure,
    FailedBecauseOfCost
}

public class Layer
{
    private readonly Dictionary<Cell, Structure> _cellToStructure = new();
    private readonly List<Structure> _structures = new();
    private List<ScaffoldCell>? _scaffoldCache;
    private List<Cell>? _supportedCache;

    public IEnumerable<Structure> Structures => _structures;

    public Structure AddStructureToLayer(Cell centerCell, StructurePlan plan, Blueprint blueprint)
    {
        var realStructure = plan.BuildReal(centerCell, blueprint);
        _structures.Add(realStructure);
        ClearScaffoldCache();
        _supportedCache = null;

        foreach (var cell in realStructure.OccupiedCells)
        {
            _cellToStructure.Add(cell, realStructure);
        }

        return realStructure;
    }

    public void ClearScaffoldCache()
    {
        _scaffoldCache = null;
    }

    public bool IsStructurallySupported(Cell centerCell, StructurePlan plan)
    {
        var structure = plan.BuildReal(centerCell, new Blueprint());
        var actualSupports = 0;

        foreach (var bottomCell in structure.CellsNeedingSupport)
        {
            var belowCell = bottomCell + new Cell(0, 1);
            if (SupportedCells().Contains(belowCell))
            {
                actualSupports++;
            }
        }

        return plan.Settings.RequiredSupports <= actualSupports;
    }

    public bool CanFit(Cell centerCell, StructurePlan plan)
    {
        var structure = plan.BuildReal(centerCell, new Blueprint());
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
        return _cellToStructure.GetValueOrDefault(cell);
    }

    private IEnumerable<ScaffoldCell> GenerateScaffoldCells()
    {
        foreach (var structure in _structures)
        {
            if (!structure.IsVisible)
            {
                continue;
            }
            
            foreach (var startingAnchorPoint in structure.ScaffoldAnchorPoints)
            {
                var anchorPoint = startingAnchorPoint + new Cell(0, 1);
                while (anchorPoint.Y <= 0)
                {
                    var foundStructure = GetStructureAt(anchorPoint);
                    if (foundStructure == null || !foundStructure.Settings.BlockScaffoldRaycasts)
                    {
                        var foundStructureBelow = GetStructureAt(anchorPoint + new Cell(0, 1));
                        var foundStructureAbove = GetStructureAt(anchorPoint + new Cell(0, -1));

                        var type = ScaffoldPointType.Middle;
                        
                        if (foundStructureBelow != null && foundStructureBelow.Settings.BlockScaffoldRaycasts)
                        {
                            type = ScaffoldPointType.Bottom;
                        }

                        if (foundStructureAbove != null && foundStructureAbove.Settings.BlockScaffoldRaycasts)
                        {
                            type = ScaffoldPointType.Top;
                        }
                        
                        yield return new ScaffoldCell(anchorPoint, type);
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

    private IEnumerable<Cell> GenerateSupportedCells()
    {
        foreach (var structure in _structures)
        {
            foreach (var cell in structure.CellsProvidingSupport)
            {
                yield return cell;
            }
        }
    }

    public IEnumerable<ScaffoldCell> ScaffoldCells()
    {
        if (_scaffoldCache == null)
        {
            _scaffoldCache = GenerateScaffoldCells().ToList();
        }

        return _scaffoldCache;
    }

    public IEnumerable<Cell> SupportedCells()
    {
        if (_supportedCache == null)
        {
            _supportedCache = GenerateSupportedCells().ToList();
        }

        return _supportedCache;
    }
}