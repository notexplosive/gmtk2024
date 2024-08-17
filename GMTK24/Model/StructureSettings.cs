using Newtonsoft.Json;

namespace GMTK24.Model;

[JsonObject(MemberSerialization.OptIn)]
public class StructureSettings
{
    [JsonProperty("drawDescription")]
    public StructureDrawDescription DrawDescription { get; set; } = new();

    [JsonProperty("createsScaffold")]
    public bool CreatesScaffold { get; set; } = true;
}
