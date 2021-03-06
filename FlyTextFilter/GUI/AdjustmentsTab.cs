using Dalamud.Game.Gui.FlyText;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FlyTextFilter.Model.FlyTextAdjustments;
using ImGuiNET;

namespace FlyTextFilter.GUI;

public class AdjustmentsTab
{
    public static unsafe void Draw()
    {
        ImGui.Text("Positions:");

        ImGui.Indent();
        ImGui.Text("Fly texts on the player are organized in two groups:" +
                   "\n- Healing = all healing received, including HoTs." +
                   "\n- Status-Damage = all status effects received/lost and damage taken." +
                   "\nThis grouping is done by Square and cannot be modified for now.");

        var adjustmentsConfig = Service.Configuration.FlyTextAdjustments;
        var posConfig = adjustmentsConfig.FlyTextPositions;
        var (healingGroupDefaultPos, statusDamageGroupDefaultPos) = FlyTextHandler.GetDefaultPositions();

        var tmp = posConfig.HealingGroupX ?? healingGroupDefaultPos.X;
        if (ImGui.DragFloat("Healing horizontal##HealingGroupPosXSlider", ref tmp))
        {
            posConfig.HealingGroupX = tmp;
            Service.Configuration.Save();
        }

        tmp = posConfig.HealingGroupY ?? healingGroupDefaultPos.Y;
        if (ImGui.DragFloat("Healing vertical##HealingGroupPosYSlider", ref tmp))
        {
            posConfig.HealingGroupY = tmp;
            Service.Configuration.Save();
        }

        tmp = posConfig.StatusDamageGroupX ?? statusDamageGroupDefaultPos.X;
        if (ImGui.DragFloat("Status-Damage horizontal##StatusDamageGroupPosXSlider", ref tmp))
        {
            posConfig.StatusDamageGroupX = tmp;
            Service.Configuration.Save();
        }

        tmp = posConfig.StatusDamageGroupY ?? statusDamageGroupDefaultPos.Y;
        if (ImGui.DragFloat("Status-Damage vertical##StatusDamageGroupPosYSlider", ref tmp))
        {
            posConfig.StatusDamageGroupY = tmp;
            Service.Configuration.Save();
        }

        if (ImGui.Button("Test##TestPositions"))
        {
            FlyTextHandler.SetPositions();

            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack2, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedIcon, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 0, 0);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset all positions##ResetPositions"))
        {
            FlyTextHandler.ResetPositions();
            Service.Configuration.FlyTextAdjustments.FlyTextPositions = new FlyTextPositions();
            Service.Configuration.Save();
        }

        ImGui.Unindent();

        ImGui.Separator();

        ImGui.Text("Scaling:");

        ImGui.Indent();

        ImGui.Text("Fly texts are from or on you, pop-ups are from or on others." +
                   "\nThere is more involved, but this is a good summary for most.");

        tmp = adjustmentsConfig.FlyTextScale ?? 1.0f;
        if (ImGui.DragFloat("Fly text scaling##FlyTextScaleSlider", ref tmp, 0.005f))
        {
            adjustmentsConfig.FlyTextScale = tmp;
            Service.Configuration.Save();
            FlyTextHandler.SetScaling();
        }

        tmp = adjustmentsConfig.PopupTextScale ?? 1.0f;
        if (ImGui.DragFloat("Pop-up text scaling##PopupTextScaleSlider", ref tmp, 0.005f))
        {
            adjustmentsConfig.PopupTextScale = tmp;
            Service.Configuration.Save();
            FlyTextHandler.SetScaling();
        }

        if (ImGui.Button("Test##TestScaling"))
        {
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack2, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedIcon, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 3, 2);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 2, 2);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset scaling##ResetPositions"))
        {
            FlyTextHandler.ResetScaling();
            Service.Configuration.FlyTextAdjustments.FlyTextScale = null;
            Service.Configuration.FlyTextAdjustments.PopupTextScale = null;
            Service.Configuration.Save();
        }

        ImGui.Unindent();

        ImGui.Separator();

        if (Framework.Instance()->GetUiModule()->GetConfigModule()->GetValueById((short)ConfigOption.FlyTextDisp)->Int == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Fly texts are disabled in the game settings.");
        }

        if (Framework.Instance()->GetUiModule()->GetConfigModule()->GetValueById((short)ConfigOption.PopUpTextDisp)->Int == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Pop-up texts are disabled in the game settings.");
        }

        ImGui.TextColored(ImGuiColors.DalamudGrey, "Tips: ctrl + click to edit values manually, this works in all plugins.");
    }
}
