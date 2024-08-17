using Newtonsoft.Json;

namespace GMTK24.Model;

[JsonObject(MemberSerialization.OptIn)]
public class StructureSettings
{
    [JsonProperty("drawDescription")]
    public StructureDrawDescription DrawDescription { get; set; } = new();

    [JsonProperty("createsScaffold")]
    public bool CreatesScaffold { get; set; } = true;

    [JsonProperty("providesSupport")]
    public bool ProvidesSupport { get; set; } = true;

    [JsonProperty("requiredSupports")]
    public int RequiredSupports { get; set; } = 1;
    
    [JsonProperty("layer")]
    public StructureLayer StructureLayer { get; set; } = StructureLayer.Main;
}