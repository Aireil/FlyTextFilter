using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FlyTextFilter.GUI
{
    internal class ConfigWindow : Window
    {
        private readonly BlacklistTab blacklistTab = new();

        public ConfigWindow()
            : base("FlyTextFilter")
        {
            this.RespectCloseHotkey = true;

            this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("###ConfigTab"))
            {
                if (ImGui.BeginTabItem("Types"))
                {
                    TypesTab.Draw();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Text Blacklist"))
                {
                    this.blacklistTab.Draw();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Positions"))
                {
                    PositionsTab.Draw();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}
