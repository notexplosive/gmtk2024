using ExplogineCore.Data;

namespace GMTK24;

public class ToggleButtonState : GameButtonState
{
    private readonly Wrapped<bool> _state;

    public ToggleButtonState(string layoutId, string label, Wrapped<bool> state) : base(layoutId, label)
    {
        _state = state;
    }

    public override void OnClick()
    {
        _state.Value = !_state.Value;
    }
}
