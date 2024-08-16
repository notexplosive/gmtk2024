using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Layout;
using Microsoft.Xna.Framework;

namespace GMTK24.UserInterface;

public class UiLayoutBuilder
{
    private readonly List<BuildAction> _buildActions = new();

    public void AddBuildAction(BuildAction button)
    {
        _buildActions.Add(button);
    }

    public Ui Build()
    {
        var buttonHeight = 100;
        var buttonWidth = 256;

        var layoutBuilder = new LayoutBuilder(new Style(Orientation.Vertical));
        layoutBuilder.Add(L.FillBoth());
        var buttonRibbonLayoutGroup = layoutBuilder.AddGroup(new Style(Alignment: Alignment.Center, PaddingBetweenElements: 20),
            L.FillHorizontal("button-ribbon",buttonHeight + 10));

        foreach (var buildAction in _buildActions)
        {
            buttonRibbonLayoutGroup.Add(L.FixedElement(GetId(buildAction), buttonWidth, buttonHeight));
        }

        var result = layoutBuilder.Bake(new Point(1920, 1080));

        var ui = new Ui(result.FindElement("button-ribbon").Rectangle);
        foreach (var buildAction in _buildActions)
        {
            ui.AddButton(new StructureButton(result.FindElement(GetId(buildAction)).Rectangle, buildAction));
        }

        return ui;
    }

    private static string GetId(BuildAction button)
    {
        return button.Blueprint.Id.ToString();
    }
}