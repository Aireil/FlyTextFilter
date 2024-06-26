﻿using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FlyTextFilter.GUI;

// ReSharper disable once UnusedType.Global
namespace FlyTextFilter;

public sealed class FlyTextFilter : IDalamudPlugin
{
    private readonly WindowSystem windowSystem;

    public FlyTextFilter(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        Service.Configuration.Upgrade();
        Service.FlyTextHandler = new FlyTextHandler();

        Service.ConfigWindow = new ConfigWindow();
        this.windowSystem = new WindowSystem("FlyTextFilter");
        this.windowSystem.AddWindow(Service.ConfigWindow);

        Service.Commands = new Commands();

        Service.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
    }

    public void Dispose()
    {
        Service.FlyTextHandler.Dispose();
        Commands.Dispose();

        Service.Interface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
    }

    private static void OpenConfigUi()
        => Service.ConfigWindow.IsOpen = true;
}
