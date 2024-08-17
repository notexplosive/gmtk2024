using ExplogineMonoGame;
using GMTK24.Config;
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

    public StructurePlan? CurrentStructure()
    {
        if (SelectedButton == null)
        {
            return null;
        }

        return SelectedButton.Blueprint.CurrentStructure();
    }

    public Blueprint? CurrentBlueprint()
    {
        if (SelectedButton == null)
        {
            return null;
        }
        
        return SelectedButton.Blueprint;
    }

    public void IncrementSelectedBlueprint()
    {
        if (SelectedButton == null)
        {
            Client.Debug.LogError("Attempted to increment structure when none was selected");
            return;
        }
        
        SelectedButton.Blueprint.IncrementStructure();
    }
}
