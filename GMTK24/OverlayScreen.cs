using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using GMTK24.Model;
using GMTK24.UserInterface;
using Microsoft.Xna.Framework;

namespace GMTK24;

public class OverlayScreen
{
    public Inventory Inventory { get; }
    private Overlay? _currentOverlay;

    public OverlayScreen(Inventory inventory)
    {
        Inventory = inventory;
    }

    public void OpenOverlay(Ui? ui, Overlay newOverlay)
    {
        newOverlay.Reset();
        _currentOverlay = newOverlay;
        ui?.FadeOut();
    }

    public bool HasOverlay()
    {
        return _currentOverlay != null;
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack, Point screenSize)
    {
        _currentOverlay?.UpdateInput(input, hitTestStack, screenSize);
    }

    public bool IsCurrentOverlayClosed()
    {
        return _currentOverlay?.IsClosed == true;
    }

    public void ClearCurrentOverlay()
    {
        _currentOverlay = null;
    }

    public void Draw(Painter painter, Point screenSize)
    {
        _currentOverlay?.Draw(painter, screenSize);
    }

    public void Update(float dt)
    {
        _currentOverlay?.Update(dt);
    }
}
