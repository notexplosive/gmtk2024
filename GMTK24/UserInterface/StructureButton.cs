using ExplogineMonoGame.Data;

namespace GMTK24.UserInterface;

public class StructureButton
{
    public BuildAction BuildAction { get; }
    public RectangleF Rectangle { get; }

    public StructureButton(RectangleF rectangle, BuildAction buildAction)
    {
        BuildAction = buildAction;
        Rectangle = rectangle;
    }

    public void OnPress()
    {
        
    }
}
