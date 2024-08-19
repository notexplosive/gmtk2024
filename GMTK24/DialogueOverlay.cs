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

    protected override void DrawContent(Painter painter, RectangleF screenRectangle)
    {
        var workingRectangle = RectangleF.FromCorners(screenRectangle.TopLeft + new Vector2(0,screenRectangle.Height * 2 /3f),screenRectangle.BottomRight);

        workingRectangle = workingRectangle.Moved(new Vector2(0, workingRectangle.Height - workingRectangle.Height * AppearPercent));
        
        painter.DrawRectangle(workingRectangle,
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.5f * ScrimOpacity)});
        
        var contentRectangle = workingRectangle.Inflated(-100, -100);
        contentRectangle = contentRectangle.Moved(new Vector2(0, -50));

        if (_currentPage != null)
        {
            var formattedText = FormattedText.FromFormatString(_currentPage.DialogueSpeaker.Font, Color.White,
                Ui.ApplyIcons(_inventory, _currentPage.Text, 2),
                GameplayConstants.FormattedTextParser);
            painter.DrawFormattedStringWithinRectangle(formattedText, contentRectangle, Alignment.Center, new DrawSettings
            {
                Color = _currentPage.DialogueSpeaker.Color.WithMultipliedOpacity(ContentOpacity)
            });
        }
        
        var belowContentRectangle =
            new RectangleF(contentRectangle.Left, contentRectangle.Bottom, contentRectangle.Width, 80);

        var font = Client.Assets.GetFont("gmtk/GameFont", 32);
        painter.DrawStringWithinRectangle(font, "Click to continue", belowContentRectangle, Alignment.Center,
            new DrawSettings {Color = Color.White.DimmedBy(0.25f).WithMultipliedOpacity(ContinueOpacity)});
    }
}
