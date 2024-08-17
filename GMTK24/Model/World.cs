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

        if (!inventory.CanAfford(plannedBlueprint.Cost))
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

    public void AddStructure(Cell centerCell, StructurePlan plan, Blueprint blueprint)
    {
        DeducePlacingLayer(plan).AddStructureToLayer(centerCell, plan, blueprint);
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
