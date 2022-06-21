using System.Collections.Generic;
using Dalamud.Game.Gui.FlyText;

namespace FlyTextFilter.Model;

public class FlyTextKindInfo
{
    public string Info = string.Empty;
    public string InfoPrefix = string.Empty;
    public List<FlyTextKind> RelatedFlyTextKinds = new();
}
