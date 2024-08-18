using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using GMTK24.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24.UserInterface;

public class Ui
{
    private readonly RectangleF _buttonBackground;
    private readonly List<StructureButton> _buttons = new();
    private readonly RectangleF _middleArea;
    private readonly List<ResourceTracker> _resourceTrackers = new();
    private readonly Vector2 _rulesCorner;
    private readonly HoverState _isRulesHovered = new();
    
    public event Action? RequestRules;

    public Ui(RectangleF buttonBackground, RectangleF middleArea, Vector2 rulesCorner)
    {
        _buttonBackground = buttonBackground;
        _middleArea = middleArea;
        _rulesCorner = rulesCorner;
    }

    public UiState State { get; } = new();

    public void AddButton(StructureButton structureButton)
    {
        _buttons.Add(structureButton);
    }

    public void AddResource(ResourceTracker tracker)
    {
        _resourceTrackers.Add(tracker);
    }

    public void Draw(Painter painter, Inventory inventory)
    {
        painter.BeginSpriteBatch(SamplerState.LinearWrap);

        painter.DrawAsRectangle(ResourceAssets.Instance.Textures["ui_background"], _buttonBackground,
            new DrawSettings {SourceRectangle = _buttonBackground.MovedToZero().ToRectangle(), Depth = Depth.Back});

        foreach (var button in _buttons)
        {
            var color = Color.White;
            var offset = Vector2.Zero;
            if (State.HoveredItem == button)
            {
                offset = new Vector2(0, -20);
            }

            if (State.SelectedButton == button)
            {
                color = Color.Orange;
            }

            painter.DrawRectangle(button.Rectangle.Moved(offset), new DrawSettings {Color = color});
        }

        foreach (var resourceTracker in _resourceTrackers)
        {
            painter.DrawRectangle(resourceTracker.TextRectangle,
                new DrawSettings {Color = Color.White, Depth = Depth.Back});

            var iconName = resourceTracker.Resource.IconName;
            
            if (iconName != null)
            {
                painter.DrawAtPosition(ResourceAssets.Instance.Textures[iconName], resourceTracker.IconRectangle.Center,
                    Scale2D.One, new DrawSettings {Origin = DrawOrigin.Center, Depth = Depth.Middle});
            }

            painter.DrawStringWithinRectangle(Client.Assets.GetFont("gmtk/GameFont", 35),
                resourceTracker.Resource.Status(), resourceTracker.TextRectangle, Alignment.Center,
                new DrawSettings {Color = Color.Black});
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(SamplerState.LinearWrap);
        if (State.HoveredItem != null)
        {
            var tooltipContent = State.HoveredItem.GetTooltip();

            var titleFont = Client.Assets.GetFont("gmtk/GameFont", 64);
            var titleRectangleNormalized = titleFont.MeasureString(tooltipContent.Title).ToRectangleF();

            var bodyFont = Client.Assets.GetFont("gmtk/GameFont", 32);
            
            var tooltipBodyText = ApplyIcons(inventory, tooltipContent.Body);
            var tooltipCostText = ApplyIcons(inventory, tooltipContent.Cost);
            
            var bodyTextFormatted = FormattedText.FromFormatString(bodyFont, Color.White, tooltipBodyText,
                GameplayConstants.FormattedTextParser);
            
            var costTextFormatted = FormattedText.FromFormatString(bodyFont, Color.White, tooltipCostText,
                GameplayConstants.FormattedTextParser);
            
            var tooltipCostRectangleNormalized = costTextFormatted.MaxNeededSize().ToRectangleF()
                .Moved(titleRectangleNormalized.Size.JustX());
            
            var bodyRectangleNormalized = bodyTextFormatted.MaxNeededSize().ToRectangleF()
                .Moved(titleRectangleNormalized.Size.JustY());

            var tooltipSize = RectangleF.Union(titleRectangleNormalized, RectangleF.Union(bodyRectangleNormalized, tooltipCostRectangleNormalized)).Size + new Vector2(50, 0);

            var paddedTooltipRectangle = RectangleF.FromSizeAlignedWithin(_middleArea.Inflated(-20, -20),
                tooltipSize + new Vector2(25), Alignment.BottomCenter);

            var tooltipRectangle =
                RectangleF.FromSizeAlignedWithin(paddedTooltipRectangle, tooltipSize, Alignment.Center);
            var titleRectangle =
                RectangleF.FromSizeAlignedWithin(tooltipRectangle, titleRectangleNormalized.Size + new Vector2(1),
                    Alignment.TopLeft);
            var bodyRectangle =
                RectangleF.FromSizeAlignedWithin(tooltipRectangle, bodyRectangleNormalized.Size + new Vector2(1),
                    Alignment.BottomLeft);

            var costRectangle =
                RectangleF.FromSizeAlignedWithin(tooltipRectangle, tooltipCostRectangleNormalized.Size + new Vector2(1),
                    Alignment.TopRight);

            painter.DrawRectangle(paddedTooltipRectangle,
                new DrawSettings {Depth = 200, Color = Color.DarkBlue.DimmedBy(0.25f).WithMultipliedOpacity(0.75f)});
            painter.DrawLineRectangle(paddedTooltipRectangle,
                new LineDrawSettings {Depth = 190, Color = Color.White, Thickness = 2});
            painter.DrawStringWithinRectangle(titleFont, tooltipContent.Title, titleRectangle, Alignment.TopLeft,
                new DrawSettings {Depth = 100});
            
            painter.DrawFormattedStringWithinRectangle(bodyTextFormatted, bodyRectangle, Alignment.TopLeft, new DrawSettings());
            painter.DrawFormattedStringWithinRectangle(costTextFormatted, costRectangle, Alignment.TopLeft, new DrawSettings());
        }

        painter.DrawAtPosition(ResourceAssets.Instance.Textures["icon_help"], _rulesCorner, Scale2D.One, new DrawSettings());

        painter.EndSpriteBatch();
    }

    private static string ApplyIcons(Inventory inventory, string tooltipContentBody)
    {
        foreach (var resource in inventory.AllResources())
        {
            tooltipContentBody = tooltipContentBody.Replace($"#{resource.Name}", resource.InlineTextIcon());
        }

        return tooltipContentBody;
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var uiHitTestLayer = hitTestStack.AddLayer(Matrix.Identity, Depth.Middle - 100);

        uiHitTestLayer.AddZone(_buttonBackground, Depth.Back, () => { });

        foreach (var button in _buttons)
        {
            uiHitTestLayer.AddZone(button.Rectangle, Depth.Middle, () => { State.ClearHover(); },
                () => { State.SetHovered(button); });
        }

        uiHitTestLayer.AddZone(new RectangleF(_rulesCorner,ResourceAssets.Instance.Textures["icon_help"].Bounds.Size.ToVector2()), Depth.Middle, _isRulesHovered);

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            if (_isRulesHovered)
            {
                RequestRules?.Invoke();
            }

            State.SelectHoveredButton();
        }
    }
}
