using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using FlyTextFilter.Model.FlyTextAdjustments;
using Newtonsoft.Json;

#pragma warning disable 618 // obsolete warning

namespace FlyTextFilter;

[Serializable]
public class PluginConfiguration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 5;

    public int Version { get; set; } = CurrentConfigVersion;

    public HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase);

    public FlyTextAdjustments FlyTextAdjustments = new();

    [JsonConverter(typeof(ConcurrentDictionaryConverter<FlyTextKind, FlyTextSetting>))]
    public ConcurrentDictionary<FlyTextKind, FlyTextSetting> FlyTextSettings = new();

    public bool ShouldAdjustDotSource = true;

    public bool ShouldAdjustPetSource = true;

    public int NbOfLogs = 50;

    public void Save()
        => Service.Interface.SavePluginConfig(this);

    public void Upgrade()
    {
        if (this.Version < CurrentConfigVersion)
        {
            // import old plugin config
            if (this.Version == 1)
            {
                for (var i = 0; i < this.KindToggleListPlayer.Count; i++)
                {
                    if (!this.KindToggleListPlayer[i])
                    {
                        this.HideFlyTextKindPlayer.Add((FlyTextKind)i);
                    }
                }

                for (var i = 0; i < this.KindToggleListOther.Count; i++)
                {
                    if (!this.KindToggleListOther[i])
                    {
                        this.HideFlyTextKindOthers.Add((FlyTextKind)i);
                    }
                }

                this.Version = 2;
            }

            // Endwalker enum changes
            if (this.Version == 2)
            {
                this.ShiftOldEnums(21, 2);

                this.Version = 3;
            }

            if (this.Version == 3)
            {
                FlyTextSetting? flyTextSetting;
                foreach (var flyTextKind in this.HideFlyTextKindPlayer)
                {
                    if (this.FlyTextSettings.TryGetValue(flyTextKind, out flyTextSetting))
                    {
                        flyTextSetting.SourceYou |= FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others;
                    }
                    else
                    {
                        flyTextSetting = new FlyTextSetting
                        {
                            SourceYou = FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others,
                        };
                    }

                    this.FlyTextSettings[flyTextKind] = flyTextSetting;
                }

                foreach (var flyTextKind in this.HideFlyTextKindOthers)
                {
                    if (this.FlyTextSettings.TryGetValue(flyTextKind, out flyTextSetting))
                    {
                        flyTextSetting.SourceParty |= FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others;
                        flyTextSetting.SourceOthers |= FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others;
                    }
                    else
                    {
                        flyTextSetting = new FlyTextSetting
                        {
                            SourceParty = FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others,
                            SourceOthers = FlyTextTargets.You | FlyTextTargets.Party | FlyTextTargets.Others,
                        };
                    }

                    this.FlyTextSettings[flyTextKind] = flyTextSetting;
                }

                foreach (var flyTextKind in this.HideFlyTextKindOnPlayer)
                {
                    if (this.FlyTextSettings.TryGetValue(flyTextKind, out flyTextSetting))
                    {
                        flyTextSetting.SourceYou |= FlyTextTargets.You;
                        flyTextSetting.SourceParty |= FlyTextTargets.You;
                        flyTextSetting.SourceOthers |= FlyTextTargets.You;
                    }
                    else
                    {
                        flyTextSetting = new FlyTextSetting
                        {
                            SourceYou = FlyTextTargets.You,
                            SourceParty = FlyTextTargets.You,
                            SourceOthers = FlyTextTargets.You,
                        };
                    }

                    this.FlyTextSettings[flyTextKind] = flyTextSetting;
                }

                foreach (var flyTextKind in this.HideFlyTextKindOnOthers)
                {
                    if (this.FlyTextSettings.TryGetValue(flyTextKind, out flyTextSetting))
                    {
                        flyTextSetting.SourceYou |= FlyTextTargets.Party | FlyTextTargets.Others;
                        flyTextSetting.SourceParty |= FlyTextTargets.Party | FlyTextTargets.Others;
                        flyTextSetting.SourceOthers |= FlyTextTargets.Party | FlyTextTargets.Others;
                    }
                    else
                    {
                        flyTextSetting = new FlyTextSetting
                        {
                            SourceYou = FlyTextTargets.Party | FlyTextTargets.Others,
                            SourceParty = FlyTextTargets.Party | FlyTextTargets.Others,
                            SourceOthers = FlyTextTargets.Party | FlyTextTargets.Others,
                        };
                    }

                    this.FlyTextSettings[flyTextKind] = flyTextSetting;
                }

                this.Version = 4;
            }

            if (this.Version == 4)
            {
                this.FlyTextAdjustments.FlyTextPositions = this.FlyTextPositions;

                this.Version = 5;
            }

            this.Save();
        }
    }

    public void UpdateFlyTextSettings(FlyTextKind flyTextKind, FlyTextSetting flyTextSetting)
    {
        if (flyTextSetting.IsSettingEmpty())
        {
            this.FlyTextSettings.TryRemove(flyTextKind, out _);
        }
        else
        {
            this.FlyTextSettings[flyTextKind] = flyTextSetting;
        }
    }

    [Obsolete("Removed in v2")]
    public List<bool> KindToggleListPlayer { private get; set; } = new();

    [Obsolete("Removed in v2")]
    public List<bool> KindToggleListOther { private get; set; } = new();

    [Obsolete("Removed in v4")]
    public HashSet<FlyTextKind> HideFlyTextKindPlayer { private get; set; } = new();

    [Obsolete("Removed in v4")]
    public HashSet<FlyTextKind> HideFlyTextKindOthers { private get; set; } = new();

    [Obsolete("Removed in v4")]
    public HashSet<FlyTextKind> HideFlyTextKindOnPlayer { private get; set; } = new();

    [Obsolete("Removed in v4")]
    public HashSet<FlyTextKind> HideFlyTextKindOnOthers { private get; set; } = new();

    [Obsolete("Removed in v4")]
    private static void ShiftOldEnum(ref HashSet<FlyTextKind> hashSet, int start, int shift)
    {
        var tmpList = hashSet.ToList();
        for (var i = 0; i < tmpList.Count; i++)
        {
            if ((int)tmpList[i] >= start)
            {
                tmpList[i] += shift;
            }
        }

        hashSet = tmpList.ToHashSet();
    }

    [Obsolete("Removed in v4")]
    private void ShiftOldEnums(int start, int shift)
    {
        var hideFlyTextKindPlayer = this.HideFlyTextKindPlayer;
        ShiftOldEnum(ref hideFlyTextKindPlayer, start, shift);
        var hideFlyTextKindOthers = this.HideFlyTextKindOthers;
        ShiftOldEnum(ref hideFlyTextKindOthers, start, shift);
        var hideFlyTextKindOnPlayer = this.HideFlyTextKindOnPlayer;
        ShiftOldEnum(ref hideFlyTextKindOnPlayer, start, shift);
        var hideFlyTextKindOnOthers = this.HideFlyTextKindOnOthers;
        ShiftOldEnum(ref hideFlyTextKindOnOthers, start, shift);
    }

    [Obsolete("Removed in v5")]
    public FlyTextPositions FlyTextPositions { private get; set; } = new();
}
