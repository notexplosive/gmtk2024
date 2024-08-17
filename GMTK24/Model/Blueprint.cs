using System.Collections.Generic;
using ExplogineCore.Data;
using GMTK24.Config;
using Newtonsoft.Json;

namespace GMTK24.Model;

public class Blueprint
{
    private static int idPool;
    private int? _id;
    private int _structureIndex;

    [JsonProperty("structurePlans")]
    public List<string> PlanNames { get; set; } = new();

    [JsonProperty("onConstructDelta")]
    public List<ResourceDelta> OnConstructDelta { get; set; } = new();
    
    [JsonProperty("onSecondDelta")]
    public List<ResourceDelta> OnSecondDelta { get; set; } = new();

    [JsonProperty("cost")]
    public List<ResourceDelta> Cost { get; set; } = new();
    
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
