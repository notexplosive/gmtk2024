using System.Collections.Generic;
using System.Text;
using GMTK24.Config;
using GMTK24.UserInterface;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class BlueprintStats
{
    
    [JsonProperty("title")]
    public string Title { get; set; } = "Title";

    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonProperty("onConstructDelta")]
    public List<ResourceDelta> OnConstructDelta { get; set; } = new();
    
    [JsonProperty("onSecondDelta")]
    public List<ResourceDelta> OnSecondDelta { get; set; } = new();

    [JsonProperty("cost")]
    public List<ResourceDelta> Cost { get; set; } = new();

    [JsonProperty("icon")]
    public string? ButtonIconName { get; set; }

    [JsonProperty("sounds")]
    public List<string> Sounds { get; set; } = new();

    [JsonProperty("ambientSound")]
    public string? AmbientSound { get; set; } = null;

    public string GenerateDescription()
    {
        var result = new StringBuilder();
        if (OnConstructDelta.Count > 0)
        {
            result.AppendLine($"Creates {StructureButton.DisplayDelta(OnConstructDelta," and ")}.");
        }

        if (OnSecondDelta.Count > 0)
        {
            result.AppendLine($"Generates {StructureButton.DisplayDelta(OnSecondDelta," and ")} per second.");
        }

        if (Description != null)
        {
            result.AppendLine(Description);
        }

        return result.ToString().Trim();
    }
}
