using Dalamud.Game.Gui.FlyText;

namespace FlyTextFilter;

public class FlyTextKindData
{
    public static string GetAlias(FlyTextKind flyTextKind)
    {
        return flyTextKind switch
        {
            FlyTextKind.AutoAttack => "Auto Attacks/DoTs",
            FlyTextKind.CriticalHit => "Auto Attacks/DoTs - Crit",
            FlyTextKind.DirectHit => "Auto Attacks/DoTs - DH",
            FlyTextKind.CriticalDirectHit => "Auto Attacks/DoTs - Crit DH",
            FlyTextKind.NamedIcon => "Beneficial Status",
            FlyTextKind.NamedIconFaded => "Beneficial Status - Fading",
            FlyTextKind.NamedIcon2 => "Detrimental Status",
            FlyTextKind.NamedIconFaded2 => "Detrimental Status - Fading",
            FlyTextKind.NamedIconFullyResisted => "Detrimental Status - Resisted",
            FlyTextKind.NamedIconHasNoEffect => "Detrimental Status - No Effect",
            FlyTextKind.NamedIconInvulnerable => "Detrimental Status - Invulnerable",
            FlyTextKind.NamedIconWithItemOutline => "Looted Items",
            FlyTextKind.NamedAttack2 => "Healing",
            FlyTextKind.NamedCriticalHit2 => "Healing - Crit",
            FlyTextKind.NamedMp2 => "Regeneration MP",
            FlyTextKind.NamedAttack => "Damage",
            FlyTextKind.NamedCriticalHit => "Damage - Crit",
            FlyTextKind.NamedDirectHit => "Damage - DH",
            FlyTextKind.NamedCriticalDirectHit => "Damage - Crit DH",
            FlyTextKind.NamedEp => "Regeneration EP",
            FlyTextKind.AutoAttackNoText2 => "Crafting Quality",
            FlyTextKind.CriticalHit2 => "Crafting Quality - Crit",
            FlyTextKind.DirectHit2 => "Crafting Quality - DH",
            FlyTextKind.CriticalDirectHit2 => "Crafting Quality - Crit DH",
            FlyTextKind.AutoAttackNoText => "Crafting Progress",
            FlyTextKind.NamedCp => "Regeneration CP",
            FlyTextKind.NamedGp => "Regeneration GP",
            FlyTextKind.AutoAttackNoText3 => "Unused - Auto3",
            FlyTextKind.AutoAttackNoText4 => "Collectability",
            FlyTextKind.CriticalHit3 => "Collectability - Crit",
            FlyTextKind.CriticalHit4 => "Unused - Crit4",
            FlyTextKind.NamedAttack3 => "Unused - Named Attack3",
            FlyTextKind.NamedCriticalHitWithMp => "Unused - Named Crit MP",
            FlyTextKind.NamedCriticalHitWithTp => "Unused - Named Crit TP",
            FlyTextKind.NamedDodge => "Dodge - Named",
            FlyTextKind.NamedFullyResisted => "Resist - Named",
            FlyTextKind.NamedHasNoEffect => "No Effect",
            FlyTextKind.NamedMiss => "Miss - Named",
            FlyTextKind.NamedMp => "MP Drain",
            FlyTextKind.NamedMp3 => "Unused - Named MP3",
            FlyTextKind.NamedTp => "Unused - Named TP",
            FlyTextKind.NamedTp2 => "Unused - Named TP2",
            FlyTextKind.NamedTp3 => "Unused - Named TP3",
            FlyTextKind.IslandExp => "Island Exp",
            FlyTextKind.None => "Unused - None",
            _ => flyTextKind.ToString(),
        };
    }
}
