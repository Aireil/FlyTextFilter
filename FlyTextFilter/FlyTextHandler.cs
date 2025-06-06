﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FlyTextFilter.Model;
using FlyTextFilter.Model.FlyTextAdjustments;
using ObjectKind = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind;

namespace FlyTextFilter;

public unsafe class FlyTextHandler
{
    public bool ShouldLog;
    public bool HasLoadingFailed;
    public ConcurrentQueue<FlyTextLog> Logs = new();

    private readonly short flyTextArrayOffset;
    private readonly short popupTextScaleOffset;
    private readonly byte flyTextScaleOffset;
    private readonly delegate* unmanaged<long, long> getScreenLogManagerDelegate; // Client::Game::Character::BattleChara_GetScreenLogManager in 6.4
    private int? val1Preview;

    private delegate void* AddonFlyTextOnSetupDelegate(
        void* a1,
        void* a2,
        void* a3);
    private readonly Hook<AddonFlyTextOnSetupDelegate>? addonFlyTextOnSetupHook;

    private delegate void AddToScreenLogWithScreenLogKindDelegate(
        Character* target,
        Character* source,
        FlyTextKind logKind,
        byte option,
        byte actionKind,
        int actionId,
        int val1,
        int val2,
        int val3);
    private readonly Hook<AddToScreenLogWithScreenLogKindDelegate>? addToScreenLogWithScreenLogKindHook;

    private delegate void* AddToScreenLogDelegate(
        long screenLogManager,
        FlyTextCreation* flyTextCreation);
    private readonly Hook<AddToScreenLogDelegate>? addToScreenLogHook;

    public FlyTextHandler()
    {
        nint addonFlyTextOnSetupAddress;
        nint getScreenLogManagerAddress;
        nint addToScreenLogWithScreenLogKindAddress;
        nint addToScreenLogAddress;

        try
        {
            addonFlyTextOnSetupAddress = Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 80 89");

            // the BattleChara vf number (x8) is near the end of addToScreenLogWithScreenLogKind
            getScreenLogManagerAddress = Service.SigScanner.ScanText("48 8D 81 E0 22 00 00");
            addToScreenLogWithScreenLogKindAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 39");
            addToScreenLogAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 45 85 E4 0F 84 ?? ?? ?? ?? 48 8B 0D");

            this.flyTextArrayOffset = *(short*)Service.SigScanner.ScanModule("?? ?? ?? ?? C7 83 ?? ?? ?? ?? ?? ?? ?? ?? 33 ED C7");
            this.flyTextScaleOffset = *(byte*)Service.SigScanner.ScanModule("?? BA ?? ?? ?? ?? F3 0F 59 05 ?? ?? ?? ?? 48 8B CF F3 4C 0F 2C C0");
            this.popupTextScaleOffset = *(short*)Service.SigScanner.ScanModule("?? ?? ?? ?? BA ?? ?? ?? ?? F3 0F 59 05 ?? ?? ?? ?? 49 8B CD 48 8B 84 24 ?? ?? ?? ?? 48 89 87");
        }
        catch (Exception ex)
        {
            this.HasLoadingFailed = true;
            Service.PluginLog.Error(ex, "Sig scan failed.");
            return;
        }

        Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;

        this.getScreenLogManagerDelegate = (delegate* unmanaged<long, long>)getScreenLogManagerAddress;

        this.addonFlyTextOnSetupHook = Service.GameInteropProvider.HookFromAddress<AddonFlyTextOnSetupDelegate>(addonFlyTextOnSetupAddress, this.AddonFlyTextOnSetupDetour);
        this.addonFlyTextOnSetupHook.Enable();

        this.addToScreenLogWithScreenLogKindHook = Service.GameInteropProvider.HookFromAddress<AddToScreenLogWithScreenLogKindDelegate>(addToScreenLogWithScreenLogKindAddress, this.AddToScreenLogWithScreenLogKindDetour);
        this.addToScreenLogWithScreenLogKindHook.Enable();

        this.addToScreenLogHook = Service.GameInteropProvider.HookFromAddress<AddToScreenLogDelegate>(addToScreenLogAddress, this.AddToScreenLogDetour);
        this.addToScreenLogHook.Enable();

        this.ApplyPositions();
        this.ApplyScaling();
    }

