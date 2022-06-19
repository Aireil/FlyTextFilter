using Dalamud.Game.Gui.FlyText;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter.GUI;

public class PositionsTab
{
    public static void Draw()
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
            FlyTextHandler.SetPositions();

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
    }
}
