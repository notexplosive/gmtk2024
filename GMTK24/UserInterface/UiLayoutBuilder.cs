using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Layout;
using GMTK24.Model;
using Microsoft.Xna.Framework;

namespace GMTK24.UserInterface;

public class UiLayoutBuilder
{
    private readonly List<Blueprint> _buildActions = new();
    private readonly List<Resource> _resources = new();

    public void AddBuildAction(Blueprint button)
    {
        _buildActions.Add(button);
    }

    public void AddResource(Resource resource)
    {
        _resources.Add(resource);
    }

    public Ui Build()
    {
        var resourceWidth = 300;
        var resourceHeight = 80;

        var buttonWidth = 256;
        var buttonHeight = 100;

        var layoutBuilder = new LayoutBuilder(new Style(Orientation.Vertical));

        var resourcesLayoutGroup = layoutBuilder.AddGroup(
            new Style(Alignment: Alignment.Center, PaddingBetweenElements: 20),
            L.FillHorizontal("resource-ribbon", resourceHeight + 10));

        foreach (var resource in _resources)
        {
            var resourceGroup = resourcesLayoutGroup.AddGroup(new Style(), L.FixedElement(GetId(resource), resourceWidth, resourceHeight));
            resourceGroup.Add(L.FillVertical(GetId(resource) + "_icon", resourceHeight));
            resourceGroup.Add(L.FillBoth(GetId(resource) + "_text"));
        }
        
        layoutBuilder.Add(L.FillBoth());
        
        var buttonRibbonLayoutGroup = layoutBuilder.AddGroup(
            new Style(Alignment: Alignment.Center, PaddingBetweenElements: 20),
            L.FillHorizontal("button-ribbon", buttonHeight + 10));

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

        foreach (var resource in _resources)
        {
            var textRectangle = result.FindElement(GetId(resource) + "_text").Rectangle;
            var iconRectangle = result.FindElement(GetId(resource) + "_icon").Rectangle;
            ui.AddResource(new ResourceTracker(textRectangle, iconRectangle, resource));
        }

        return ui;
    }

    private string GetId(Resource resource)
    {
        return resource.Id.ToString();
    }

    private static string GetId(Blueprint blueprint)
    {
        return blueprint.Id.ToString();
    }
}
