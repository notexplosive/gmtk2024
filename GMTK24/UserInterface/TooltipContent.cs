using System;
using Newtonsoft.Json;

namespace GMTK24.UserInterface;

public class TooltipContent
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Cost { get; set; } = "Free";
    public bool CanAfford { get; set; }
}
