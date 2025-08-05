using System;
using System.Collections.Concurrent;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FlyTextFilter.Model;

namespace FlyTextFilter.GUI.TypesTab;

public class TypesTab
{
    private readonly TypesTable typesTable = new();
    private ConcurrentDictionary<FlyTextKind, FlyTextSetting>? importedFlyTextSettings;

    public void Draw()
    {
        if (!Service.ConfigWindow.HasPassedTests)
        {
            DisabledTab.Draw();
            return;
        }

        if (Util.DrawButtonIcon(FontAwesomeIcon.Trash))
        {
            ImGui.OpenPopup("##ConfirmationDeleteAll");
        }

        Util.SetHoverTooltip("Delete all type settings");

        DrawConfirmationDeleteAllPopup();

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.SignOutAlt))
        {
            ImGui.SetClipboardText(ImportExport.ExportFlyTextSettings(Service.Configuration.FlyTextSettings));
            Service.ConfigWindow.UpdateMessage("Copied to clipboard.", ImGuiColors.DalamudWhite);
        }

        Util.SetHoverTooltip("Export all types");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.SignInAlt))
        {
            try
            {
                this.importedFlyTextSettings = ImportExport.ImportFlyTextSettings(ImGui.GetClipboardText());
                ImGui.OpenPopup("##ConfirmationImportAll");
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, "Failed to import settings from clipboard.");
                Service.ConfigWindow.UpdateMessage("Failed to import settings from clipboard.", ImGuiColors.DalamudRed);
            }
        }

        Util.SetHoverTooltip("Import all types");

        this.DrawConfirmationImportAllPopup();

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.InfoCircle))
        {
            ImGui.OpenPopup("##TypeHelp");
        }

        Util.SetHoverTooltip("Help");

        DrawTypeHelpPopup();

        ImGui.AlignTextToFramePadding();
        Service.ConfigWindow.DrawMessage();

        this.typesTable.Draw();
    }

    private static void DrawTypeHelpPopup()
    {
        if (ImGui.BeginPopup("##TypeHelp"))
        {
            ImGui.Text("The filtering is based on source and target, spread in 3 different groups:");
            ImGui.Text("(Y)ou - Only from or to you.");
            ImGui.Text("(P)arty - Only from or to your party members, excluding you.");
            ImGui.Text("(O)thers - From or to everyone, excluding you and your party members.");
            ImGui.Text("The main table can be a bit confusing at first. The top part (You - Party - Others) is the source," +
                       "\nand every letter underneath refers to the target.");
            ImGui.Text("For example:");
            ImGui.Text("   - the column Y under You refers to the fly texts from you to you.");
            ImGui.Text("   - the column P under You refers to the fly texts from you to party members.");
            ImGui.Text("   - the column Y under Party refers to the fly texts from your party members to you.");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("If you are still confused, use this button in the table: ");

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(FontAwesomeIcon.Edit.ToIconString());
            ImGui.PopFont();

            ImGui.EndPopup();
        }
    }

    private static void DrawConfirmationDeleteAllPopup()
    {
        if (ImGui.BeginPopup("##ConfirmationDeleteAll", ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("This will reset all your type settings contained in the table below.\nAre you sure you want to do this?");
            ImGui.Separator();
            if (ImGui.Button("Yes##ConfirmationDeleteAll"))
            {
                Service.Configuration.FlyTextSettings = new ConcurrentDictionary<FlyTextKind, FlyTextSetting>();
                Service.Configuration.Save();
                Service.ConfigWindow.UpdateMessage("Reset OK.", ImGuiColors.DalamudWhite);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("No##ConfirmationDeleteAll"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawConfirmationImportAllPopup()
    {
        if (ImGui.BeginPopup("##ConfirmationImportAll", ImGuiWindowFlags.NoMove))
        {
            ImGui.Text($"This will replace all your type settings, the imported settings have {this.importedFlyTextSettings!.Count} type(s) set.\nAre you sure you want to do this?");
            ImGui.Separator();
            if (ImGui.Button("Yes##ConfirmationImportAll"))
            {
                Service.Configuration.FlyTextSettings = this.importedFlyTextSettings;
                Service.Configuration.Save();
                this.importedFlyTextSettings = null;
                Service.ConfigWindow.UpdateMessage("Import OK.", ImGuiColors.DalamudWhite);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("No##ConfirmationImportAll"))
            {
                this.importedFlyTextSettings = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}
