using System.Collections.Generic;
using System.Linq;
using GMTK24.Config;

namespace GMTK24.Model;

public class BuildAttemptResult
{
    private BuildAttemptResult(BuildAttemptResultType resultType, string? failureMessage)
    {
        ResultType = resultType;
        FailureMessage = failureMessage;
    }

    public BuildAttemptResultType ResultType { get; }
    public string? FailureMessage { get; }

    public bool IsSuccessful => ResultType == BuildAttemptResultType.Success;

    public static BuildAttemptResult FailedBecauseOfFit { get; } =
        new(BuildAttemptResultType.CannotFit, "Not Enough Space");

    public static BuildAttemptResult FailedBecauseOfStructure { get; } =
        new(BuildAttemptResultType.NoSupport, "Needs Structural Support");

    public static BuildAttemptResult Success()
    {
        return new BuildAttemptResult(BuildAttemptResultType.Success, null);
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

        var resources = string.Join(" ", shortResources.Select(a => $"[color(ffffff)]{a.InlineTextIcon(2)}[/color]"));

        return new BuildAttemptResult(BuildAttemptResultType.CantAfford, $"Not Enough {resources}");
    }
}

public enum BuildAttemptResultType
{
    Success,
    CannotFit,
    NoSupport,
    CantAfford
}
