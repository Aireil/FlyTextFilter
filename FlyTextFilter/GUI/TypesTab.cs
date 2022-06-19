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
        if (!Service.ConfigWindow.hasPassedTests)
        {
            DrawDisabledTab();
            return;
        }

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

    private static void DrawDisabledTab()
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
                Service.ConfigWindow.hasPassedTests = true;
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
