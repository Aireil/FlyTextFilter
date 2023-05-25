using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FlyTextFilter.Model;
using FlyTextFilter.Model.FlyTextAdjustments;

namespace FlyTextFilter;

public unsafe class FlyTextHandler
{
    public bool ShouldLog;
    public bool HasLoadingFailed;
    public ConcurrentQueue<FlyTextLog> Logs = new();

    private readonly short flyTextArrayOffset;
    private readonly short popupTextScaleOffset;
    private readonly byte flyTextScaleOffset;
    private readonly delegate* unmanaged<long, long> getTargetIdDelegate; // BattleChara_vf84 in 6.2
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
        byte damageType);
    private readonly Hook<AddToScreenLogWithScreenLogKindDelegate>? addToScreenLogWithScreenLogKindHook;

    private delegate void* AddToScreenLogDelegate(
        long targetId,
        FlyTextCreation* flyTextCreation);
    private readonly Hook<AddToScreenLogDelegate>? addToScreenLogHook;

    public FlyTextHandler()
    {
        nint addonFlyTextOnSetupAddress;
        nint getTargetIdAddress;
        nint addToScreenLogWithScreenLogKindAddress;
        nint addToScreenLogAddress;

        try
        {
            addonFlyTextOnSetupAddress = Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 80 89");
            getTargetIdAddress = Service.SigScanner.ScanText("48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 89 5C 24 ?? 48 89 74 24");
            addToScreenLogWithScreenLogKindAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 39");
            addToScreenLogAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 83 7D 7F 00 0F 86");

            this.flyTextArrayOffset = *(short*)Service.SigScanner.ScanModule("?? ?? ?? ?? C7 83 ?? ?? ?? ?? ?? ?? ?? ?? 33 ED C7");
            this.flyTextScaleOffset = *(byte*)Service.SigScanner.ScanModule("?? BA ?? ?? ?? ?? F3 0F 59 05 ?? ?? ?? ?? 48 8B CF F3 4C 0F 2C C0");
            this.popupTextScaleOffset = *(short*)Service.SigScanner.ScanModule("?? ?? ?? ?? BA ?? ?? ?? ?? F3 0F 59 05 ?? ?? ?? ?? 49 8B CD 48 8B 84 24 ?? ?? ?? ?? 48 89 87");
        }
        catch (Exception ex)
        {
            this.HasLoadingFailed = true;
            PluginLog.Error(ex, "Sig scan failed.");
            return;
        }

        Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;

        this.getTargetIdDelegate = (delegate* unmanaged<long, long>)getTargetIdAddress;

        this.addonFlyTextOnSetupHook = Hook<AddonFlyTextOnSetupDelegate>.FromAddress(addonFlyTextOnSetupAddress, this.AddonFlyTextOnSetupDetour);
        this.addonFlyTextOnSetupHook.Enable();

        this.addToScreenLogWithScreenLogKindHook = Hook<AddToScreenLogWithScreenLogKindDelegate>.FromAddress(addToScreenLogWithScreenLogKindAddress, this.AddToScreenLogWithScreenLogKindDetour);
        this.addToScreenLogWithScreenLogKindHook.Enable();

        this.addToScreenLogHook = Hook<AddToScreenLogDelegate>.FromAddress(addToScreenLogAddress, this.AddToScreenLogDetour);
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
        var agent = (nint)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ScreenLog);
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
            var targetId = this.getTargetIdDelegate(localPlayer.Value);
            int val1;
            if (flyTextKind is FlyTextKind.NamedIcon2 or FlyTextKind.NamedIconFaded2)
                val1 = 3166;
            else if (flyTextKind is FlyTextKind.NamedIcon or FlyTextKind.NamedIconFaded)
                val1 = 3260;
            else
                val1 = 1111;

            var val2 = 0;
            if (flyTextKind is FlyTextKind.Exp or FlyTextKind.IslandExp)
            {
                val2 = 10;
            }

            var actionId = flyTextKind is FlyTextKind.NamedAttack2 or FlyTextKind.NamedCriticalHit2 ? 16230 : 2555;

            var flyTextCreation = new FlyTextCreation
            {
                FlyTextKind = flyTextKind,
                SourceStyle = sourceStyle,
                TargetStyle = targetStyle,
                Option = 5,
                ActionKind = (byte)(flyTextKind == FlyTextKind.NamedIconWithItemOutline ? 2 : 1),
                ActionId = actionId,
                Val1 = val1,
                Val2 = val2,
                DamageType = 1,
            };

            this.val1Preview = flyTextCreation.Val1;

            this.addToScreenLogHook?.Original(targetId, &flyTextCreation);
        }
    }

    public void CreateFlyText(FlyTextLog flyTextLog)
    {
        var localPlayer = Service.ClientState.LocalPlayer?.Address;
        if (localPlayer != null)
        {
            var targetId = this.getTargetIdDelegate(localPlayer.Value);
            var flyTextCreation = flyTextLog.FlyTextCreation;
            this.val1Preview = flyTextCreation.Val1;
            this.addToScreenLogHook?.Original(targetId, &flyTextCreation);
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
            case FlyTextKind.AutoAttack:
            case FlyTextKind.CriticalHit:
            case FlyTextKind.DirectHit:
            case FlyTextKind.CriticalDirectHit:
                if (Service.Configuration.FlyTextAdjustments.ShouldHideDamageTypeIconAutoAttacks)
                {
                    return true;
                }

                break;

            case FlyTextKind.NamedIcon:
            case FlyTextKind.NamedIcon2:
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
        if (!Service.Configuration.Blacklist.Any())
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
            PluginLog.Error(ex, "Exception in AddonFlyTextOnSetupDetour");
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
        byte damageType)
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
                                         + $"S{(source->GameObject.ObjectKind == 1 ? "-" : $"\"{MemoryHelper.ReadSeStringNullTerminated((nint)source->GameObject.Name)}\"")}"
                                         + $"T{(target->GameObject.ObjectKind == 1 ? "-" : $"\"{MemoryHelper.ReadSeStringNullTerminated((nint)target->GameObject.Name)}\"")}";
                    PluginLog.Information(this.explorerString);
                }
                catch
                {
                    PluginLog.Information("Unknown type found part I: failed to get info");
                }
            }

            if (damageType is 1 or 2 or 3 && ShouldHideDamageTypeIcon(flyTextKind))
            {
                damageType = 0;
            }

            var adjustedSource = source;
            if ((Service.Configuration.ShouldAdjustDotSource && flyTextKind == FlyTextKind.AutoAttack && option == 0 && actionKind == 0 && target == source)
                || (Service.Configuration.ShouldAdjustPetSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Pet && source->CompanionOwnerID == Service.ClientState.LocalPlayer?.ObjectId)
                || (Service.Configuration.ShouldAdjustChocoboSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Chocobo && source->CompanionOwnerID == Service.ClientState.LocalPlayer?.ObjectId))
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
            PluginLog.Error(ex, "Exception in AddScreenLogDetour");
        }

        this.addToScreenLogWithScreenLogKindHook!.Original(target, source, flyTextKind, option, actionKind, actionId, val1, val2, damageType);
    }

    private readonly List<FlyTextKind> seenExplorer = new();
    private string? explorerString;

    private bool IsExplorerAndUnknownType(FlyTextKind flyTextKind)
    {
        if (!Service.Configuration.IsExplorer)
        {
            return false;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (flyTextKind)
        {
            case FlyTextKind.AutoAttackNoText3:
            case FlyTextKind.CriticalHit4:
            case FlyTextKind.NamedCriticalHitWithMp:
            case FlyTextKind.NamedMp3:
                return !this.seenExplorer.Contains(flyTextKind);
        }

        return false;
    }

    private void* AddToScreenLogDetour(long targetId, FlyTextCreation* flyTextCreation)
    {
        try
        {
            if (this.IsExplorerAndUnknownType(flyTextCreation->FlyTextKind))
            {
                PluginLog.Information($"Unknown type found part II: {flyTextCreation->FlyTextKind}");
                Service.ChatGui.PrintError($"[FlyTextFilter] You found the unknown type: {flyTextCreation->FlyTextKind}! Please copy the text in the next message and ping Aireil#8310 on Discord with it or send it as a feedback in the installer. Thank you!");
                Service.ChatGui.PrintError($"\"K{(uint)flyTextCreation->FlyTextKind}{this.explorerString}\"");
                this.explorerString = null;
                this.seenExplorer.Add(flyTextCreation->FlyTextKind);
            }

            bool shouldFilter;

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
            PluginLog.Error(ex, "Exception in AddToScreenLogDetour");
        }

        return this.addToScreenLogHook!.Original(targetId, flyTextCreation);
    }
}
