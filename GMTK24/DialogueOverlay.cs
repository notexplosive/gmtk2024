using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;

namespace GMTK24;

public class DialogueOverlay : Overlay
{
    private readonly Inventory _inventory;
    private readonly List<DialoguePage> _pages;
    private readonly Action? _onClose;
    private int _pageIndex;
    private DialoguePage? _currentPage;

    public DialogueOverlay(Inventory inventory,List<DialoguePage> pages, Action? onClose = null)
    {
        _inventory = inventory;
        _pages = pages;
        _onClose = onClose;
        if (_pages.Count == 0)
        {
            _pages.Add(new DialoguePage(){Text = "..."});
        }
        _currentPage = _pages.First();
    }
    
    protected override void OnContinue(SequenceTween tween)
    {
        tween.SkipToEnd();
        _pageIndex++;
        
        if (!_pages.IsValidIndex(_pageIndex))
        {
            _onClose?.Invoke();
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
            var formattedText = FormattedText.FromFormatString(_currentPage.DialogueSpeaker.Font, Color.White,
                Ui.ApplyIcons(_inventory, _currentPage.Text, 3),
                GameplayConstants.FormattedTextParser);
            painter.DrawFormattedStringWithinRectangle(formattedText, rectangle, Alignment.Center, new DrawSettings
            {
                Color = _currentPage.DialogueSpeaker.Color.WithMultipliedOpacity(ContentOpacity)
            });
        }
    }
}
