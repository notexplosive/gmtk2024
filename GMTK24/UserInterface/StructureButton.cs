using ExplogineMonoGame.Data;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class StructureButton : IHoverable
{
    public Blueprint Blueprint { get; }
    public RectangleF Rectangle { get; }

    public StructureButton(RectangleF rectangle, Blueprint blueprint)
    {
        Blueprint = blueprint;
        Rectangle = rectangle;
    }

    public TooltipContent GetTooltip()
    {
        return new TooltipContent()
        {
            Title = Blueprint.Title,
            Body = Blueprint.Description
        };
    }
}
