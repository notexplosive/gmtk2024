using System.Linq;
using GMTK24.Config;
using GMTK24.Model;
using GMTK24.UserInterface;

namespace GMTK24;

public class Objective
{
    private readonly LevelCompletionCriteria _criteria;

    public Objective(LevelCompletionCriteria levelCompletionCriteria)
    {
        _criteria = levelCompletionCriteria;
    }

    public bool IsComplete(Ui ui, Inventory inventory, World world)
    {
        var hasBuildCriteria = _criteria.RequiredStructures != null;
        var hasResourceCriteria = _criteria.RequiredResources != null;

        var resourceCriteriaSatisfied = !hasResourceCriteria;
        var buildCriteriaSatisfied = !hasBuildCriteria;

        if (hasBuildCriteria)
        {
            var blueprint = ui.GetBlueprint(_criteria.RequiredStructures!.BlueprintName);
            var matchingStructures = world.AllStructures().Count(a => a.Blueprint == blueprint);
            buildCriteriaSatisfied = matchingStructures >= _criteria.RequiredStructures.TargetQuantity;
        }

        if (hasResourceCriteria)
        {
            var currentResource = inventory.GetResource(_criteria.RequiredResources!.ResourceName);
            resourceCriteriaSatisfied = currentResource.Quantity >= _criteria.RequiredResources.TargetQuantity;
        }

        return buildCriteriaSatisfied && resourceCriteriaSatisfied;
    }
}
