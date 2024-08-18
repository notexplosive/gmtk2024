using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;

namespace GMTK24;

public class DialogueOverlay : Overlay
{
    private readonly List<DialoguePage> _pages;
    private int _pageIndex;
    private DialoguePage? _currentPage;

    public DialogueOverlay(List<DialoguePage> pages)
    {
        _pages = pages;
        _currentPage = _pages.First();
    }
    
    protected override void OnContinue(SequenceTween tween)
    {
        tween.SkipToEnd();
        _pageIndex++;
        
        if (!_pages.IsValidIndex(_pageIndex))
        {
            Close();
            return;
        }
        
        tween.Add(ContentOpacity.TweenTo(0, 0.25f, Ease.Linear));
        tween.Add(new CallbackTween(() => { _currentPage = _pages[_pageIndex]; }));
        tween.Add(ContentOpacity.TweenTo(1, 0.25f, Ease.Linear));
    }

    protected override void OnSetupTween(SequenceTween tween)
    {
        
    }

    protected override void DrawContent(Painter painter, RectangleF rectangle)
    {
        if (_currentPage != null)
        {
            painter.DrawStringWithinRectangle(_currentPage.DialogueSpeaker.Font, _currentPage.Text, rectangle, Alignment.Center, new DrawSettings
            {
                Color = _currentPage.DialogueSpeaker.Color.WithMultipliedOpacity(ContentOpacity)
            });
        }
    }
}
