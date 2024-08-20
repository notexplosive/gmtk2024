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
    public float HoverTime { get; set; }

    public static float MaxHoverTime => 0.1f; 

    public TooltipContent GetTooltip()
    {
        var costString = DisplayDelta(Blueprint.Stats().Cost);

        if (costString == string.Empty)
        {
            costString = "Free";
        }

        return new TooltipContent
        {
            Title = Blueprint.Stats().Title,
            Body = Blueprint.Stats().GenerateDescription(),
            Cost = costString
        };
    }

    public static string DisplayDelta(List<ResourceDelta> deltas, string joiner = "  ")
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(string.Join(joiner, deltas.Select(delta =>
        {
            if (delta.AffectCapacity)
            {
                return $"increase max #{delta.ResourceName} by {delta.Amount}"; 
            }
            return delta.Amount + $"#{delta.ResourceName}";
        })));

        return stringBuilder.ToString();
    }
}
