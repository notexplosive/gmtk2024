using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExplogineMonoGame.Data;
using GMTK24.Config;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class StructureButton : IHoverable
{
    public StructureButton(RectangleF rectangle, Blueprint blueprint)
    {
        Blueprint = blueprint;
        Rectangle = rectangle;
    }

    public Blueprint Blueprint { get; }
    public RectangleF Rectangle { get; }

    public TooltipContent GetTooltip()
    {
        return new TooltipContent
        {
            Title = Blueprint.Title,
            Body = Blueprint.Description,
            Cost = DisplayCost(Blueprint.Cost)
        };
    }

    private string DisplayCost(List<ResourceDelta> deltas)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(string.Join("  ", deltas.Select(delta => delta.Amount + $"#{delta.ResourceName}")));

        return stringBuilder.ToString();
    }
}
