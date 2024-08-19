using ExplogineMonoGame.TextFormatting;

namespace GMTK24.Model;

public static class GameplayConstants
{
    public static FormattedTextParser FormattedTextParser
    {
        get
        {
            var formattedTextParser = new FormattedTextParser();
            formattedTextParser.AddCommand("resourceTexture", new Command(args => new ResourceImageInstruction(args)));
            return formattedTextParser;
        }
    }
    
    public static string Title => "We are a City";
}
