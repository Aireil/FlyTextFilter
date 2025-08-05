using System;
using System.Diagnostics;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FlyTextFilter.Model;

namespace FlyTextFilter;

public class Util
{
    public static (float width, float height) GetScreenSize()
    {
        float width;
        float height;
        try
        {
            width = ImGui.GetIO().DisplaySize.X;
            height = ImGui.GetIO().DisplaySize.Y;
        }
        catch (NullReferenceException)
        {
            width = 1920.0f;
            height = 1080.0f;
        }

        return (width, height);
    }

    public static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = link,
            UseShellExecute = true,
        });
    }

    public static bool DrawButtonIcon(FontAwesomeIcon icon, Vector2? size = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (size != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, size.Value);
        }

        var ret = ImGui.Button(icon.ToIconString());

        if (size != null)
        {
            ImGui.PopStyleVar();
        }

        ImGui.PopFont();

        return ret;
    }

    public static void CenterCursor(string text)
    {
        CenterCursor(ImGui.CalcTextSize(text).X);
    }

    public static void CenterCursor(float size)
    {
        var offsetX = Round((ImGui.GetContentRegionAvail().X - size) / 2);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offsetX);
    }

    public static void CenterTextColored(Vector4 color, string text)
    {
        CenterCursor(text);
        ImGui.TextColored(color, text);
    }

    public static void CenterText(string text)
    {
        CenterCursor(text);
        ImGui.TextUnformatted(text);
    }

    public static void CenterText(string text, uint nbOfLines)
    {
        var offsetY = Round(nbOfLines * ImGui.GetFontSize() / 3.0f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offsetY);
        CenterText(text);
    }

    public static void CenterSelectable(string text, ref bool isClicked)
    {
        CenterCursor(text);
        var textSize = ImGui.CalcTextSize(text);
        ImGui.Selectable(text, ref isClicked, ImGuiSelectableFlags.None, textSize);
    }

    public static bool CenterButtonIcon(FontAwesomeIcon icon, Vector2? size = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        CenterCursor(icon.ToIconString());
        ImGui.PopFont();
        ImGui.SetCursorPosX(Round(ImGui.GetCursorPosX() - (ImGui.GetStyle().ItemSpacing.X / 2)));

        return DrawButtonIcon(icon, size);
    }

    public static void CenterIcon(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        CenterText(icon.ToIconString());
        ImGui.PopFont();
    }

    public static bool CheckboxFlag(string id, ref FlyTextTargets targets, FlyTextTargets target)
    {
        var tmp = targets.HasFlag(target);
        if (ImGui.Checkbox(id, ref tmp))
        {
            if (tmp)
            {
                targets |= target;
            }
            else
            {
                targets &= ~target;
            }

            return true;
        }

        return false;
    }

    public static bool CheckboxFlagAll(string id, ref FlyTextSetting flyTextSetting)
    {
        var tmp = flyTextSetting.SourceYou.HasFlag(FlyTextTargets.All)
                  && flyTextSetting.SourceParty.HasFlag(FlyTextTargets.All)
                  && flyTextSetting.SourceOthers.HasFlag(FlyTextTargets.All);
        if (ImGui.Checkbox(id, ref tmp))
        {
            if (tmp)
            {
                flyTextSetting.SourceYou = FlyTextTargets.All;
                flyTextSetting.SourceParty = FlyTextTargets.All;
                flyTextSetting.SourceOthers = FlyTextTargets.All;
            }
            else
            {
                flyTextSetting.SourceYou = FlyTextTargets.None;
                flyTextSetting.SourceParty = FlyTextTargets.None;
                flyTextSetting.SourceOthers = FlyTextTargets.None;
            }

            return true;
        }

        return false;
    }

    public static void SetHoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }

    public static void DrawHelp(string helpMessage)
    {
        SetHoverTooltip(helpMessage);

        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "(?)");
        SetHoverTooltip(helpMessage);
    }

    public static float Round(float value)
    {
        return (float)Math.Round(value);
    }
}
