using System.Collections.Generic;
using GMTK24.Model;
using Newtonsoft.Json;

namespace GMTK24.Config;

public class StructurePlan
{
    [JsonProperty("settings")]
    public PlanSettings Settings { get; init; } = new();

    [JsonProperty("cells")]
    public HashSet<Cell> OccupiedCells { get; init; } = new();
    
    [JsonProperty("providesStructureCells")]
    public HashSet<Cell> ProvidesStructureCells { get; init; } = new();
    
    [JsonProperty("requiresSupportCells")]
    public HashSet<Cell> RequiresSupportCells { get; init; } = new();

    [JsonProperty("scaffoldAnchors")]
    public HashSet<Cell> ScaffoldAnchorPoints { get; init; } = new();

    public Structure BuildReal(Cell centerCell, Blueprint blueprint)
    {
        return new Structure(centerCell, this, blueprint);
    }
}
