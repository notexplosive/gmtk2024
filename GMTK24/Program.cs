using ExplogineDesktop;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using GMTK24;
using GMTK24.Model;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = GameplayConstants.Title
};
Bootstrap.Run(args, new WindowConfig(config), runtime => new HotReloadCartridge(runtime, new GMTKCartridge(runtime)));