using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.FlyText;
using ImGuiNET;

namespace FlyTextFilter.GUI;

public class TypesTab
{
    public static void Draw()
    {
        var hasChanged = false;

        ImGui.Checkbox($"Enable logging##EnableLoggingButton", ref Service.Configuration.IsLoggingEnabled);

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Reset every restart to reduce spam");

        ImGui.Text("Enable logging and use /xllog to see logs of fly text types.");

        if (ImGui.BeginTable("###TableKinds", 5, ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Hide, from You", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Hide, from Others", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Hide, on You", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Hide, on Others", ImGuiTableColumnFlags.None);
            ImGui.TableHeadersRow();

            foreach (var kind in ((FlyTextKind[])Enum.GetValues(typeof(FlyTextKind))).OrderBy(x => x.ToString()))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{kind.ToString()}");

                ImGui.TableSetColumnIndex(1);
                hasChanged |= DrawCheckboxTable(Service.Configuration.HideFlyTextKindPlayer, kind);

                ImGui.TableSetColumnIndex(2);
                hasChanged |= DrawCheckboxTable(Service.Configuration.HideFlyTextKindOthers, kind);

                ImGui.TableSetColumnIndex(3);
                hasChanged |= DrawCheckboxTable(Service.Configuration.HideFlyTextKindOnPlayer, kind);

                ImGui.TableSetColumnIndex(4);
                hasChanged |= DrawCheckboxTable(Service.Configuration.HideFlyTextKindOnOthers, kind);
            }

            ImGui.EndTable();
        }

        if (hasChanged)
        {
            Service.Configuration.Save();
        }
    }

    private static bool DrawCheckboxTable(ISet<FlyTextKind> collection, FlyTextKind kind)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() / 2) - 12);

        var tmpKind = collection.Contains(kind);
        ImGui.Checkbox($"###{kind}{ImGui.TableGetColumnIndex()}", ref tmpKind);

        if (tmpKind != collection.Contains(kind))
        {
            if (tmpKind)
            {
                collection.Add(kind);
            }
            else
            {
                collection.Remove(kind);
            }

            return true;
        }

        return false;
    }
}
