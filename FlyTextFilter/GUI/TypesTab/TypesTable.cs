using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter.GUI.TypesTab;

public class TypesTable
{
    public static void Draw()
    {
        if (ImGui.BeginTable(
                "##TableKinds",
                8,
                ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
                new Vector2(-1.0f, 600.0f * ImGuiHelpers.GlobalScale)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            DrawHeader();

            foreach (var flyTextKind in ((FlyTextKind[])Enum.GetValues(typeof(FlyTextKind))).OrderBy(x => x.ToString()))
            {
                ImGui.PushID($"{flyTextKind}RowTypesType");

                var hasChanged = false;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{flyTextKind}  ");

                ImGui.TableNextColumn();
                if (FlyTextKindData.HasInfo(flyTextKind))
                {
                    ImGui.AlignTextToFramePadding();
                    Util.CenterIcon(FontAwesomeIcon.Question);

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(FlyTextKindData.GetInfoFormatted(flyTextKind));
                    }
                }

                ImGui.TableNextColumn();
                if (Util.CenterButtonIcon(FontAwesomeIcon.Eye))
                {
                    Service.FlyTextHandler.CreateFlyText(flyTextKind, 0, 3);
                    Service.FlyTextHandler.CreateFlyText(flyTextKind, 3, 0);
                    Service.FlyTextHandler.CreateFlyText(flyTextKind, 3, 2);
                    Service.FlyTextHandler.CreateFlyText(flyTextKind, 2, 2);
                }

                Util.SetHoverTooltip("Create a fly text of this type on you");

                Service.Configuration.FlyTextSettings.TryGetValue(flyTextKind, out var flyTextSetting);
                flyTextSetting ??= new FlyTextSetting();

                ImGui.TableNextColumn();
                Util.CenterCursor(22.0f * ImGuiHelpers.GlobalScale);
                hasChanged |= Util.CheckboxFlagAll("##AllCheckbox" + flyTextKind, ref flyTextSetting);

                ImGui.TableNextColumn();
                hasChanged |= DrawTargets("##DrawTargetsSourceYou", ref flyTextSetting.SourceYou);

                ImGui.TableNextColumn();
                hasChanged |= DrawTargets("##DrawTargetsSourceParty", ref flyTextSetting.SourceParty);

                ImGui.TableNextColumn();
                hasChanged |= DrawTargets("##DrawTargetsSourceOthers", ref flyTextSetting.SourceOthers);

                ImGui.TableNextColumn();
                if (Util.DrawButtonIcon(FontAwesomeIcon.Edit))
                {
                    ImGui.OpenPopup("##PopupEditKind");
                }

                Util.SetHoverTooltip("Edit");

                ImGui.SameLine();
                if (Util.DrawButtonIcon(FontAwesomeIcon.SignOutAlt))
                {
                    var flyTextSettingDic = new ConcurrentDictionary<FlyTextKind, FlyTextSetting>();
                    flyTextSettingDic.TryAdd(flyTextKind, flyTextSetting);
                    ImGui.SetClipboardText(ImportExport.ExportFlyTextSettings(flyTextSettingDic));
                    Service.ConfigWindow.UpdateMessage("Copied to clipboard.", ImGuiColors.DalamudWhite);
                }

                Util.SetHoverTooltip("Export single type");

                ImGui.SameLine();
                if (Util.DrawButtonIcon(FontAwesomeIcon.SignInAlt))
                {
                    try
                    {
                        var importedFlyTextSettings = ImportExport.ImportFlyTextSettings(ImGui.GetClipboardText());
                        if (importedFlyTextSettings.Count > 1)
                        {
                            Service.ConfigWindow.UpdateMessage("This import contains multiple settings.", ImGuiColors.DalamudRed);
                        }
                        else
                        {
                            if (importedFlyTextSettings.IsEmpty)
                            {
                                flyTextSetting = new FlyTextSetting();
                            }
                            else
                            {
                                foreach (var (_, importedFlyTextSetting) in importedFlyTextSettings)
                                {
                                    flyTextSetting = importedFlyTextSetting;
                                }
                            }

                            hasChanged = true;
                            Service.ConfigWindow.UpdateMessage("Import OK.", ImGuiColors.DalamudWhite);
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error(ex, "Failed to import setting from clipboard.");
                        Service.ConfigWindow.UpdateMessage("Failed to import setting from clipboard.", ImGuiColors.DalamudRed);
                    }
                }

                Util.SetHoverTooltip("Import single type");

                ImGui.SameLine();
                ImGui.Dummy(new Vector2(-5.0f));

                hasChanged |= EditPopup.Draw(flyTextKind, flyTextSetting);

                if (hasChanged)
                {
                    Service.Configuration.UpdateFlyTextSettings(flyTextKind, flyTextSetting);
                    Service.Configuration.Save();
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    private static bool DrawTargets(string id, ref FlyTextTargets targets)
    {
        var hasChanged = false;
        hasChanged |= Util.CheckboxFlag(id + "TargetYou", ref targets, FlyTextTargets.You);

        ImGui.SameLine();
        hasChanged |= Util.CheckboxFlag(id + "TargetParty", ref targets, FlyTextTargets.Party);

        ImGui.SameLine();
        hasChanged |= Util.CheckboxFlag(id + "TargetOthers", ref targets, FlyTextTargets.Others);

        return hasChanged;
    }

    private static void DrawHeader()
    {
        ImGui.TableNextColumn();
        Util.CenterText("Type", 2);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Info ", 2);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText(" Show (?)", 2);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);
        Util.SetHoverTooltip("When possible, spawns two fly texts and two pop-up texts." +
                             "\nThis will display all different styles for a type." +
                             "\nSome types only support fly texts, therefore, only" +
                             "\ntwo will appear.");

        ImGui.TableNextColumn();
        Util.CenterText("   All   ", 2);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        DrawTargetsHeader("You");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        DrawTargetsHeader("Party");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        DrawTargetsHeader("Others");
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);

        ImGui.TableNextColumn();
        Util.CenterText("Misc", 2);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xFF303033);
    }

    private static void DrawTargetsHeader(string header)
    {
        Util.CenterText(header);
        if (ImGui.BeginTable("##TargetsHeader", 3))
        {
            ImGui.TableNextColumn();
            Util.CenterText("Y");

            ImGui.TableNextColumn();
            Util.CenterText("P");

            ImGui.TableNextColumn();
            Util.CenterText("O");

            ImGui.EndTable();
        }
    }
}
