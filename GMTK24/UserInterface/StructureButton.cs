using ExplogineMonoGame.Data;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class StructureButton
{
    public Blueprint Blueprint { get; }
    public RectangleF Rectangle { get; }

    public StructureButton(RectangleF rectangle, Blueprint blueprint)
    {
        Blueprint = blueprint;
        Rectangle = rectangle;
    }
}