    public static FlyTextPositions GetDefaultPositions()
    {
        var (width, height) = Util.GetScreenSize();
        return new FlyTextPositions
        {
            HealingGroupX = width * 49.0f / 100.0f,
            HealingGroupY = height / 2.0f,
            StatusDamageGroupX = width * 11.0f / 20.0f,
            StatusDamageGroupY = height / 2.0f,
        };
    }

    public static (float flyTextScale, float popUpTextScale) GetDefaultScaling()
    {
        var flyTextScale = 1.0f;
        if (Service.GameConfig.UiConfig.TryGetUInt("FlyTextDispSize", out var flyTextDispSize))
        {
            flyTextScale = flyTextDispSize switch
            {
                2 => // maximum
                    1.4f,
                1 => // large
                    1.2f,
                _ => 1.0f, // standard
            };
        }

        var popUpTextScale = 1.0f;
        if (Service.GameConfig.UiConfig.TryGetUInt("PopUpTextDispSize", out var popUpTextDispSize))
        {
            popUpTextScale = popUpTextDispSize switch
            {
                2 => // maximum
                    1.4f,
                1 => // large
                    1.2f,
                _ => 1.0f, // standard
            };
        }

        return (flyTextScale, popUpTextScale);
    }

    public void ResetPositions()
    {
        var defaultFlyTextPositions = GetDefaultPositions();
        this.SetPositions(defaultFlyTextPositions);
    }

    public void ApplyPositions()
    {
        var flyTextPositions = Service.Configuration.FlyTextAdjustments.FlyTextPositions;
        this.SetPositions(flyTextPositions);
    }

    public void SetPositions(FlyTextPositions flyTextPositions)
    {
        var addon = Service.GameGui.GetAddonByName("_FlyText");
        if (addon == nint.Zero || this.HasLoadingFailed)
        {
            return;
        }

        var flyTextArray = (FlyTextArray*)(addon + this.flyTextArrayOffset);

        if (flyTextPositions.HealingGroupX != null)
        {
            (*flyTextArray)[0]->X = flyTextPositions.HealingGroupX.Value;
        }

        if (flyTextPositions.HealingGroupY != null)
        {
            (*flyTextArray)[0]->Y = flyTextPositions.HealingGroupY.Value;
        }

        if (flyTextPositions.StatusDamageGroupX != null)
        {
            (*flyTextArray)[1]->X = flyTextPositions.StatusDamageGroupX.Value;
        }

        if (flyTextPositions.StatusDamageGroupY != null)
        {
            (*flyTextArray)[1]->Y = flyTextPositions.StatusDamageGroupY.Value;
        }
    }

    public void ResetScaling()
    {
        var (defaultFlyTextScale, defaultPopUpTextScale) = GetDefaultScaling();
        this.SetScaling(defaultFlyTextScale, defaultPopUpTextScale);
    }

    public void ApplyScaling()
    {
        var adjustmentsConfig = Service.Configuration.FlyTextAdjustments;
        this.SetScaling(adjustmentsConfig.FlyTextScale, adjustmentsConfig.PopupTextScale);
    }

    public void SetScaling(float? flyTextScale, float? popUpTextScale)
    {
        var agent = (nint)Framework.Instance()->GetUIModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ScreenLog);
        if (agent == nint.Zero || this.HasLoadingFailed)
        {
            return;
        }

        var currFlyTextScalePtr = (float*)(agent + this.flyTextScaleOffset);
        var currPopUpTextScalePtr = (float*)(agent + this.popupTextScaleOffset);

        if (flyTextScale != null)
        {
            *currFlyTextScalePtr = flyTextScale.Value;
        }

