using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24.UserInterface;

public class Ui
{
    private readonly RectangleF _buttonBackground;
    private readonly List<StructureButton> _buttons = new();
    private readonly RectangleF _middleArea;
    private readonly List<ResourceTracker> _resourceTrackers = new();

    public Ui(RectangleF buttonBackground, RectangleF middleArea)
    {
        _buttonBackground = buttonBackground;
        _middleArea = middleArea;
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

    public void Draw(Painter painter)
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
            
            painter.DrawRectangle(resourceTracker.IconRectangle,
                new DrawSettings {Color = Color.Green.DimmedBy(0.3f), Depth = Depth.Back - 1});
            if (iconName != null)
            {
                painter.DrawAtPosition(ResourceAssets.Instance.Textures[iconName], resourceTracker.IconRectangle.Center, Scale2D.One, new DrawSettings{Origin = DrawOrigin.Center, Depth = Depth.Middle});
            }

            painter.DrawStringWithinRectangle(Client.Assets.GetFont("gmtk/GameFont", 50),
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
            var bodyRectangleNormalized = bodyFont.MeasureString(tooltipContent.Body).ToRectangleF()
                .Moved(titleRectangleNormalized.Size.JustY());

            var tooltipSize = RectangleF.Union(titleRectangleNormalized, bodyRectangleNormalized).Size;

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

            painter.DrawRectangle(paddedTooltipRectangle,
                new DrawSettings {Depth = 200, Color = Color.DarkBlue.DimmedBy(0.25f).WithMultipliedOpacity(0.75f)});
            painter.DrawLineRectangle(paddedTooltipRectangle,
                new LineDrawSettings {Depth = 190, Color = Color.White, Thickness = 2});
            painter.DrawStringWithinRectangle(titleFont, tooltipContent.Title, titleRectangle, Alignment.TopLeft,
                new DrawSettings {Depth = 100});
            painter.DrawStringWithinRectangle(bodyFont, tooltipContent.Body, bodyRectangle, Alignment.TopLeft,
                new DrawSettings {Depth = 100});
        }

        painter.EndSpriteBatch();
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

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            State.SelectHoveredButton();
        }
    }
}
