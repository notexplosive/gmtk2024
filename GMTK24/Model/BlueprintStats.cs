using System.Collections.Generic;
using GMTK24.Config;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class BlueprintStats
{
    
    [JsonProperty("title")]
    public string Title { get; set; } = "Title";

    [JsonProperty("description")]
    public string Description { get; set; } = "Lorem ipsum";
    
    
    [JsonProperty("onConstructDelta")]
    public List<ResourceDelta> OnConstructDelta { get; set; } = new();
    
    [JsonProperty("onSecondDelta")]
    public List<ResourceDelta> OnSecondDelta { get; set; } = new();

    [JsonProperty("cost")]
    public List<ResourceDelta> Cost { get; set; } = new();

    [JsonProperty("icon")]
    public string? ButtonIconName { get; set; }
}
