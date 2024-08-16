using GMTK24.Model;

namespace GMTK24.UserInterface;

public class UiState
{
    public StructureButton? SelectedButton { get; private set; }
    public StructureButton? HoveredButton { get; private set; }

    public void ClearHover()
    {
        HoveredButton = null;
    }

    public void SetHovered(StructureButton button)
    {
        HoveredButton = button;
    }

    public void SelectHoveredButton()
    {
        if (HoveredButton != null)
        {
            SelectedButton = HoveredButton;
        }
    }

    public PlannedStructure? CurrentStructure()
    {
        if (SelectedButton == null)
        {
            return null;
        }

        return SelectedButton.BuildAction.Blueprint.NextStructure();
    }
}
