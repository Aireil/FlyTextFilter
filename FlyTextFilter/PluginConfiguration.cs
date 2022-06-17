using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using Newtonsoft.Json;

#pragma warning disable 618 // obsolete warning

namespace FlyTextFilter
{
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration
    {
        [JsonIgnore]
        public const int CurrentConfigVersion = 3;

        public int Version { get; set; } = CurrentConfigVersion;

        [Obsolete("Removed in v2")]
        public List<bool> KindToggleListPlayer { private get; set; } = new();

        [Obsolete("Removed in v2")]
        public List<bool> KindToggleListOther { private get; set; } = new();

        public HashSet<string> Blacklist = new();

        public HashSet<FlyTextKind> HideFlyTextKindPlayer = new();

        public HashSet<FlyTextKind> HideFlyTextKindOthers = new();

        public HashSet<FlyTextKind> HideFlyTextKindOnPlayer = new();

        public HashSet<FlyTextKind> HideFlyTextKindOnOthers = new();

        public FlyTextPositions FlyTextPositions = new();

        [JsonIgnore]
        public bool IsLoggingEnabled;

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
                    this.ShiftEnums(21, 2);

                    this.Version = 3;
                }

                this.Save();
            }
        }

        private static void ShiftEnum(ref HashSet<FlyTextKind> hashSet, int start, int shift)
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

        private void ShiftEnums(int start, int shift)
        {
            ShiftEnum(ref this.HideFlyTextKindPlayer, start, shift);
            ShiftEnum(ref this.HideFlyTextKindOthers, start, shift);
            ShiftEnum(ref this.HideFlyTextKindOnPlayer, start, shift);
            ShiftEnum(ref this.HideFlyTextKindOnOthers, start, shift);
        }
    }
}
