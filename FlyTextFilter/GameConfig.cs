using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Configuration;

namespace FlyTextFilter;

// Shamelessly stolen from XIVDeck which stole it from SimpleTweaks, call it teamwork!
// Temporary, to avoid waiting on Dalamud to update CS when config options break.
public static unsafe class GameConfig
{
    public class GameConfigSection
    {
        private readonly Dictionary<string, uint> indexCache = new();
        private readonly ConfigBase* configBase;

        public GameConfigSection(ConfigBase* configBase)
        {
            this.configBase = configBase;

            // Preload cache for performance
            var e = this.configBase->ConfigEntry;
            for (var i = 0U; i < this.configBase->ConfigCount; i++, e++)
            {
                if (e->Name == null) continue;
                this.indexCache[MemoryHelper.ReadStringNullTerminated((nint)e->Name)] = i;
            }
        }

        public bool TryGetBool(ConfigOption option, out bool value)
        {
            value = false;
            if (!this.TryGetEntry(option, out var entry)) return false;
            value = entry->Value.UInt != 0;
            return true;
        }

        public bool GetBool(ConfigOption option)
        {
            if (!this.TryGetBool(option, out var value))
                throw new ArgumentOutOfRangeException(nameof(option), @$"No option {option} was found.");

            return value;
        }

        public bool TryGetUInt(ConfigOption option, out uint value)
        {
            value = 0;
            if (!this.TryGetEntry(option, out var entry)) return false;
            value = entry->Value.UInt;
            return true;
        }

        public uint GetUInt(ConfigOption option)
        {
            if (!this.TryGetUInt(option, out var value))
                throw new ArgumentOutOfRangeException(nameof(option), @$"No option {option} was found.");

            return value;
        }

        private bool TryGetIndex(string name, out uint index)
        {
            if (this.indexCache.TryGetValue(name, out index)) return true;

            PluginLog.Verbose($"Cache miss on TryGetIndex for {name}!");
            var e = this.configBase->ConfigEntry;
            for (var i = 0U; i < this.configBase->ConfigCount; i++, e++)
            {
                if (e->Name == null) continue;
                if (e != null)
                {
                    var eName = MemoryHelper.ReadStringNullTerminated((nint)e->Name);

                    if (eName.Equals(name))
                    {
                        this.indexCache.Add(name, i);
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetEntry(uint index, out ConfigEntry* entry)
        {
            entry = null;
            if (this.configBase->ConfigEntry == null || index >= this.configBase->ConfigCount) return false;
            entry = this.configBase->ConfigEntry;
            entry += index;
            return true;
        }

        private bool TryGetEntry(ConfigOption option, out ConfigEntry* entry, bool searchByName = true)
        {
            entry = null;
            var index = (uint)option;
            if (searchByName && !this.TryGetIndex(option.ToString(), out index)) return false;

            return this.TryGetEntry(index, out entry);
        }
    }

    static GameConfig()
    {
        System = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.ConfigBase);
        UiConfig = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.UiConfig);
        UiControl = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.UiControlConfig);
    }

    public static readonly GameConfigSection System;
    public static readonly GameConfigSection UiConfig;
    public static readonly GameConfigSection UiControl;
}