        if (popUpTextScale != null)
        {
            *currPopUpTextScalePtr = popUpTextScale.Value;
        }
    }

    public void CreateFlyText(FlyTextKind flyTextKind, byte sourceStyle, byte targetStyle)
    {
        var localPlayer = Service.ClientState.LocalPlayer?.Address;
        if (localPlayer != null)
        {
            var screenLogManager = this.getScreenLogManagerDelegate(localPlayer.Value);
            byte option = 5;
            var actionId = 2555;
            byte actionKind = 1;
            var val1 = 1111;
            var val2 = 0;
            var val3 = 1;
            switch (flyTextKind)
            {
                case FlyTextKind.Debuff or FlyTextKind.DebuffFading:
                    val1 = 3166;
                    break;
                case FlyTextKind.Buff or FlyTextKind.BuffFading:
                    val1 = 3260;
                    break;
                case FlyTextKind.Exp or FlyTextKind.IslandExp or FlyTextKind.Unknown17 or FlyTextKind.Unknown18:
                    option = 1;
                    val2 = 10;
                    val3 = 50;
                    break;
                case FlyTextKind.Healing or FlyTextKind.HealingCrit:
                    actionId = 16230;
                    break;
                case FlyTextKind.LootedItem:
                    actionKind = 2;
                    break;
                case FlyTextKind.Dataset:
                    val3 = 4032;
                    val2 = 35;
                    break;
            }

            var flyTextCreation = new FlyTextCreation
            {
                FlyTextKind = flyTextKind,
                SourceStyle = sourceStyle,
                TargetStyle = targetStyle,
                Option = option,
                ActionKind = actionKind,
                ActionId = actionId,
                Val1 = val1,
                Val2 = val2,
                Val3 = val3,
            };

            this.val1Preview = flyTextCreation.Val1;

            this.addToScreenLogHook?.Original(screenLogManager, &flyTextCreation);
        }
    }

    public void CreateFlyText(FlyTextLog flyTextLog)
    {
        var localPlayer = Service.ClientState.LocalPlayer?.Address;
        if (localPlayer != null)
        {
            var screenLogManager = this.getScreenLogManagerDelegate(localPlayer.Value);
            var flyTextCreation = flyTextLog.FlyTextCreation;
            this.val1Preview = flyTextCreation.Val1;
            this.addToScreenLogHook?.Original(screenLogManager, &flyTextCreation);
        }
    }

    public void Dispose()
    {
        Service.FlyTextGui.FlyTextCreated -= this.FlyTextCreate;
        this.addonFlyTextOnSetupHook?.Dispose();
        this.addToScreenLogHook?.Dispose();
        this.addToScreenLogWithScreenLogKindHook?.Dispose();
        if (!Service.Configuration.FlyTextAdjustments.IsDefaultPositions())
        {
            this.ResetPositions();
        }

        if (!Service.Configuration.FlyTextAdjustments.IsDefaultScaling())
        {
            this.ResetScaling();
        }
    }

    private static bool ShouldHideDamageTypeIcon(FlyTextKind flyTextKind)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (flyTextKind)
        {
            case FlyTextKind.AutoAttackOrDot:
            case FlyTextKind.AutoAttackOrDotDh:
            case FlyTextKind.AutoAttackOrDotCrit:
            case FlyTextKind.AutoAttackOrDotCritDh:
                if (Service.Configuration.FlyTextAdjustments.ShouldHideDamageTypeIconAutoAttacks)
                {
                    return true;
                }

                break;

            case FlyTextKind.Buff:
            case FlyTextKind.Debuff:
                if (Service.Configuration.FlyTextAdjustments.ShouldHideDamageTypeIconStatusEffects)
                {
                    return true;
                }

                break;

            default:
                if (Service.Configuration.FlyTextAdjustments.ShouldHideDamageTypeIconOthers)
                {
                    return true;
                }

                break;
        }

        return false;
    }

    private static bool ShouldFilter(Character* source, Character* target, FlyTextKind flyTextKind)
    {
        if (source != null
            && target != null &&
            Service.Configuration.FlyTextSettings.TryGetValue(flyTextKind, out var flyTextSetting))
        {
            switch (GetFlyTextCharCategory(source))
            {
                case FlyTextCharCategory.You:
                    return ShouldFilter(target, flyTextSetting.SourceYou);
                case FlyTextCharCategory.Party:
                    return ShouldFilter(target, flyTextSetting.SourceParty);
                case FlyTextCharCategory.Others:
                    return ShouldFilter(target, flyTextSetting.SourceOthers);
                case FlyTextCharCategory.None:
                default:
                    return false;
            }
        }

        return false;
    }

    private static bool ShouldFilter(Character* target, FlyTextTargets flyTextTargets)
    {
        switch (GetFlyTextCharCategory(target))
        {
            case FlyTextCharCategory.You:
                return flyTextTargets.HasFlag(FlyTextTargets.You);
            case FlyTextCharCategory.Party:
                return flyTextTargets.HasFlag(FlyTextTargets.Party);
            case FlyTextCharCategory.Others:
                return flyTextTargets.HasFlag(FlyTextTargets.Others);
            case FlyTextCharCategory.None:
            default:
                return false;
        }
    }

    private static FlyTextCharCategory GetFlyTextCharCategory(Character* character)
    {
        var localPlayer = (Character*)(Service.ClientState.LocalPlayer?.Address ?? nint.Zero);
        if (character == null || localPlayer == null)
        {
            return FlyTextCharCategory.None;
        }

        if (character == localPlayer)
        {
            return FlyTextCharCategory.You;
        }

        if (character->IsPartyMember)
        {
            return FlyTextCharCategory.Party;
        }

        return FlyTextCharCategory.Others;
    }

    private void FlyTextCreate(
        ref FlyTextKind flyTextKind,
        ref int val1,
        ref int val2,
        ref SeString text1,
        ref SeString text2,
        ref uint color,
        ref uint icon,
        ref uint damageTypeIcon,
        ref float yOffset,
        ref bool handled)
    {
        if (Service.Configuration.Blacklist.Count == 0)
        {
            return;
        }

        // preview
        if (this.val1Preview != null && val1 == this.val1Preview)
        {
            this.val1Preview = null;
            return;
        }

        var text1Adjusted = text1.ToString();

        // status effects
        if (text1.TextValue.StartsWith("+ ") || text1.TextValue.StartsWith("- "))
        {
            text1Adjusted = text1.TextValue[2..];
        }

        if (Service.Configuration.Blacklist.Contains(text1Adjusted)
            || Service.Configuration.Blacklist.Contains(text2.TextValue))
        {
            handled = true;
            if (this.ShouldLog)
            {
                var last = this.Logs.LastOrDefault();
                if (last != null && last.FlyTextCreation.FlyTextKind == flyTextKind && last.FlyTextCreation.Val1 == val1)
                {
                    last.WasFiltered = true;
                }
            }
        }
    }

    private void AddLog(FlyTextLog flyTextLog)
    {
        this.Logs.Enqueue(flyTextLog);

        while (this.Logs.Count > Service.Configuration.NbOfLogs)
        {
            this.Logs.TryDequeue(out _);
        }
    }

    private void* AddonFlyTextOnSetupDetour(void* a1, void* a2, void* a3)
    {
        var result = this.addonFlyTextOnSetupHook!.Original(a1, a2, a3);
        try
        {
            this.ApplyPositions();
            this.ApplyScaling();
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in AddonFlyTextOnSetupDetour");
        }

        return result;
    }

    private void AddToScreenLogWithScreenLogKindDetour(
        Character* target,
        Character* source,
        FlyTextKind flyTextKind,
        byte option, // 0 = DoT / 1 = % increase / 2 = blocked / 3 = parried / 4 = resisted / 5 = default
        byte actionKind,
        int actionId,
        int val1,
        int val2,
        int val3)
    {
        try
        {
            if (this.IsExplorerAndUnknownType(flyTextKind))
            {
                try
                {
                    this.explorerString = $"K{(uint)flyTextKind}"
                                         + $"T{Service.ClientState.TerritoryType}"
                                         + $"A{actionId}"
                                         + $"S{(source->GameObject.ObjectKind == ObjectKind.Pc ? "-" : $"\"{source->GameObject.NameString}\"")}"
                                         + $"T{(target->GameObject.ObjectKind == ObjectKind.Pc ? "-" : $"\"{target->GameObject.NameString}\"")}";
                    Service.PluginLog.Information(this.explorerString);
                }
                catch
                {
                    Service.PluginLog.Information("Unknown type found part I: failed to get info");
                }
            }

            var adjustedSource = source;
            if ((Service.Configuration.ShouldAdjustDotSource && flyTextKind == FlyTextKind.AutoAttackOrDot && option == 0 && actionKind == 0 && target == source)
                || (Service.Configuration.ShouldAdjustPetSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Pet && source->CompanionOwnerId == Service.ClientState.LocalPlayer?.GameObjectId)
                || (Service.Configuration.ShouldAdjustChocoboSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Chocobo && source->CompanionOwnerId == Service.ClientState.LocalPlayer?.GameObjectId))
            {
                adjustedSource = (Character*)(Service.ClientState.LocalPlayer?.Address ?? nint.Zero);
                if (adjustedSource == null)
                {
                    adjustedSource = source;
                }
            }

            var shouldFilter = ShouldFilter(adjustedSource, target, flyTextKind);

            if (this.ShouldLog)
            {
                var flyTextLog = new FlyTextLog
                {
                    SourceCategory = GetFlyTextCharCategory(adjustedSource),
                    TargetCategory = GetFlyTextCharCategory(target),
                    HasSourceBeenAdjusted = source != adjustedSource,
                    WasFiltered = shouldFilter,
                    IsPartial = true,
                };

                this.AddLog(flyTextLog);
            }

            option += (byte)(shouldFilter ? 150 : 100); // still go in AddToScreenLogDetour, but keep the filtering
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in AddScreenLogDetour");
        }

        this.addToScreenLogWithScreenLogKindHook!.Original(target, source, flyTextKind, option, actionKind, actionId, val1, val2, val3);
    }

    private readonly List<FlyTextKind> seenExplorer = [];
    private string? explorerString;

    private bool IsExplorerAndUnknownType(FlyTextKind flyTextKind)
    {
        if (!Service.Configuration.IsExplorer)
        {
            return false;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        return flyTextKind switch
        {
            FlyTextKind.AutoAttackNoText3
                or FlyTextKind.CriticalHit4
                or FlyTextKind.NamedCriticalHitWithMp
                or FlyTextKind.NamedMp3 => !this.seenExplorer.Contains(flyTextKind),
            _ => false,
        };
    }

    private void* AddToScreenLogDetour(long screenLogManager, FlyTextCreation* flyTextCreation)
    {
        try
        {
            if (this.IsExplorerAndUnknownType(flyTextCreation->FlyTextKind))
            {
                Service.PluginLog.Information($"Unknown type found part II: {flyTextCreation->FlyTextKind}");
                Service.ChatGui.PrintError($"[FlyTextFilter] You found the unknown type: {flyTextCreation->FlyTextKind}! Please copy the text in the next message and ping Aireil on Discord with it or send it as a feedback in the installer. Thank you!");
                Service.ChatGui.PrintError($"\"K{(uint)flyTextCreation->FlyTextKind}{this.explorerString}\"");
                this.explorerString = null;
                this.seenExplorer.Add(flyTextCreation->FlyTextKind);
            }

            bool shouldFilter;

            if (flyTextCreation->Val3 is 1 or 2 or 3 && ShouldHideDamageTypeIcon(flyTextCreation->FlyTextKind))
            {
                flyTextCreation->Val3 = 0;
            }

            // classic function
            if (flyTextCreation->Option >= 100)
            {
                // should filter
                if (flyTextCreation->Option >= 150)
                {
                    flyTextCreation->Option -= 150;
                    shouldFilter = true;
                }
                else
                {
                    flyTextCreation->Option -= 100;
                    shouldFilter = false;
                }

                if (this.ShouldLog)
                {
                    foreach (var flyTextLog in this.Logs)
                    {
                        if (flyTextLog.IsPartial)
                        {
                            flyTextLog.FlyTextCreation = *flyTextCreation;
                            flyTextLog.IsPartial = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                // item or crafting function
                var localPlayer = (Character*)(Service.ClientState.LocalPlayer?.Address ?? nint.Zero);
                shouldFilter = ShouldFilter(localPlayer, localPlayer, flyTextCreation->FlyTextKind);

                if (this.ShouldLog)
                {
                    var flyTextLog = new FlyTextLog
                    {
                        FlyTextCreation = *flyTextCreation,
                        SourceCategory = FlyTextCharCategory.You,
                        TargetCategory = FlyTextCharCategory.You,
                        WasFiltered = shouldFilter,
                    };

                    this.AddLog(flyTextLog);
                }
            }

            if (shouldFilter)
            {
                return (void*)0;
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in AddToScreenLogDetour");
        }

        return this.addToScreenLogHook!.Original(screenLogManager, flyTextCreation);
    }
}
