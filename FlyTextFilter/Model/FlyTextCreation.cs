﻿using System.Runtime.InteropServices;
using Dalamud.Game.Gui.FlyText;

namespace FlyTextFilter.Model;

/* Style:
 *   0 - is local player (or if source, is local player's pet)
 *   1 - is an alliance member
 *   2 - default if not the others
 *   3 - is a battle NPC, is in a duel with local player, or is a player character other than the local player in pvp instance or duel
 * If (source == 0 || target == 0) { Fly Text } else { Pop-up Text }
 *
 * (target == 0) is style 1, red on attacks.
 * (source == 0 && target == 3) is style 2, yellow on attacks.
 * (source == 3 && target != 0) is style 3, red on attacks.
 * ((source == 1 || source == 2) && target != 0) is style 4, blue on attacks.
 *
 *
 * Val3:
 *   1-2-3: Damage type icon (Physical-Magic-Unique)
 *   if (kind == Dataset) gets the text value from WKSCosmoToolName sheet using that as rowId (4001->4044).
 *   if (kind == exp related (Exp/IslandExp/Unknown17/Unknown18) && option == 1) is the increase amount
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
