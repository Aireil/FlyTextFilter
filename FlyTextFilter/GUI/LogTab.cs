using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
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

        ImGui.SameLine();
        if (ImGui.Button("Clear"))
        {
            Service.FlyTextHandler.Logs.Clear();
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
                var flyTextLog = Service.FlyTextHandler.Logs.ElementAt(i);
                if (flyTextLog.IsPartial)
                {
                    continue;
                }

                ImGui.PushID($"{i}RowLog");

                var hasChanged = false;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{FlyTextKindData.GetAlias(flyTextLog.FlyTextCreation.FlyTextKind)}  ");

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
                Util.CenterText(flyTextLog.WasFiltered ? "Yes" : "No");

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

                var value = flyTextLog.FlyTextCreation.Val1 == 0 ? flyTextLog.FlyTextCreation.Val2 : flyTextLog.FlyTextCreation.Val1;

                Util.CenterText(value + " ");

                Service.Configuration.FlyTextSettings.TryGetValue(flyTextLog.FlyTextCreation.FlyTextKind, out var flyTextSetting);
                flyTextSetting ??= new FlyTextSetting();
                hasChanged |= EditPopup.Draw(flyTextLog.FlyTextCreation.FlyTextKind, flyTextSetting);

                if (hasChanged)
                {
                    Service.Configuration.UpdateFlyTextSettings(flyTextLog.FlyTextCreation.FlyTextKind, flyTextSetting);
                    Service.Configuration.Save();
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    public static void DrawHeader()
    {
        var headerColor = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.TableHeaderBg]);

        ImGui.TableNextColumn();
        Util.CenterText(" Type ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);

        ImGui.TableNextColumn();
        Util.CenterText(" Show (?)");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);
        Util.SetHoverTooltip("If you use Damage Info, the created fly text" +
                             "\nmay not have the same color as it did.");

        ImGui.TableNextColumn();
        Util.CenterText(" Edit type ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);

        ImGui.TableNextColumn();
        Util.CenterText(" Filtered? ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);

        ImGui.TableNextColumn();
        Util.CenterText(" Source ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);

        ImGui.TableNextColumn();
        Util.CenterText(" Target ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);

        ImGui.TableNextColumn();
        Util.CenterText(" Value (?) ");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, headerColor);
        Util.SetHoverTooltip("Usually contains the displayed value if there is one." +
                             "\nIf there is no value, this represents an id and can be ignored");
    }
}
