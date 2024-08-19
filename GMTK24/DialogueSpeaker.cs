using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace GMTK24;

public class DialogueSpeaker
{
    public Color Color { get; init; } = Color.White;
    public IFontGetter Font { get; init; } = Client.Assets.GetFont("gmtk/GameFont", 70);
}
