namespace GMTK24.Model;

public class World
{
    public Layer DecorationLayer = new();
    public Layer MainLayer = new();

    public BuildResult CanBuild(Cell centerCell, PlannedStructure plan)
    {
        var placingLayer = DeducePlacingLayer(plan);

        if (!placingLayer.CanFit(centerCell, plan))
        {
            return BuildResult.FailedBecauseOfFit;
        }

        if (!MainLayer.IsStructurallySupported(centerCell, plan))
        {
            return BuildResult.FailedBecauseOfStructure;
        }

        return BuildResult.Success;
    }

    private Layer DeducePlacingLayer(PlannedStructure plan)
    {
        var placingLayer = MainLayer;

        if (plan.Settings.StructureLayer == StructureLayer.Decoration)
        {
            placingLayer = DecorationLayer;
        }

        return placingLayer;
    }

    public void AddStructure(Cell centerCell, PlannedStructure plan)
    {
        DeducePlacingLayer(plan).AddStructureToLayer(centerCell, plan);
    }
}
