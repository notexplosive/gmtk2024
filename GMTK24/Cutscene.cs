using System;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using GMTK24.UserInterface;

namespace GMTK24;

public class Cutscene
{
    private readonly OverlayScreen _overlayScreen;
    private readonly SequenceTween _tween = new();

    public Cutscene(OverlayScreen overlayScreen)
    {
        _overlayScreen = overlayScreen;
    }
    
    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void DisplayMessage(Ui? ui, params string[] message)
    {
        _tween.Add(new CallbackTween(() =>
        {
            _overlayScreen.OpenOverlay(ui, new DialogueOverlay(_overlayScreen.Inventory, message.Select(item=> new DialoguePage(){Text = item}).ToList()));
        }));

        _tween.Add(new WaitUntilTween(() => !_overlayScreen.HasOverlay()));
    }

    public void Callback(Action action)
    {
        _tween.Add(new CallbackTween(action));
    }

    public void Delay(float seconds)
    {
        _tween.Add(new WaitSecondsTween(seconds));
    }

    public void PanCamera(Camera camera, RectangleF viewBounds, float duration, Ease.Delegate ease)
    {
        _tween.Add(camera.TweenableViewBounds.TweenTo(viewBounds, duration, ease));
    }

    public bool IsDone()
    {
        return _tween.IsDone();
    }
}
