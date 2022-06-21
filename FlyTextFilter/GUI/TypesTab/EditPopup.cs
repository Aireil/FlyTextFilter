using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter.GUI.TypesTab;

public class EditPopup
{
    public static bool Draw(FlyTextKind flyTextKind, FlyTextSetting flyTextSetting)
    {
        var hasChanged = false;
        if (ImGui.BeginPopup("##PopupEditKind"))
        {
            ImGui.Text($"Editing {flyTextKind}");
            ImGui.Text("Checked = filtered.");
            Util.DrawHelp("For each source (You|Party|Others), 3 possible targets (Y|P|O)." +
                          "\nTo filter the fly text coming from source to target, check the corresponding checkbox." +
                          "\n Example: If from You to P is checked, it will filter all fly texts coming from you to" +
                          "\n your party members.");

            hasChanged |= Util.CheckboxFlagAll("Filter all##CheckboxEdit", ref flyTextSetting);

            if (ImGui.BeginTable("PopupEditTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableNextColumn();
                ImGui.Text("(From) Source");
                ImGui.TableNextColumn();
                ImGui.Text("(To) Target");

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("You");
                ImGui.TableNextColumn();
                hasChanged |= DrawPopupTargetColumn("##DrawPopupTargetColumnSourceYou", ref flyTextSetting.SourceYou);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Party");
                ImGui.TableNextColumn();
                hasChanged |= DrawPopupTargetColumn("##DrawPopupTargetColumnSourceParty", ref flyTextSetting.SourceParty);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Others");
                ImGui.TableNextColumn();
                hasChanged |= DrawPopupTargetColumn("##DrawPopupTargetColumnSourceOthers", ref flyTextSetting.SourceOthers);

                ImGui.EndTable();
            }

            ImGui.EndPopup();
        }

        return hasChanged;
    }

    private static bool DrawPopupTargetColumn(string id, ref FlyTextTargets flyTextTargets)
    {
        ImGui.Separator();

        var hasChanged = false;
        hasChanged |= Util.CheckboxFlag("You" + id, ref flyTextTargets, FlyTextTargets.You);
        hasChanged |= Util.CheckboxFlag("Party" + id, ref flyTextTargets, FlyTextTargets.Party);
        hasChanged |= Util.CheckboxFlag("Others" + id, ref flyTextTargets, FlyTextTargets.Others);

        return hasChanged;
    }
}
