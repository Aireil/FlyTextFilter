using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter.GUI;

public class EditPopup
{
    public static bool Draw(FlyTextKind flyTextKind, FlyTextSetting flyTextSetting)
    {
        var hasChanged = false;
        if (ImGui.BeginPopup("##PopupEditKind"))
        {
            ImGui.Text($"Editing {FlyTextKindData.GetAlias(flyTextKind)}");
            ImGui.Text("Checked = filtered.");
            Util.DrawHelp("For each source (You|Party|Others), 3 possible targets (Y|P|O)." +
                          "\nTo filter the fly text coming from source to target, check the corresponding checkbox." +
                          "\n Example: If from You to P is checked, it will filter all fly texts coming from you to" +
                          "\n your party members.");

            hasChanged |= Util.CheckboxFlagAll("Filter all##CheckboxEdit", ref flyTextSetting);

            if (ImGui.BeginTable("PopupEditTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableNextColumn();
                ImGui.Text("(From) Source");
                ImGui.TableNextColumn();
                ImGui.Text("(To) Target");

                hasChanged |= DrawPopupSetting("You", "##DrawPopupSettingSourceYou", ref flyTextSetting.SourceYou);
                hasChanged |= DrawPopupSetting("Party", "##DrawPopupSettingSourceParty", ref flyTextSetting.SourceParty);
                hasChanged |= DrawPopupSetting("Others", "##DrawPopupSettingSourceOthers", ref flyTextSetting.SourceOthers);

                ImGui.EndTable();
            }

            ImGui.EndPopup();
        }

        return hasChanged;
    }

    private static bool DrawPopupSetting(string source, string id, ref FlyTextTargets flyTextTargets)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(source);
        ImGui.TableNextColumn();

        var hasChanged = false;
        hasChanged |= Util.CheckboxFlag("You" + id, ref flyTextTargets, FlyTextTargets.You);
        hasChanged |= Util.CheckboxFlag("Party" + id, ref flyTextTargets, FlyTextTargets.Party);
        hasChanged |= Util.CheckboxFlag("Others" + id, ref flyTextTargets, FlyTextTargets.Others);

        return hasChanged;
    }
}
