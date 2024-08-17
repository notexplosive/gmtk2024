using System.Collections.Generic;
using ExplogineCore.Data;
using GMTK24.Config;

namespace GMTK24.Model;

public class Blueprint
{
    private readonly List<StructurePlan> _structures;
    private static int idPool;
    private int _structureIndex;
    private readonly Noise _noise;

    public Blueprint(List<StructurePlan> structure)
    {
        _structures = structure;
        Id = idPool++;
        _noise = new Noise(Id);
    }

    public int Id { get; }

    public StructurePlan CurrentStructure()
    {
        return _structures[_noise.PositiveIntAt(_structureIndex, _structures.Count)];
    }

    public void IncrementStructure()
    {
        _structureIndex++;
    }
}