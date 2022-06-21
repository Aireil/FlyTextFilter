﻿using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using FlyTextFilter.GUI.TypesTab;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter.GUI;

public class LogTab
{
    public static void Draw()
    {
        if (!Service.ConfigWindow.HasPassedTests)
        {
            ImGui.Text("Tests failed, this tab has been disabled to protect your config." +
                       "\nSee the Types tab for more information.");
            return;
        }

        ImGui.Checkbox("Log fly texts", ref Service.FlyTextHandler.ShouldLog);

        Util.DrawHelp("Log all fly texts in the table below, including filtered ones." +
                      "\nIf a fly text is not logged, please report it with steps to reproduce it." +
                      "\nThis logging is turned off when restarting to avoid useless processing");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(34.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt("Max number of logs", ref Service.Configuration.NbOfLogs, 0))
        {
            if (Service.Configuration.NbOfLogs < 1)
            {
                Service.Configuration.NbOfLogs = 1;
            }
            else if (Service.Configuration.NbOfLogs > 500)
            {
                Service.Configuration.NbOfLogs = 500;
            }

            Service.Configuration.Save();
        }

        if (ImGui.BeginTable(
                "##LogTable",
                7,
                ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
                new Vector2(-1.0f, 600.0f * ImGuiHelpers.GlobalScale)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            DrawHeader();

            for (var i = Service.FlyTextHandler.Logs.Count - 1; i >= 0; i--)
            {
                ImGui.PushID($"{i}RowLog");

                var hasChanged = false;

                var flyTextLog = Service.FlyTextHandler.Logs.ElementAt(i);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{flyTextLog.FlyTextKind}  ");

                ImGui.TableNextColumn();
                if (FlyTextKindData.HasInfo(flyTextLog.FlyTextKind))
                {
                    ImGui.AlignTextToFramePadding();
                    Util.CenterIcon(FontAwesomeIcon.Question);

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(FlyTextKindData.GetInfoFormatted(flyTextLog.FlyTextKind));
                    }
                }

                ImGui.TableNextColumn();
                if (Util.CenterButtonIcon(FontAwesomeIcon.Eye))
                {
                    Service.FlyTextHandler.CreateFlyText(flyTextLog);
                }

                Util.SetHoverTooltip("Create this fly text on you");

                ImGui.TableNextColumn();
                if (Util.CenterButtonIcon(FontAwesomeIcon.Edit))
                {
                    ImGui.OpenPopup("##PopupEditKind");
                }

                Util.SetHoverTooltip("Edit");

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Util.CenterText(flyTextLog.SourceCategory + (flyTextLog.HasSourceBeenAdjusted ? " (?)" : string.Empty));
                if (flyTextLog.HasSourceBeenAdjusted)
                {
                    Util.SetHoverTooltip("This fly text original source has been adjusted, see Misc tab for more info.");
                }

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Util.CenterText(flyTextLog.TargetCategory.ToString());

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Util.CenterText(flyTextLog.Val1 + " ");

                Service.Configuration.FlyTextSettings.TryGetValue(flyTextLog.FlyTextKind, out var flyTextSetting);
                flyTextSetting ??= new FlyTextSetting();
                hasChanged |= EditPopup.Draw(flyTextLog.FlyTextKind, flyTextSetting);

                if (hasChanged)
                {
                    Service.Configuration.UpdateFlyTextSettings(flyTextLog.FlyTextKind, flyTextSetting);
                    Service.Configuration.Save();
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    public static void DrawHeader()
    {
        ImGui.TableNextColumn();
        Util.CenterText("Type");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Info ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Show (?)");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);
        Util.SetHoverTooltip("Some fly texts change color based on different criteria" +
                             "\nThe created fly text may not appear as it did.");

        ImGui.TableNextColumn();
        Util.CenterText(" Edit type ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Source ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Target ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Value (?) ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);
        Util.SetHoverTooltip("Usually contains the displayed value if there is one." +
                             "\nIf there is no value, this represents an id and can be ignored");
    }
}