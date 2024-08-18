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

    public void AddBlueprint(Blueprint button)
    {
        _buildActions.Add(button);
    }

    public void AddResource(Resource resource)
    {
        _resources.Add(resource);
    }

    public Ui Build(Point screenSize)
    {
        var resourceWidth = 250;
        var resourceHeight = 40;

        var buttonWidth = 150;
        var buttonHeight = 60;
        
        var buttonRibbonHeight = 110;

        var layoutBuilder = new LayoutBuilder(new Style(Orientation.Vertical));

        var topGroup = layoutBuilder.AddGroup(new Style(Margin: new Vector2(20)), L.FillHorizontal(resourceHeight));
        
        topGroup.Add(L.FixedElement("top-corner",0,0));
        
        var resourcesLayoutGroup = topGroup.AddGroup(
            new Style(Alignment: Alignment.TopCenter, PaddingBetweenElements: 20),
            L.FillBoth("resource-ribbon"));
        
        foreach (var resource in _resources)
        {
            var resourceGroup = resourcesLayoutGroup.AddGroup(new Style(), L.FixedElement(GetId(resource), resourceWidth, resourceHeight));
            resourceGroup.Add(L.FillVertical(GetId(resource) + "_icon", resourceHeight));
            resourceGroup.Add(L.FillBoth(GetId(resource) + "_text"));
        }
        
        layoutBuilder.Add(L.FillBoth("middle-area"));
        
        var buttonRibbonLayoutGroup = layoutBuilder.AddGroup(
            new Style(Alignment: Alignment.Center, PaddingBetweenElements: 20),
            L.FillHorizontal("button-ribbon", buttonRibbonHeight));

        buttonRibbonLayoutGroup.Add(L.FixedElement("rules-button",resourceHeight,resourceHeight));
        
        foreach (var buildAction in _buildActions)
        {
            buttonRibbonLayoutGroup.Add(L.FixedElement(GetId(buildAction), buttonWidth, buttonHeight));
        }

        var result = layoutBuilder.Bake(screenSize);

        var ui = new Ui(
            result.FindElement("button-ribbon").Rectangle,
            result.FindElement("middle-area").Rectangle,
            result.FindElement("top-corner").Rectangle.TopLeft
            );
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
        return blueprint.Id().ToString();
    }
}
