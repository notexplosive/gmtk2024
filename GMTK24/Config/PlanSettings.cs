using GMTK24.Model;
using Newtonsoft.Json;

namespace GMTK24.Config;

[JsonObject(MemberSerialization.OptIn)]
public class PlanSettings
{
    [JsonProperty("drawDescription")]
    public StructureDrawDescription DrawDescription { get; set; } = new();

    [JsonProperty("requiredSupports")]
    public int RequiredSupports { get; set; } = 1;

    [JsonProperty("layer")]
    public StructureLayer StructureLayer { get; set; } = StructureLayer.Main;

    [JsonProperty("blockScaffold")]
    public bool BlockScaffoldRaycasts { get; set; } = true;
}
