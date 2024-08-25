using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using GMTK24.Model;
using Microsoft.Xna.Framework;

namespace GMTK24.UserInterface;

public class ErrorMessage
{
    private readonly TweenableFloat _opacity = new(0);

    private readonly TweenableRectangleF _ribbonRectangle = new(RectangleF.Empty);
    private readonly Vector2 _screenSize;
    private readonly SequenceTween _tween = new();
    private FormattedText? _currentDisplayedMessage;
    private readonly Font _font;

    public ErrorMessage(Point screenSize)
    {
        _screenSize = screenSize.ToVector2();
        _font = Client.Assets.GetFont("gmtk/GameFont", 64);
    }

    public void Display(string message)
    {
        _currentDisplayedMessage = FormattedText.FromFormatString(_font, Color.White, message, GameplayConstants.FormattedTextParser);
        _tween.Clear();

        _opacity.Value = 0;
        var center = new Vector2(_screenSize.X / 2f,_screenSize.Y / 3f);
        var startingRectangle = RectangleF.FromCenterAndSize(center, new Vector2(_screenSize.X, 0));
        var finalRectangle =
            RectangleF.FromCenterAndSize(center, new Vector2(_screenSize.X, _font.Height * 1.5f));
        _ribbonRectangle.Value = startingRectangle;

        _tween
            .Add(new MultiplexTween()
                .Add(_ribbonRectangle.TweenTo(finalRectangle, 0.25f, Ease.CubicFastSlow))
                .Add(_opacity.TweenTo(1f, 0.25f, Ease.Linear))
            )
            
            .Add(new WaitSecondsTween(0.75f))
            
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
            new DrawSettings {Color = Color.DarkBlue.WithMultipliedOpacity(0.35f * _opacity), Depth = Depth.Back});

        if (_currentDisplayedMessage != null)
        {
            var messageSize = _currentDisplayedMessage.MaxNeededSize();
            painter.DrawFormattedStringAtPosition(_currentDisplayedMessage,
                _ribbonRectangle.Value.Center - messageSize / 2f, Alignment.Center, 
                new DrawSettings {Color = Color.White.WithMultipliedOpacity(_opacity)});
        }

        painter.EndSpriteBatch();
    }
}
