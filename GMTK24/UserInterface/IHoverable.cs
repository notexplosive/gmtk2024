using GMTK24.Model;

namespace GMTK24.UserInterface;

public interface IHoverable
{
    public TooltipContent GetTooltip(Inventory inventory);
}