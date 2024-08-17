using System.Collections.Generic;
using ExplogineCore.Data;

namespace GMTK24.Model;

public class Blueprint
{
    private readonly List<PlannedStructure> _structures;
    private static int idPool;
    private int _structureIndex;
    private readonly Noise _noise;

    public Blueprint(List<PlannedStructure> structure)
    {
        _structures = structure;
        Id = idPool++;
        _noise = new Noise(Id);
    }

    public int Id { get; }

    public PlannedStructure CurrentStructure()
    {
        return _structures[_noise.PositiveIntAt(_structureIndex, _structures.Count)];
    }

    public void IncrementStructure()
    {
        _structureIndex++;
    }
}