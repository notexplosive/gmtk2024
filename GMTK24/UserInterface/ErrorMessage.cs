using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace GMTK24.UserInterface;

public class ErrorMessage
{
    private readonly TweenableFloat _opacity = new(0);

    private readonly TweenableRectangleF _ribbonRectangle = new(RectangleF.Empty);
    private readonly Vector2 _screenSize;
    private readonly SequenceTween _tween = new();
    private string _currentDisplayedMessage = string.Empty;
    private readonly Font _font;

    public ErrorMessage(Point screenSize)
    {
        _screenSize = screenSize.ToVector2();
        _font = Client.Assets.GetFont("gmtk/GameFont", 64);
    }

    public void Display(string message)
    {
        _currentDisplayedMessage = message;
        _tween.Clear();

        _opacity.Value = 0;
        var center = new Vector2(_screenSize.X / 2f,_screenSize.Y / 3f);
        var startingRectangle = RectangleF.FromCenterAndSize(center, new Vector2(_screenSize.X, 0));
        var finalRectangle =
            RectangleF.FromCenterAndSize(center, new Vector2(_screenSize.X, _font.Height * 1.5f));
        _ribbonRectangle.Value = startingRectangle;

        _tween
            .Add(new MultiplexTween()
                .Add(_ribbonRectangle.TweenTo(finalRectangle, 0.5f, Ease.CubicFastSlow))
                .Add(_opacity.TweenTo(1f, 0.5f, Ease.Linear))
            )
            
            .Add(new WaitSecondsTween(1f))
            
            .Add( new MultiplexTween()
                .Add(_ribbonRectangle.TweenTo(startingRectangle, 0.5f, Ease.CubicSlowFast))
                .Add(_opacity.TweenTo(0f, 0.25f, Ease.Linear))
            )
            ;
    }

    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void Draw(Painter painter)
    {
        painter.BeginSpriteBatch();

        painter.DrawRectangle(_ribbonRectangle,
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.5f * _opacity), Depth = Depth.Back});

        var messageSize = _font.MeasureString(_currentDisplayedMessage);
        painter.DrawStringAtPosition(_font, _currentDisplayedMessage,
            _ribbonRectangle.Value.Center - messageSize / 2f,
            new DrawSettings {Color = Color.Yellow.WithMultipliedOpacity(_opacity)});

        painter.EndSpriteBatch();
    }
}
