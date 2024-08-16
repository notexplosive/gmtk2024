using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24;

public class GMTKCartridge : BasicGameCartridge
{
    private ISession? _gameSession;

    public GMTKCartridge(IRuntime runtime) : base(runtime)
    {
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointWrap);

    public override void OnCartridgeStarted()
    {
        _gameSession = new GameSession();
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _gameSession?.UpdateInput(input, hitTestStack);
    }

    public override void Update(float dt)
    {
        _gameSession?.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _gameSession?.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded");
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        ResourceAssets.Reset();
        foreach (var item in ResourceAssets.Instance.LoadEvents(painter))
        {
            yield return item;
        }        
    }

    public override void Unload()
    {
        ResourceAssets.Reset();
    }
}