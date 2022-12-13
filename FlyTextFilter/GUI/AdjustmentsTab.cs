using Dalamud.Game.Gui.FlyText;
using Dalamud.Interface.Colors;
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
        var defaultPosConfig = FlyTextPositions.GetDefaultPositions();

        var hasPosChanged = false;
        var tmp = posConfig.HealingGroupX ?? defaultPosConfig.HealingGroupX!.Value;
        if (ImGui.DragFloat("Healing horizontal##HealingGroupPosXSlider", ref tmp))
        {
            posConfig.HealingGroupX = tmp;
            hasPosChanged = true;
        }

        tmp = posConfig.HealingGroupY ?? defaultPosConfig.HealingGroupY!.Value;
        if (ImGui.DragFloat("Healing vertical##HealingGroupPosYSlider", ref tmp))
        {
            posConfig.HealingGroupY = tmp;
            hasPosChanged = true;
        }

        tmp = posConfig.StatusDamageGroupX ?? defaultPosConfig.StatusDamageGroupX!.Value;
        if (ImGui.DragFloat("Status-Damage horizontal##StatusDamageGroupPosXSlider", ref tmp))
        {
            posConfig.StatusDamageGroupX = tmp;
            hasPosChanged = true;
        }

        tmp = posConfig.StatusDamageGroupY ?? defaultPosConfig.StatusDamageGroupY!.Value;
        if (ImGui.DragFloat("Status-Damage vertical##StatusDamageGroupPosYSlider", ref tmp))
        {
            posConfig.StatusDamageGroupY = tmp;
            hasPosChanged = true;
        }

        if (ImGui.Button("Test##TestPositions"))
        {
            CreateFlyPopupText(true, false);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset all positions##ResetPositions"))
        {
            FlyTextHandler.ResetPositions();
            Service.Configuration.FlyTextAdjustments.FlyTextPositions = new FlyTextPositions();
            hasPosChanged = true;
        }

        if (hasPosChanged)
        {
            Service.Configuration.Save();
            FlyTextHandler.ApplyPositions();
            CreateFlyPopupText(true, false);
        }

        ImGui.Unindent();

        ImGui.Separator();

        ImGui.Text("Scaling:");

        ImGui.Indent();

        ImGui.Text("Fly texts are from or on you, pop-ups are from or on others." +
                   "\nThere is more involved, but this is a good summary for most.");

        var hasPopUpTextPosChanged = false;
        tmp = adjustmentsConfig.FlyTextScale ?? 1.0f;
        if (ImGui.DragFloat("Fly text scaling##FlyTextScaleSlider", ref tmp, 0.005f))
        {
            adjustmentsConfig.FlyTextScale = tmp;
            hasPopUpTextPosChanged = true;
        }

        tmp = adjustmentsConfig.PopupTextScale ?? 1.0f;
        if (ImGui.DragFloat("Pop-up text scaling##PopupTextScaleSlider", ref tmp, 0.005f))
        {
            adjustmentsConfig.PopupTextScale = tmp;
            hasPopUpTextPosChanged = true;
        }

        if (ImGui.Button("Test##TestScaling"))
        {
            CreateFlyPopupText(true, true);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset scaling##ResetPositions"))
        {
            FlyTextHandler.ResetScaling();
            Service.Configuration.FlyTextAdjustments.FlyTextScale = null;
            Service.Configuration.FlyTextAdjustments.PopupTextScale = null;
            hasPopUpTextPosChanged = true;
        }

        if (hasPopUpTextPosChanged)
        {
            Service.Configuration.Save();
            FlyTextHandler.ApplyScaling();
            CreateFlyPopupText(true, true);
        }

        ImGui.Unindent();

        ImGui.Separator();

        if (ConfigModule.Instance()->GetIntValue(ConfigOption.FlyTextDisp) == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Fly texts are disabled in the game settings.");
        }

        if (ConfigModule.Instance()->GetIntValue(ConfigOption.PopUpTextDisp) == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Pop-up texts are disabled in the game settings.");
        }

        ImGui.TextColored(ImGuiColors.DalamudGrey, "Tip: ctrl + click to directly edit values, this works in all plugins.");
    }

    public static void CreateFlyPopupText(bool createFlyText, bool createPopUpText)
    {
        if (createFlyText)
        {
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack2, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedIcon, 0, 0);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 0, 0);
        }

        if (createPopUpText)
        {
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 3, 2);
            Service.FlyTextHandler.CreateFlyText(FlyTextKind.NamedAttack, 2, 2);
        }
    }
}
