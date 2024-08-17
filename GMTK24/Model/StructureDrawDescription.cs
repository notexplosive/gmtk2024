using Newtonsoft.Json;

namespace GMTK24.Model;

[JsonObject(MemberSerialization.OptIn)]
public class StructureDrawDescription
{
    [JsonProperty("textureName")]
    public string? TextureName { get; init; }
    
    [JsonProperty("graphicsTopLeft")]
    public Cell GraphicTopLeft { get; init; }
}
