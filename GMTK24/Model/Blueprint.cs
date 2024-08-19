using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using GMTK24.Config;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class Blueprint
{
    private static int idPool;
    private int? _id;
    private int _structureIndex;
    private BlueprintStats? _cachedStats;

    [JsonProperty("structurePlans")]
    public List<string> PlanNames { get; set; } = new();

    [JsonProperty("stats")]
    public string StatsName { get; set; } = string.Empty;

    public BlueprintStats Stats()
    {
        if (_cachedStats == null)
        {
            var statsFolder = Client.Debug.RepoFileSystem.GetDirectory("Resource/stats");
            _cachedStats = JsonFileReader.ReadOrDefault<BlueprintStats>(statsFolder, StatsName);
        }
        
        return _cachedStats;
    }
    
    [JsonIgnore]
    public List<StructurePlan> Plans { get; } = new();


    public int Id()
    {
        if (_id == null)
        {
            _id = idPool++;
        }

        return _id.Value;
    }

    private Noise Noise()
    {
        return new Noise(Id());
    }

    public StructurePlan CurrentStructure()
    {
        InitializeStructures();
        return Plans[Noise().PositiveIntAt(_structureIndex, Plans.Count)];
    }

    private void InitializeStructures()
    {
        if (Plans.Count != 0)
        {
            return;
        }

        foreach (var planName in PlanNames)
        {
            Plans.Add(JsonFileReader.ReadPlan(planName));
        }
    }

    public void IncrementStructure()
    {
        _structureIndex++;
    }
}
