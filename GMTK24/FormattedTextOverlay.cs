using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace GMTK24;

public class FormattedTextOverlay : Overlay
{
    private readonly FormattedText _formattedText;

    public FormattedTextOverlay(FormattedText formattedText)
    {
        _formattedText = formattedText;
    }

    protected override void OnContinue(SequenceTween tween)
    {
        Close();
    }

    protected override void OnSetupTween(SequenceTween tween)
    {
    }

    protected override void DrawContent(Painter painter, RectangleF rectangle)
    {
        painter.DrawFormattedStringWithinRectangle(_formattedText,
            rectangle.Moved(new Vector2(0, 50 * 1f / Ease.QuadFastSlow(ContentOpacity))), Alignment.TopLeft,
            new DrawSettings {Color = Color.White.WithMultipliedOpacity(ContentOpacity)});
    }
}
