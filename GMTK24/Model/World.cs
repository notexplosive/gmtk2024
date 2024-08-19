using System.Collections.Generic;
using GMTK24.Config;

namespace GMTK24.Model;

public class World
{
    public Layer DecorationLayer = new();
    public Layer MainLayer = new();

    public BuildResult CanBuild(Cell centerCell, StructurePlan plan, Inventory inventory, Blueprint plannedBlueprint)
    {
        var placingLayer = DeducePlacingLayer(plan);

        if (!inventory.CanAfford(plannedBlueprint.Stats().Cost))
        {
            return BuildResult.FailedBecauseOfCost;
        }
        
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

    private Layer DeducePlacingLayer(StructurePlan plan)
    {
        var placingLayer = MainLayer;

        if (plan.Settings.StructureLayer == StructureLayer.Decoration)
        {
            placingLayer = DecorationLayer;
        }

        return placingLayer;
    }

    public Structure AddStructure(Cell centerCell, StructurePlan plan, Blueprint blueprint)
    {
        var layer = DeducePlacingLayer(plan);
        var structure =  layer.AddStructureToLayer(centerCell, plan, blueprint);
        structure.VisibilityChanged += () =>
        {
            layer.ClearScaffoldCache();
        };
        return structure;
    }

    public IEnumerable<Structure> AllStructures()
    {
        foreach (var item in MainLayer.Structures)
        {
            yield return item;
        }
        
        foreach (var item in DecorationLayer.Structures)
        {
            yield return item;
        }
    }
}
