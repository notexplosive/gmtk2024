using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GMTK24.Config;

[Serializable]
public class LevelSequence
{
    [JsonProperty("levels")]
    public List<Level> Levels { get; set; } = new();
}

[Serializable]
public class Level
{
    [JsonProperty("blueprints")]
    public List<LevelBlueprint> Blueprints = new();
    
    [JsonProperty("completionCriteria")]
    public LevelCompletionCriteria CompletionCriteria = new();
    
    [JsonProperty("introDialogue")]
    public List<string> IntroDialogue { get; set; } = new();
}

[Serializable]
public class LevelCompletionCriteria
{
    [JsonProperty("buildStructures")]
    public BuildStructureCriteria? RequiredStructures { get; set; } = null;
    
    [JsonProperty("getResource")]
    public GetResourceCriteria? RequiredResources { get; set; } = null;
}

[Serializable]
public class GetResourceCriteria
{
    [JsonProperty("targetQuantity")]
    public int TargetQuantity { get; set; }
    
    [JsonProperty("resource")]
    public string ResourceName { get; set; } = string.Empty;
}

[Serializable]
public class BuildStructureCriteria
{
    [JsonProperty("targetQuantity")]
    public int TargetQuantity { get; set; }

    [JsonProperty("blueprint")]
    public string BlueprintName { get; set; } = string.Empty;
}

[Serializable]
public class LevelBlueprint
{
    [JsonProperty("isLocked")]
    public bool IsLocked { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}
