using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Layout;
using ExTween;
using GMTK24.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24;

public class TitleOverlay : Overlay
{
    private readonly List<GameButtonState> _buttonStates = new();
    private readonly LayoutBuilder _layoutRoot;
    private Point _rememberedSize;
    private readonly IWindow _window;
    private LayoutArrangement _bakedLayout;
    private GameButtonState? _primedButton;

    public event Action? ResolutionChanged;

    public TitleOverlay(IWindow window, Wrapped<bool> muteSfx, Wrapped<bool> muteMusic, Action onPlay)
    {
        _window = window;
        _rememberedSize = _window.RenderResolution;

        _layoutRoot = new LayoutBuilder(new Style
            {Alignment = Alignment.Center, Orientation = Orientation.Vertical, PaddingBetweenElements = 10});
        
        _layoutRoot.Add(L.FillHorizontal("title", 128));

        var buttonSize = 120;
        var bufferSize = 40;

        _layoutRoot.Add(L.FixedElement(bufferSize, bufferSize));

        _layoutRoot.Add(L.FixedElement("play", buttonSize * 4, buttonSize));

        _layoutRoot.Add(L.FixedElement(bufferSize, bufferSize));

        var buttonGroup = _layoutRoot.AddGroup(new Style {Alignment = Alignment.Center, PaddingBetweenElements = 10},
            L.FillHorizontal(buttonSize));
        buttonGroup.Add(L.FixedElement("fullscreen", buttonSize * 2, buttonSize));
        //buttonGroup.Add(L.FixedElement("mute-music", buttonSize * 2, buttonSize));
        //buttonGroup.Add(L.FixedElement("mute-sfx", buttonSize * 2, buttonSize));
        
        _layoutRoot.Add(L.FixedElement(0, 200));


        _buttonStates.Add(new PressButtonState("play", "Play", () =>
        {
            onPlay();
            Close();
        }));
        _buttonStates.Add(new PressButtonState("fullscreen", "Toggle Fullscreen", () =>
        {
            window.SetFullscreen(!window.IsFullscreen);
            window.SetRenderResolution(new CartridgeConfig(window.Size, SamplerState.PointWrap));
        }));
        //_buttonStates.Add(new ToggleButtonState("mute-music", "Mute Music", muteMusic));
        //_buttonStates.Add(new ToggleButtonState("mute-sfx", "Mute SFX", muteSfx));

        _bakedLayout = CreateNewLayout();
    }

    private LayoutArrangement CreateNewLayout()
    {
        return _layoutRoot.Bake(_window.RenderResolution);
    }

    protected override void UpdateInputInternal(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_window.RenderResolution != _rememberedSize)
        {
            ResolutionChanged?.Invoke();
            _bakedLayout = CreateNewLayout();
            _rememberedSize = _window.RenderResolution;
        }

        foreach (var button in _buttonStates)
        {
            var rectangle = _bakedLayout.FindElement(button.LayoutId);

            hitTestStack.AddZone(rectangle, Depth.Middle - 10, button.HoverState);

            if (button.HoverState)
            {
                if (Client.Input.Mouse.GetButton(MouseButton.Left).WasPressed)
                {
                    _primedButton = button;
                }
            }
        }

        if (Client.Input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            if (_primedButton != null && _primedButton.HoverState)
            {
                ResourceAssets.Instance.PlaySound("sounds/ui_button", new SoundEffectSettings());
                _primedButton.OnClick();
            }

            _primedButton = null;
        }
    }

    protected override void OnTapAnywhere(SequenceTween tween)
    {
        // do nothing
    }

    protected override void OnSetupTween(SequenceTween tween)
    {
    }

    protected override void DrawContent(Painter painter, RectangleF screenRectangle)
    {
        painter.DrawRectangle(_bakedLayout.FindElement("title"),
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.5f * ContentOpacity), Depth = Depth.Middle});

        var font = Client.Assets.GetFont("gmtk/GameFont", 80);
        painter.DrawStringWithinRectangle(font, GameplayConstants.Title, _bakedLayout.FindElement("title"),
            Alignment.Center,
            new DrawSettings {Depth = Depth.Middle - 10, Color = Color.White.WithMultipliedOpacity(ContentOpacity)});

        foreach (var button in _buttonStates)
        {
            DrawButton(painter, button);
        }
    }

    public void DrawButton(Painter painter, GameButtonState button)
    {
        var name = button.Label;
        var rectangle = _bakedLayout.FindElement(button.LayoutId).Rectangle;
        var font = Client.Assets.GetFont("gmtk/GameFont", (int) (rectangle.Width / 7f));
        var backerTexture = "BUTTONS_Base2";

        if (button.HoverState)
        {
            backerTexture = "BUTTONS_Base1";
        }

        var offset = Vector2.Zero;
        if (_primedButton == button)
        {
            offset += new Vector2(0, 10);
        }

        offset += new Vector2(rectangle.X * (1f - ContinueOpacity),0);

        rectangle = rectangle.Moved(offset);

        painter.DrawAsRectangle(ResourceAssets.Instance.Textures[backerTexture], rectangle,
            new DrawSettings {Depth = Depth.Middle + 100, Color = Color.White.WithMultipliedOpacity(ContinueOpacity)});
        painter.DrawStringWithinRectangle(font, name, rectangle, Alignment.Center,
            new DrawSettings {Depth = Depth.Middle - 1, Color = Color.White.WithMultipliedOpacity(ContinueOpacity)});
    }
}
