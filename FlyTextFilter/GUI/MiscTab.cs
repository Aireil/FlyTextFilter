using ImGuiNET;

namespace FlyTextFilter.GUI;

public class MiscTab
{
    public static void Draw()
    {
        var hasChanged = false;
        hasChanged |= ImGui.Checkbox("Force You as source for DoTs", ref Service.Configuration.ShouldAdjustDotSource);

        Util.DrawHelp("DoTs are stacked and their source is always the affected entity." +
                      "\nIt is therefore impossible to discriminate your DoTs from other players'." +
                      "\nThis setting forces all DoTs to have You as source, the goal is to" +
                      "\nbe able to filter Others, without removing your (potential) DoTs.");

        hasChanged |= ImGui.Checkbox("Force You as source for your pet's actions", ref Service.Configuration.ShouldAdjustPetSource);

        Util.DrawHelp("By default, pet actions have Others as source, this setting" +
                      "\nforces your pet's actions to have You instead.");

        hasChanged |= ImGui.Checkbox("Force You as source for your chocobo's actions", ref Service.Configuration.ShouldAdjustChocoboSource);

        Util.DrawHelp("By default, chocobo actions have Others as source, this setting" +
                      "\nforces your chocobo's actions to have You instead.");

        ImGui.Separator();

        hasChanged |= ImGui.Checkbox("Help to find unknown types", ref Service.Configuration.IsExplorer);

        Util.DrawHelp("Some type sources are still unknown, if this setting is enabled" +
                      "\nit will print in chat when you encounter one of those unknown type." +
                      "\nThis will only print it once per type per game session.");

        if (hasChanged)
        {
            Service.Configuration.Save();
        }
    }
}
