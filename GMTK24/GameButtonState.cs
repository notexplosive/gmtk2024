using ExplogineMonoGame.Data;

namespace GMTK24;

public abstract class GameButtonState
{
    public GameButtonState(string layoutId, string label)
    {
        LayoutId = layoutId;
        Label = label;
    }

    public string LayoutId { get; }
    public string Label { get; }
    public HoverState HoverState { get; } = new();
    public abstract void OnClick();
}
