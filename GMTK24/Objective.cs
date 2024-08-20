using System.Linq;
using GMTK24.Config;
using GMTK24.Model;
using GMTK24.UserInterface;

namespace GMTK24;

public class Objective
{
    public LevelCompletionCriteria Criteria { get; }

    public Objective(LevelCompletionCriteria levelCompletionCriteria)
    {
        Criteria = levelCompletionCriteria;
    }

    public bool IsComplete(Ui ui, Inventory inventory, World world)
    {
        var hasBuildCriteria = Criteria.RequiredStructures != null;
        var hasResourceCriteria = Criteria.RequiredResources != null;

        var resourceCriteriaSatisfied = !hasResourceCriteria;
        var buildCriteriaSatisfied = !hasBuildCriteria;

        if (hasBuildCriteria)
        {
            var blueprint = ui.GetBlueprint(Criteria.RequiredStructures!.BlueprintName);
            var matchingStructures = world.AllStructures().Count(a => a.Blueprint == blueprint);
            buildCriteriaSatisfied = matchingStructures >= Criteria.RequiredStructures.TargetQuantity;
        }

        if (hasResourceCriteria)
        {
            var currentResource = inventory.GetResource(Criteria.RequiredResources!.ResourceName);
            resourceCriteriaSatisfied = currentResource.Quantity >= Criteria.RequiredResources.TargetQuantity;
        }

        return buildCriteriaSatisfied && resourceCriteriaSatisfied;
    }
}
