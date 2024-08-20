using System;

namespace GMTK24;

public class PressButtonState : GameButtonState
{
    private readonly Action _onClick;

    public PressButtonState(string layoutId, string label, Action onClick) : base(layoutId, label)
    {
        _onClick = onClick;
    }

    public override void OnClick()
    {
        _onClick();
    }
}
