using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using Microsoft.Xna.Framework;

namespace GMTK24;

public abstract class Overlay
{
    private readonly HoverState _isHovered = new();
    protected readonly TweenableFloat ContinueOpacity = new(0);
    protected readonly TweenableFloat ScrimOpacity = new(0);
    protected readonly TweenableFloat ContentOpacity = new(0);
    private readonly SequenceTween _tween = new();

    public bool IsClosed { get; private set; }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack, Point screenSize)
    {
        hitTestStack.AddZone(screenSize.ToRectangleF(), Depth.Middle, _isHovered);

        if (_isHovered && input.Mouse.GetButton(MouseButton.Left, true).WasPressed)
        {
            OnContinue(_tween);
        }
    }

    protected abstract void OnContinue(SequenceTween tween);

    public void Reset()
    {
        IsClosed = false;

        ScrimOpacity.Value = 0;
        ContentOpacity.Value = 0;

        _tween.Clear();

        _tween
            .Add(new MultiplexTween()
                .Add(ScrimOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                .Add(ContentOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                
                .Add(new SequenceTween()
                    .Add(new WaitSecondsTween(0.5f))
                    .Add(ContinueOpacity.TweenTo(1f, 0.25f, Ease.Linear))
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
                .Add(ContinueOpacity.TweenTo(0, 0.25f, Ease.Linear))
                .Add(ContentOpacity.TweenTo(0, 0.25f, Ease.Linear))
                .Add(ScrimOpacity.TweenTo(0, 0.25f, Ease.Linear))
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
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.25f * ScrimOpacity)});

        var contentRectangle = screenRect.Inflated(-100, -100);
        DrawContent(painter, contentRectangle);

        var belowContentRectangle =
            new RectangleF(contentRectangle.Left, contentRectangle.Bottom, contentRectangle.Width, 80);

        var font = Client.Assets.GetFont("gmtk/GameFont", 45);
        painter.DrawStringWithinRectangle(font, "Click anywhere to continue", belowContentRectangle, Alignment.Center,
            new DrawSettings {Color = Color.White.WithMultipliedOpacity(ContinueOpacity)});

        painter.EndSpriteBatch();
    }

    protected abstract void DrawContent(Painter painter, RectangleF rectangle);
}
