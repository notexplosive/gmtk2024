using System.Collections.Generic;
using System.Linq;

namespace GMTK24.Model;

public class PlannedStructure
{
    private readonly StructureSettings _settings;
    private readonly HashSet<Cell> _pendingCells;
    private readonly HashSet<Cell> _scaffoldAnchorPoints = new();

    public PlannedStructure(HashSet<Cell> pendingCells, StructureSettings settings)
    {
        _pendingCells = pendingCells;
        _settings = settings;

        var leftX = _pendingCells.MinBy(a => a.X).X;
        var rightX = _pendingCells.MaxBy(a => a.X).X;

        var bottomLeft = _pendingCells.Where(a => a.X == leftX).MaxBy(a => a.Y);
        var bottomRight = _pendingCells.Where(a => a.X == rightX).MaxBy(a => a.Y);

        _scaffoldAnchorPoints.Add(bottomLeft + new Cell(0, 1));
        _scaffoldAnchorPoints.Add(bottomRight + new Cell(0, 1));
    }

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, _pendingCells, _scaffoldAnchorPoints, _settings);
    }
}
