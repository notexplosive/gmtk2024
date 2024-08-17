using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GMTK24.Model;

[JsonObject(MemberSerialization.OptIn)]
public class PlannedStructure
{
    [field: JsonProperty("settings")]
    public StructureSettings Settings { get; init; }

    [field: JsonProperty("cells")]
    public HashSet<Cell> PendingCells { get; init; }

    [JsonProperty("scaffoldAnchors")]
    public HashSet<Cell> ScaffoldAnchorPoints { get; init; } = new();

    [JsonConstructor]
    public PlannedStructure(HashSet<Cell> cells, StructureSettings settings)
    {
        PendingCells = cells;
        Settings = settings;

        var leftX = PendingCells.MinBy(a => a.X).X;
        var rightX = PendingCells.MaxBy(a => a.X).X;

        var bottomLeft = PendingCells.Where(a => a.X == leftX).MaxBy(a => a.Y);
        var bottomRight = PendingCells.Where(a => a.X == rightX).MaxBy(a => a.Y);

        ScaffoldAnchorPoints.Add(bottomLeft + new Cell(0, 1));
        ScaffoldAnchorPoints.Add(bottomRight + new Cell(0, 1));
    }

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, PendingCells, ScaffoldAnchorPoints, Settings);
    }
}
