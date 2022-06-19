using System;
using System.Diagnostics;
using ImGuiNET;

namespace FlyTextFilter;

public class Util
{
    public static (float width, float height) GetScreenSize()
    {
        float width;
        float height;
        try
        {
            width = ImGui.GetIO().DisplaySize.X;
            height = ImGui.GetIO().DisplaySize.Y;
        }
        catch (NullReferenceException)
        {
            width = 1920.0f;
            height = 1080.0f;
        }

        return (width, height);
    }

    public static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = link,
            UseShellExecute = true,
        });
    }
}
