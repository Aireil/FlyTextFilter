using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
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
            this.Size = new Vector2(740, 490);

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

                    if (ImGui.BeginTable("###TableKinds", 3))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None);
                        ImGui.TableSetupColumn("Hide, from You", ImGuiTableColumnFlags.None);
                        ImGui.TableSetupColumn("Hide, from Others", ImGuiTableColumnFlags.None);
                        ImGui.TableHeadersRow();

                        foreach (var kind in this.sortedFlyTextKinds)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);

                            ImGui.Text($"{kind.ToString()}");

                            ImGui.TableSetColumnIndex(1);

                            var tmpPlayer = Service.Configuration.HideFlyTextKindPlayer.Contains(kind);
                            hasChanged |= ImGui.Checkbox($"###{kind}player", ref tmpPlayer);

                            if (tmpPlayer != Service.Configuration.HideFlyTextKindPlayer.Contains(kind))
                            {
                                if (tmpPlayer)
                                {
                                    Service.Configuration.HideFlyTextKindPlayer.Add(kind);
                                }
                                else
                                {
                                    Service.Configuration.HideFlyTextKindPlayer.Remove(kind);
                                }
                            }

                            ImGui.TableSetColumnIndex(2);

                            var tmpOthers = Service.Configuration.HideFlyTextKindOthers.Contains(kind);
                            hasChanged |= ImGui.Checkbox($"###{kind}others", ref tmpOthers);

                            if (tmpOthers != Service.Configuration.HideFlyTextKindOthers.Contains(kind))
                            {
                                if (tmpOthers)
                                {
                                    Service.Configuration.HideFlyTextKindOthers.Add(kind);
                                }
                                else
                                {
                                    Service.Configuration.HideFlyTextKindOthers.Remove(kind);
                                }
                            }
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

                ImGui.EndTabBar();
            }
        }
    }
}
