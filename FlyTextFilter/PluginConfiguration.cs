using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Gui.FlyText;
using Newtonsoft.Json;

#pragma warning disable 618 // obsolete warning

namespace FlyTextFilter
{
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration
    {
        [JsonIgnore]
        public const int CurrentConfigVersion = 2;

        public int Version { get; set; } = CurrentConfigVersion;

        [Obsolete("Removed in v2")]
        public List<bool> KindToggleListPlayer { private get; set; } = new ();

        [Obsolete("Removed in v2")]
        public List<bool> KindToggleListOther { private get; set; } = new ();

        public HashSet<string> Blacklist = new ();

        public HashSet<FlyTextKind> HideFlyTextKindPlayer = new ();

        public HashSet<FlyTextKind> HideFlyTextKindOthers = new ();

        public HashSet<FlyTextKind> HideFlyTextKindOnPlayer = new ();

        public HashSet<FlyTextKind> HideFlyTextKindOnOthers = new ();

        [JsonIgnore]
        public bool IsLoggingEnabled;

        public void Save()
            => Service.Interface.SavePluginConfig(this);

        public void Upgrade()
        {
            if (this.Version < CurrentConfigVersion)
            {
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

                this.Save();
            }
        }
    }
}
