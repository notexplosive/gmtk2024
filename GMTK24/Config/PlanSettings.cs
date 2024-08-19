using GMTK24.Model;
using Newtonsoft.Json;

namespace GMTK24.Config;

[JsonObject(MemberSerialization.OptIn)]
public class PlanSettings
{
    [JsonProperty("drawDescription")]
    public StructureDrawDescription DrawDescription { get; set; } = new();

    [JsonProperty("createsScaffold")]
    public bool CreatesScaffold { get; set; } = true;

    [JsonProperty("requiredSupports")]
    public int RequiredSupports { get; set; } = 1;

    [JsonProperty("layer")]
    public StructureLayer StructureLayer { get; set; } = StructureLayer.Main;
}
