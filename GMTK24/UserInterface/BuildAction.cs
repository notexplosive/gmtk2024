using GMTK24.Model;

namespace GMTK24.UserInterface;

public class BuildAction
{
    public BuildAction(Blueprint blueprint)
    {
        Blueprint = blueprint;
    }

    public Blueprint Blueprint { get; }
}