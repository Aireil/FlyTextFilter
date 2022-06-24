using System.Runtime.InteropServices;
using Dalamud.Game.Gui.FlyText;

namespace FlyTextFilter.Model;

/* Style:
 *   0 - is local player (or if source, is local player's pet)
 *   1 - is an alliance member
 *   2 - default if not the others
 *   3 - is a battle NPC, is in a duel with local player, or is a player character other than the local player in pvp instance or duel
 * If (source || target) == 0, then the style is the same, regardless of the other value.
 * If (source == 3 && target != 0), then the style is the same, regardless of the target value.
 * If ((source == 1 || source == 2) && target != 0), then the style is the same, regardless of the target value.
 * If (source || target) == 0 => Fly Text
 * else => Popup Text
 * */
[StructLayout(LayoutKind.Sequential)]
public struct FlyTextCreation
{
    public FlyTextKind FlyTextKind;
    public byte SourceStyle;
    public byte TargetStyle;
    public byte Option;
    public byte ActionKind;
    public int ActionId;
    public int Val1; // status id if icon
    public int Val2;
    public int Val3;
}
