using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class PlannedStructure
{
    [field: JsonProperty("settings")]
    public StructureSettings Settings { get; init; } = new();

    [field: JsonProperty("cells")]
    public HashSet<Cell> PendingCells { get; init; } = new();

    [JsonProperty("scaffoldAnchors")]
    public HashSet<Cell> ScaffoldAnchorPoints { get; init; } = new();

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, PendingCells, ScaffoldAnchorPoints, Settings);
    }
}
