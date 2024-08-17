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
    private readonly List<ResourceTracker> _resourceTrackers = new();

    public Ui(RectangleF buttonBackground)
    {
        _buttonBackground = buttonBackground;
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

        painter.DrawRectangle(_buttonBackground,
            new DrawSettings {Color = Color.Yellow.DimmedBy(0.25f), Depth = Depth.Back});

        foreach (var button in _buttons)
        {
            var color = Color.White;
            var offset = Vector2.Zero;
            if (State.HoveredButton == button)
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
            painter.DrawRectangle(resourceTracker.TextRectangle, new DrawSettings{Color = Color.White, Depth = Depth.Back});
            painter.DrawRectangle(resourceTracker.IconRectangle, new DrawSettings{Color = Color.DarkBlue, Depth = Depth.Back- 1});
            painter.DrawStringWithinRectangle(Client.Assets.GetFont("gmtk/GameFont",50),resourceTracker.Resource.Status(),resourceTracker.TextRectangle, Alignment.Center, new DrawSettings{Color = Color.Black});
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
