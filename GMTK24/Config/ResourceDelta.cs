using Newtonsoft.Json;

namespace GMTK24.Config;

public class ResourceDelta
{
    [JsonProperty("resourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public int Amount { get; set; } = 0;
}
