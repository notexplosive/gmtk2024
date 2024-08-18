using ExplogineMonoGame.Data;
using ExplogineMonoGame.TextFormatting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24;

public class ResourceImageInstruction : Instruction
{
    private readonly Texture2D? _texture;

    public ResourceImageInstruction(string[] args)
    {
        if (args.IsValidIndex(0))
        {
            _texture = ResourceAssets.Instance.Textures[args[0]];
        }
    }

    private FormattedText.IFragment GetFragment(IFontGetter font, Color color)
    {
        Vector2 SizeCallback()
        {
            if (_texture == null)
            {
                return Vector2.Zero;
            }
            
            return _texture.Bounds.Size.ToVector2();
        }

        return new FormattedText.FragmentDrawable((painter, position, drawSettings) =>
            {
                var rectangle = new RectangleF(position, SizeCallback()).MovedByOrigin(-drawSettings.Origin);
                if (_texture != null)
                {
                    painter.DrawAsRectangle(_texture, rectangle, drawSettings with {Origin = DrawOrigin.Zero});
                }
            },
            SizeCallback);
    }

    public override void Do(TextRun textRun)
    {
        textRun.Fragments.Add(GetFragment(textRun.PeekFont(), textRun.PeekColor()));
    }
}
