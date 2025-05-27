using Dalamud.Game.Gui.FlyText;

namespace FlyTextFilter;

public class FlyTextKindData
{
    public static string GetAlias(FlyTextKind flyTextKind)
    {
        return flyTextKind switch
        {
            FlyTextKind.AutoAttackOrDot => "Auto Attacks/DoTs",
            FlyTextKind.AutoAttackOrDotDh => "Auto Attacks/DoTs - DH",
            FlyTextKind.AutoAttackOrDotCrit => "Auto Attacks/DoTs - Crit",
            FlyTextKind.AutoAttackOrDotCritDh => "Auto Attacks/DoTs - Crit DH",
            FlyTextKind.BuffFading => "Buff - Fading",
            FlyTextKind.DebuffFading => "Debuff - Fading",
            FlyTextKind.DebuffResisted => "Debuff - Resisted",
            FlyTextKind.DebuffNoEffect => "Debuff - No Effect",
            FlyTextKind.DebuffInvulnerable => "Debuff - Invulnerable",
            FlyTextKind.LootedItem => "Looted Items",
            FlyTextKind.HealingCrit => "Healing - Crit",
            FlyTextKind.DamageCrit => "Damage - Crit",
            FlyTextKind.DamageDh => "Damage - DH",
            FlyTextKind.DamageCritDh => "Damage - Crit DH",
            FlyTextKind.CraftingQuality => "Crafting Quality",
            FlyTextKind.CraftingQualityCrit => "Crafting Quality - Crit",
            FlyTextKind.CraftingQualityDh => "Crafting Quality - DH",
            FlyTextKind.CraftingQualityCritDh => "Crafting Quality - Crit DH",
            FlyTextKind.CraftingProgress => "Crafting Progress",
            FlyTextKind.CpRegen => "Regeneration - CP",
            FlyTextKind.EpRegen => "Regeneration - EP",
            FlyTextKind.GpRegen => "Regeneration - GP",
            FlyTextKind.MpRegen => "Regeneration - MP",
            FlyTextKind.AutoAttackNoText3 => "Unused - Auto3",
            FlyTextKind.Collectability => "Collectability",
            FlyTextKind.CollectabilityCrit => "Collectability - Crit",
            FlyTextKind.CriticalHit4 => "Unused - Crit4",
            FlyTextKind.HpDrain => "Drain - HP",
            FlyTextKind.MpDrain => "Drain - MP",
            FlyTextKind.NamedCriticalHitWithMp => "Unused - Named Crit MP",
            FlyTextKind.NamedCriticalHitWithTp => "Unused - Named Crit TP",
            FlyTextKind.NamedDodge => "Dodge - Named",
            FlyTextKind.FullyResisted => "Fully Resisted",
            FlyTextKind.HasNoEffect => "Has No Effect",
            FlyTextKind.NamedMiss => "Miss - Named",
            FlyTextKind.NamedMp3 => "Unused - Named MP3",
            FlyTextKind.NamedTp => "Unused - Named TP",
            FlyTextKind.NamedTp2 => "Unused - Named TP2",
            FlyTextKind.NamedTp3 => "Unused - Named TP3",
            FlyTextKind.IslandExp => "Exp - Island",
            FlyTextKind.None => "Unused - None",
            FlyTextKind.Unknown17 => "Knowledge",
            FlyTextKind.Unknown18 => "Exp - Phantom",
            _ => flyTextKind.ToString(),
        };
    }
}
