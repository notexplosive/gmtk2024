using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using Microsoft.Xna.Framework;

namespace GMTK24;

public abstract class Overlay
{
    private readonly TweenableFloat _continueOpacity = new(0);
    private readonly HoverState _isHovered = new();
    private readonly TweenableFloat _scrimOpacity = new(0);
    private readonly TweenableFloat _textOpacity = new(0);
    private readonly SequenceTween _tween = new();

    public bool IsClosed { get; private set; }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack, Point screenSize)
    {
        hitTestStack.AddZone(screenSize.ToRectangleF(), Depth.Middle, _isHovered);

        if (_isHovered && input.Mouse.GetButton(MouseButton.Left, true).WasPressed)
        {
            OnContinue();
        }
    }

    protected abstract void OnContinue();

    public void Reset()
    {
        IsClosed = false;

        _scrimOpacity.Value = 0;
        _textOpacity.Value = 0;

        _tween.Clear();

        _tween
            .Add(new MultiplexTween()
                .Add(_scrimOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                .Add(_textOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                
                .Add(new SequenceTween()
                    .Add(new WaitSecondsTween(0.5f))
                    .Add(_continueOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                )
            )
            ;

        OnSetupTween(_tween);
    }

    public void Close()
    {
        _tween.SkipToEnd();

        _tween
            .Add(new MultiplexTween()
                .Add(_continueOpacity.TweenTo(0, 0.25f, Ease.Linear))
                .Add(_textOpacity.TweenTo(0, 0.25f, Ease.Linear))
                .Add(_scrimOpacity.TweenTo(0, 0.25f, Ease.Linear))
            )
            
            .Add(new CallbackTween(() => IsClosed = true));
        ;
    }

    protected abstract void OnSetupTween(SequenceTween tween);

    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void Draw(Painter painter, Point screenSize)
    {
        painter.BeginSpriteBatch();

        var screenRect = screenSize.ToRectangleF();
        painter.DrawRectangle(screenRect,
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.5f * _scrimOpacity)});

        var contentRectangle = screenRect.Inflated(-100, -100);
        DrawContent(painter, contentRectangle, _textOpacity);

        var belowContentRectangle =
            new RectangleF(contentRectangle.Left, contentRectangle.Bottom, contentRectangle.Width, 80);

        var font = Client.Assets.GetFont("gmtk/GameFont", 45);
        painter.DrawStringWithinRectangle(font, "Click anywhere to continue", belowContentRectangle, Alignment.Center,
            new DrawSettings {Color = Color.White.WithMultipliedOpacity(_continueOpacity)});

        painter.EndSpriteBatch();
    }

    protected abstract void DrawContent(Painter painter, RectangleF rectangle, TweenableFloat textOpacity);
}
