using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;

namespace FlyTextFilter;

public class FlyTextKindData
{
    private static readonly Dictionary<FlyTextKind, FlyTextKindInfo> FlyTextKindDataDic = new()
    {
        {
            FlyTextKind.AutoAttack,
            new FlyTextKindInfo
            {
                Info = "Auto attacks, DoTs.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttack, FlyTextKind.CriticalHit, FlyTextKind.DirectHit, FlyTextKind.CriticalDirectHit },
            }
        },
        {
            FlyTextKind.CriticalHit,
            new FlyTextKindInfo
            {
                Info = "Auto attacks, DoTs.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttack, FlyTextKind.CriticalHit, FlyTextKind.DirectHit, FlyTextKind.CriticalDirectHit },
                InfoPrefix = "Crit",
            }
        },
        {
            FlyTextKind.DirectHit,
            new FlyTextKindInfo
            {
                Info = "Auto attacks, DoTs.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttack, FlyTextKind.CriticalHit, FlyTextKind.DirectHit, FlyTextKind.CriticalDirectHit },
                InfoPrefix = "DH",
            }
        },
        {
            FlyTextKind.CriticalDirectHit,
            new FlyTextKindInfo
            {
                Info = "Auto attacks, DoTs.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttack, FlyTextKind.CriticalHit, FlyTextKind.DirectHit, FlyTextKind.CriticalDirectHit },
                InfoPrefix = "Crit DH",
            }
        },
        {
            FlyTextKind.NamedIcon,
            new FlyTextKindInfo
            {
                Info = "Beneficial status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon, FlyTextKind.NamedIconFaded },
            }
        },
        {
            FlyTextKind.NamedIconFaded,
            new FlyTextKindInfo
            {
                Info = "Fading Beneficial status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon, FlyTextKind.NamedIconFaded },
            }
        },
        {
            FlyTextKind.NamedIcon2,
            new FlyTextKindInfo
            {
                Info = "Detrimental status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon2, FlyTextKind.NamedIconFaded2, FlyTextKind.NamedIconFullyResisted, FlyTextKind.NamedIconHasNoEffect, FlyTextKind.NamedIconInvulnerable },
            }
        },
        {
            FlyTextKind.NamedIconFaded2,
            new FlyTextKindInfo
            {
                Info = "Fading detrimental status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon2, FlyTextKind.NamedIconFaded2, FlyTextKind.NamedIconFullyResisted, FlyTextKind.NamedIconHasNoEffect, FlyTextKind.NamedIconInvulnerable },
            }
        },
        {
            FlyTextKind.NamedIconFullyResisted,
            new FlyTextKindInfo
            {
                Info = "Resisted detrimental status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon2, FlyTextKind.NamedIconFaded2, FlyTextKind.NamedIconFullyResisted, FlyTextKind.NamedIconHasNoEffect, FlyTextKind.NamedIconInvulnerable },
            }
        },
        {
            FlyTextKind.NamedIconHasNoEffect,
            new FlyTextKindInfo
            {
                Info = "No effect detrimental status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon2, FlyTextKind.NamedIconFaded2, FlyTextKind.NamedIconFullyResisted, FlyTextKind.NamedIconHasNoEffect, FlyTextKind.NamedIconInvulnerable },
            }
        },
        {
            FlyTextKind.NamedIconInvulnerable,
            new FlyTextKindInfo
            {
                Info = "Invulnerable detrimental status effects.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedIcon2, FlyTextKind.NamedIconFaded2, FlyTextKind.NamedIconFullyResisted, FlyTextKind.NamedIconHasNoEffect, FlyTextKind.NamedIconInvulnerable },
            }
        },
        {
            FlyTextKind.Miss,
            new FlyTextKindInfo
            {
                Info = "Text changes to DODGE if the fly text is on You.",
            }
        },
        {
            FlyTextKind.NamedIconWithItemOutline,
            new FlyTextKindInfo
            {
                Info = "Looted items.",
            }
        },
        {
            FlyTextKind.NamedAttack2,
            new FlyTextKindInfo
            {
                Info = "Healing.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack2, FlyTextKind.NamedCriticalHit2 },
            }
        },
        {
            FlyTextKind.NamedCriticalHit2,
            new FlyTextKindInfo
            {
                Info = "Healing.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack2, FlyTextKind.NamedCriticalHit2 },
                InfoPrefix = "Crit",
            }
        },
        {
            FlyTextKind.NamedMp2,
            new FlyTextKindInfo
            {
                Info = "MP regeneration.",
            }
        },
        {
            FlyTextKind.NamedAttack,
            new FlyTextKindInfo
            {
                Info = "Damage.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack, FlyTextKind.NamedCriticalHit, FlyTextKind.NamedDirectHit, FlyTextKind.NamedCriticalDirectHit },
            }
        },
        {
            FlyTextKind.NamedCriticalHit,
            new FlyTextKindInfo
            {
                Info = "Damage.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack, FlyTextKind.NamedCriticalHit, FlyTextKind.NamedDirectHit, FlyTextKind.NamedCriticalDirectHit },
                InfoPrefix = "Crit",
            }
        },
        {
            FlyTextKind.NamedDirectHit,
            new FlyTextKindInfo
            {
                Info = "Damage.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack, FlyTextKind.NamedCriticalHit, FlyTextKind.NamedDirectHit, FlyTextKind.NamedCriticalDirectHit },
                InfoPrefix = "DH",
            }
        },
        {
            FlyTextKind.NamedCriticalDirectHit,
            new FlyTextKindInfo
            {
                Info = "Damage.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.NamedAttack, FlyTextKind.NamedCriticalHit, FlyTextKind.NamedDirectHit, FlyTextKind.NamedCriticalDirectHit },
                InfoPrefix = "Crit DH",
            }
        },
        {
            FlyTextKind.NamedEp,
            new FlyTextKindInfo
            {
                Info = "Energy points regeneration (used in robots in Alexander/Rival Wings).",
            }
        },
        {
            FlyTextKind.AutoAttackNoText2,
            new FlyTextKindInfo
            {
                Info = "Crafting quality.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttackNoText2, FlyTextKind.CriticalHit2, FlyTextKind.DirectHit2, FlyTextKind.CriticalDirectHit2 },
            }
        },
        {
            FlyTextKind.CriticalHit2,
            new FlyTextKindInfo
            {
                Info = "Crafting quality.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttackNoText2, FlyTextKind.CriticalHit2, FlyTextKind.DirectHit2, FlyTextKind.CriticalDirectHit2 },
                InfoPrefix = "Crit",
            }
        },
        {
            FlyTextKind.DirectHit2,
            new FlyTextKindInfo
            {
                Info = "Crafting quality.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttackNoText2, FlyTextKind.CriticalHit2, FlyTextKind.DirectHit2, FlyTextKind.CriticalDirectHit2 },
                InfoPrefix = "DH",
            }
        },
        {
            FlyTextKind.CriticalDirectHit2,
            new FlyTextKindInfo
            {
                Info = "Crafting quality.",
                RelatedFlyTextKinds = new List<FlyTextKind> { FlyTextKind.AutoAttackNoText2, FlyTextKind.CriticalHit2, FlyTextKind.DirectHit2, FlyTextKind.CriticalDirectHit2 },
                InfoPrefix = "Crit DH",
            }
        },
        {
            FlyTextKind.AutoAttackNoText,
            new FlyTextKindInfo
            {
                Info = "Crafting progress.",
            }
        },
        {
            FlyTextKind.NamedCp,
            new FlyTextKindInfo
            {
                Info = "CP regeneration (Crafting Points).",
            }
        },
        {
            FlyTextKind.NamedGp,
            new FlyTextKindInfo
            {
                Info = "GP regeneration (Gathering Points).",
            }
        },
    };

    public static string GetInfoFormatted(FlyTextKind flyTextKind)
    {
        if (!FlyTextKindDataDic.TryGetValue(flyTextKind, out var flyTextKindInfo))
        {
            return string.Empty;
        }

        var formattedInfo = new StringBuilder();
        if (flyTextKindInfo.InfoPrefix != string.Empty)
        {
            formattedInfo.Append($"{flyTextKindInfo.InfoPrefix} - ");
        }

        formattedInfo.AppendLine(flyTextKindInfo.Info);
        if (flyTextKindInfo.RelatedFlyTextKinds.Any())
        {
            formattedInfo.AppendLine("Related type(s):");
            foreach (var relatedFlyTextKind in flyTextKindInfo.RelatedFlyTextKinds)
            {
                if (relatedFlyTextKind != flyTextKind)
                {
                    formattedInfo.AppendLine(relatedFlyTextKind.ToString());
                }
            }
        }

        return formattedInfo.ToString();
    }

    public static bool HasInfo(FlyTextKind flyTextKind)
    {
        return FlyTextKindDataDic.ContainsKey(flyTextKind);
    }
}
