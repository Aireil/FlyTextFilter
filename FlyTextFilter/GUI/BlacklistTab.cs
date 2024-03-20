using ImGuiNET;

namespace FlyTextFilter.GUI;

public class BlacklistTab
{
    private string addToBlacklist = string.Empty;

    public void Draw()
    {
        ImGui.Text("Any entire match will filter the fly text (e.g. action name, status effect name, etc.).");

        ImGui.InputText("##addToBlacklist", ref this.addToBlacklist, 100);
        if (ImGui.Button("Add To Blacklist"))
        {
            Service.Configuration.Blacklist.Add(this.addToBlacklist);
            Service.Configuration.Save();
        }

        if (Service.Configuration.Blacklist.Count != 0)
        {
            ImGui.Separator();
        }

        foreach (var blItem in Service.Configuration.Blacklist)
        {
            if (ImGui.Button($"Remove##{blItem}"))
            {
                Service.Configuration.Blacklist.Remove(blItem);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            ImGui.Text(blItem);
        }
    }
}
