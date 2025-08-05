using Dalamud.Bindings.ImGui;

namespace FlyTextFilter.GUI.TypesTab;

public class DisabledTab
{
    public static void Draw()
    {
        ImGui.Text("Tests failed, this tab has been disabled to protect your config." +
                   "\nSome of your settings could not behave as expected, but manually editing the config" +
                   "\nwill break it again once the plugin is updated or simply corrupt it.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Please report this issue on GitHub:");
        ImGui.SameLine();
        if (ImGui.Button("Open the GitHub repo"))
        {
            Util.OpenLink("https://github.com/Aireil/FlyTextFilter");
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text("If you nevertheless want to edit your config:");
        ImGui.SameLine();
        if (ImGui.Button("Overwrite"))
        {
            ImGui.OpenPopup("##OverwriteTestsResults");
        }

        if (ImGui.BeginPopup("##OverwriteTestsResults", ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("No support will be offered if your config gets corrupted.\nAre you sure you want to do this?");
            ImGui.Separator();
            if (ImGui.Button("Yes##OverwriteTestsResults"))
            {
                Service.ConfigWindow.HasPassedTests = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("No##OverwriteTestsResults"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}
