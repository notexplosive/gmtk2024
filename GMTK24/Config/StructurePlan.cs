using System.Collections.Generic;
using GMTK24.Model;
using Newtonsoft.Json;

namespace GMTK24.Config;

public class StructurePlan
{
    [JsonProperty("settings")]
    public PlanSettings Settings { get; init; } = new();

    [JsonProperty("cells")]
    public HashSet<Cell> Cells { get; init; } = new();

    [JsonProperty("scaffoldAnchors")]
    public HashSet<Cell> ScaffoldAnchorPoints { get; init; } = new();

    public Structure BuildReal(Cell centerCell)
    {
        return new Structure(centerCell, Cells, ScaffoldAnchorPoints, Settings);
    }
}
