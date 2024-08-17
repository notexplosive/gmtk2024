using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class PlannedStructure
{
    [JsonProperty("settings")]
    public StructureSettings Settings { get; init; } = new();

    [JsonProperty("cells")]
    public HashSet<Cell> Cells { get; init; } = new();

    [JsonProperty("scaffoldAnchors")]
    public HashSet<Cell> ScaffoldAnchorPoints { get; init; } = new();

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, Cells, ScaffoldAnchorPoints, Settings);
    }
}
