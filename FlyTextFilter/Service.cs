using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.IoC;
using Dalamud.Plugin;
using FlyTextFilter.GUI;

namespace FlyTextFilter;

internal class Service
{
    internal static PluginConfiguration Configuration { get; set; } = null!;
    internal static FlyTextHandler FlyTextHandler { get; set; } = null!;
    internal static Commands Commands { get; set; } = null!;
    internal static ConfigWindow ConfigWindow { get; set; } = null!;

    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService]
    internal static ChatGui ChatGui { get; private set; } = null!;
    [PluginService]
    internal static ClientState ClientState { get; private set; } = null!;
    [PluginService]
    internal static CommandManager CommandManager { get; private set; } = null!;
    [PluginService]
    internal static Condition Condition { get; private set; } = null!;
    [PluginService]
    internal static DataManager DataManager { get; private set; } = null!;
    [PluginService]
    internal static FlyTextGui FlyTextGui { get; private set; } = null!;
    [PluginService]
    internal static SigScanner SigScanner { get; private set; } = null!;
    [PluginService]
    internal static GameGui GameGui { get; private set; } = null!;
    [PluginService]
    internal static ObjectTable ObjectTable { get; private set; } = null!;
    [PluginService]
    internal static Framework Framework { get; private set; } = null!;
}
