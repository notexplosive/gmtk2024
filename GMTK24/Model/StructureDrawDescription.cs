using System.Collections.Generic;
using Newtonsoft.Json;

namespace GMTK24.Model;

[JsonObject(MemberSerialization.OptIn)]
public class StructureDrawDescription
{
    [JsonProperty("textureName")]
    public string? TextureName { get; set; }
    
    [JsonProperty("frames")]
    public List<string>? FrameNames { get; set; }
    
    [JsonProperty("graphicsTopLeft")]
    public Cell GraphicTopLeft { get; set; }
}
