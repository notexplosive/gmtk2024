using ExplogineMonoGame;
using GMTK24.Config;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class UiState
{
    public StructureButton? SelectedButton { get; private set; }
    public IHoverable? HoveredItem { get; private set; }

    public void ClearHover()
    {
        HoveredItem = null;
    }

    public void SetHovered(IHoverable button)
    {
        HoveredItem = button;
    }

    public void SelectHoveredButton()
    {
        if (HoveredItem is StructureButton structureButton && !structureButton.IsLocked)
        {
            SelectedButton = structureButton;
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

    public void ClearSelection()
    {
        SelectedButton = null;
    }
}
