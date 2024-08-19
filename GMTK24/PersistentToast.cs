using System;
using ExTween;

namespace GMTK24;

public class PersistentToast
{
    public string Text { get; }
    private readonly Func<bool>? _vanishCriteria;
    private SequenceTween _tween = new();
    private bool _isFadingOut;

    public TweenableFloat VisiblePercent { get; } = new(0);

    private PersistentToast(string text)
    {
        Text = text;
        _tween
            .Add(VisiblePercent.TweenTo(1f, 0.75f, Ease.CubicFastSlow));
    }
    
    public PersistentToast(string text, float duration) : this(text)
    {
        _tween
            .Add(new WaitSecondsTween(duration))
            .Add(new CallbackTween(FadeOut));
    }
    
    public PersistentToast(string text, Func<bool> vanishCriteria) : this(text)
    {
        _vanishCriteria = vanishCriteria;
        
    }

    public void FadeOut()
    {
        if (_isFadingOut)
        {
            return;
        }

        _isFadingOut = true;
        _tween.Clear();
        
        _tween
            .Add(VisiblePercent.TweenTo(0f, 0.15f, Ease.CubicSlowFast));
    }

    public void Update(float dt)
    {
        _tween.Update(dt);

        if (_vanishCriteria?.Invoke() == true)
        {
            FadeOut();
        }
    }
}
