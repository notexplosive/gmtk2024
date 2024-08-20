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
    private ISession? _session;

    public GMTKCartridge(IRuntime runtime) : base(runtime)
    {
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1600, 900), SamplerState.PointWrap);

    public override void OnCartridgeStarted()
    {
        SwitchToPlayMode();

        if (Client.Args.GetValue<bool>("editor"))
        {
            SwitchToEditor();
        }

    }

    private void SwitchToEditor()
    {
        var planEditorSession = new PlanEditorSession(Runtime.Window.RenderResolution);
        planEditorSession.RequestPlayMode += SwitchToPlayMode;
        _session = planEditorSession;
    }

    private void SwitchToPlayMode()
    {
        var gameSession = new GameSession(Runtime.Window);
        gameSession.RequestEditorSession += SwitchToEditor;
        _session = gameSession;
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _session?.UpdateInput(input, hitTestStack);
    }

    public override void Update(float dt)
    {
        _session?.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _session?.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
        parameters.RegisterParameter<bool>("editor");
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