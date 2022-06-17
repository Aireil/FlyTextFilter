using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Interface.Windowing;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter
{
    internal class ConfigWindow : Window
    {
        private readonly IOrderedEnumerable<FlyTextKind> sortedFlyTextKinds;
        private string addToBlacklist = string.Empty;

        public ConfigWindow()
            : base("FlyTextFilter")
        {
            this.RespectCloseHotkey = true;

            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.Size = new Vector2(600, 700);

            this.sortedFlyTextKinds = ((FlyTextKind[])Enum.GetValues(typeof(FlyTextKind))).OrderBy(x => x.ToString());
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("###ConfigTab"))
            {
                if (ImGui.BeginTabItem("Types"))
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

                        foreach (var kind in this.sortedFlyTextKinds)
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

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Text Blacklist"))
                {
                    ImGui.Text("Any exact match in Text1 or Text2 will filter the fly text.");

                    ImGui.InputText("###addToBlacklist", ref this.addToBlacklist, 100);
                    if (ImGui.Button("Add To Blacklist"))
                    {
                        Service.Configuration.Blacklist.Add(this.addToBlacklist);
                        Service.Configuration.Save();
                    }

                    ImGui.Separator();

                    foreach (var blString in Service.Configuration.Blacklist)
                    {
                        if (ImGui.Button($"Remove###{blString}"))
                        {
                            Service.Configuration.Blacklist.Remove(blString);
                            Service.Configuration.Save();
                        }

                        ImGui.SameLine();
                        ImGui.Text(blString);
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Positions"))
                {
                    ImGui.Text("Fly texts on the player are organized in two groups:" +
                               "\n- Healing, which refers to all healing received, including HoTs." +
                               "\n- Status-Damage, which refers all status effects received/lost and damage taken." +
                               "\nThis grouping is done by Square and cannot be modified for now.");

                    var posConfig = Service.Configuration.FlyTextPositions;
                    var (healingGroupDefaultPos, statusDamageGroupDefaultPos) = FlyTextHandler.GetDefaultPositions();
                    var (width, height) = Util.GetScreenSize();

                    var tmp = posConfig.HealingGroupX ?? healingGroupDefaultPos.X;
                    if (ImGui.SliderFloat("Healing horizontal###HealingGroupPosXSlider", ref tmp, 0.0f, width))
                    {
                        posConfig.HealingGroupX = tmp;
                        Service.Configuration.Save();
                    }

                    tmp = posConfig.HealingGroupY ?? healingGroupDefaultPos.Y;
                    if (ImGui.SliderFloat("Healing vertical###HealingGroupPosYSlider", ref tmp, 0.0f, height))
                    {
                        posConfig.HealingGroupY = tmp;
                        Service.Configuration.Save();
                    }

                    tmp = posConfig.StatusDamageGroupX ?? statusDamageGroupDefaultPos.X;
                    if (ImGui.SliderFloat("Status-Damage horizontal###StatusDamageGroupPosXSlider", ref tmp, 0.0f, width))
                    {
                        posConfig.StatusDamageGroupX = tmp;
                        Service.Configuration.Save();
                    }

                    tmp = posConfig.StatusDamageGroupY ?? statusDamageGroupDefaultPos.Y;
                    if (ImGui.SliderFloat("Status-Damage vertical###StatusDamageGroupPosYSlider", ref tmp, 0.0f, height))
                    {
                        posConfig.StatusDamageGroupY = tmp;
                        Service.Configuration.Save();
                    }

                    if (ImGui.Button("Test###TestPositions"))
                    {
                        Service.FlyTextGui.AddFlyText(
                            FlyTextKind.NamedAttack2,
                            0,
                            5513,
                            0,
                            "Adloquium",
                            string.Empty,
                            4278213930,
                            2801);

                        Service.FlyTextGui.AddFlyText(
                            FlyTextKind.NamedIcon,
                            1,
                            0,
                            0,
                            "+ Galvanize",
                            string.Empty,
                            4278213930,
                            12801);

                        Service.FlyTextGui.AddFlyText(
                            FlyTextKind.NamedAttack,
                            1,
                            5998,
                            0,
                            "Triumvirate",
                            string.Empty,
                            4278190218,
                            405);
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Reset all positions###ResetPositions"))
                    {
                        FlyTextHandler.ResetPositions();
                        Service.Configuration.FlyTextPositions = new FlyTextPositions();
                        Service.Configuration.Save();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
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
}
