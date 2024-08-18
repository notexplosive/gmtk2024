using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExplogineMonoGame.Data;
using GMTK24.Config;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class StructureButton : IHoverable
{
    public StructureButton(RectangleF rectangle, string blueprintName, Blueprint blueprint, bool isLocked)
    {
        BlueprintName = blueprintName;
        IsLocked = isLocked;
        Blueprint = blueprint;
        Rectangle = rectangle;
    }

    public string BlueprintName { get; }
    public bool IsLocked { get; }
    public Blueprint Blueprint { get; }
    public RectangleF Rectangle { get; }

    public TooltipContent GetTooltip()
    {
        var costString = DisplayCost(Blueprint.Stats().Cost);

        if (costString == string.Empty)
        {
            costString = "Free";
        }

        return new TooltipContent
        {
            Title = Blueprint.Stats().Title,
            Body = Blueprint.Stats().Description,
            Cost = costString
        };
    }

    private string DisplayCost(List<ResourceDelta> deltas)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(string.Join("  ", deltas.Select(delta => delta.Amount + $"#{delta.ResourceName}")));

        return stringBuilder.ToString();
    }
}
