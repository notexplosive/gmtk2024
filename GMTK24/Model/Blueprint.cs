namespace GMTK24.Model;

public class Blueprint
{
    private readonly PlannedStructure _structure;
    private static int idPool;

    public Blueprint(PlannedStructure structure)
    {
        _structure = structure;
        Id = idPool++;
    }

    public int Id { get; }

    public PlannedStructure NextStructure()
    {
        return _structure;
    }
}