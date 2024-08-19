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
    private readonly SequenceTween _tween = new();
    protected readonly TweenableFloat AppearPercent = new(0);
    protected readonly TweenableFloat ContentOpacity = new(0);
    protected readonly TweenableFloat ContinueOpacity = new(0);
    protected readonly TweenableFloat ScrimOpacity = new(0);

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
            .Add(
                new MultiplexTween()
                    .Add(AppearPercent.TweenTo(1f, 0.5f, Ease.QuadFastSlow))
                    .Add(ScrimOpacity.TweenTo(1f, 0.25f, Ease.QuadFastSlow)))
            .Add(
                new MultiplexTween()
                    .Add(ContentOpacity.TweenTo(1f, 0.25f, Ease.Linear))
                    .Add(new SequenceTween()
                        .Add(new WaitSecondsTween(1f))
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
                .Add(AppearPercent.TweenTo(0f, 1f, Ease.QuadFastSlow))
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

        DrawContent(painter, screenRect);

        painter.EndSpriteBatch();
    }

    protected abstract void DrawContent(Painter painter, RectangleF rectangle);
}
