using System.Collections.Generic;
using System.Linq;
using GMTK24.Config;

namespace GMTK24.Model;

public class BuildAttemptResult
{
    public string? FailureMessage { get; }

    public bool IsSuccessful => FailureMessage == null;

    private BuildAttemptResult(string? failureMessage)
    {
        FailureMessage = failureMessage;
    }

    public static BuildAttemptResult Success()
    {
        return new BuildAttemptResult(null);
    }

    public static BuildAttemptResult FailedBecauseOfCost(Inventory inventory, List<ResourceDelta> costs)
    {
        var shortResources = new List<Resource>();
        foreach (var cost in costs)
        {
            var resource = inventory.GetResource(cost.ResourceName);

            if (resource.Quantity < cost.Amount)
            {
                shortResources.Add(resource);
            }
        }

        var resources = string.Join(" ", shortResources.Select(a=> $"[color(ffffff)]{a.InlineTextIcon(2)}[/color]"));
        
        return new BuildAttemptResult($"Not Enough {resources}");
    }

    public static BuildAttemptResult FailedBecauseOfFit { get; } = new("Not Enough Space");
    public static BuildAttemptResult FailedBecauseOfStructure { get; } = new("Needs Structural Support");
}
