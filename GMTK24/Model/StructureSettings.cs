namespace GMTK24.Model;

public class StructureSettings
{
    public StructureDrawDescription DrawDescription { get; init; } = new();

    public bool ShouldScaffold { get; init; } = true;
}
