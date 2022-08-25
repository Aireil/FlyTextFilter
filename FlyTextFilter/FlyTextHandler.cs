using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FlyTextFilter.Model;
using ImGuiNET;

namespace FlyTextFilter;

public unsafe class FlyTextHandler
{
    public bool HasLoadingFailed;
    public bool ShouldLog;
    public ConcurrentQueue<FlyTextLog> Logs = new();

    private readonly delegate* unmanaged<long, long> getTargetIdDelegate; // BattleChara_vf84 in 6.2

    private int limiter;
    private int? val1Preview;

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

    private delegate void* AddToScreenLogDelegate(long targetId, FlyTextCreation* flyTextCreation);
    private readonly Hook<AddToScreenLogDelegate>? addToScreenLogHook;

    public FlyTextHandler()
    {
        IntPtr getTargetIdAddress;
        IntPtr addToScreenLogWithScreenLogKindAddress;
        IntPtr addToScreenLogAddress;

        try
        {
            getTargetIdAddress = Service.SigScanner.ScanText("48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 8D 81 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 89 5C 24 ?? 48 89 74 24");
            addToScreenLogWithScreenLogKindAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 3A");
            addToScreenLogAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 48 33 CC E8 ?? ?? ?? ?? 48 83 C4 68 41 5F 41 5E");
        }
        catch (Exception ex)
        {
            this.HasLoadingFailed = true;
            PluginLog.Error(ex, "addToScreenLog sig scan failed.");
            return;
        }

        Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;
        Service.Framework.Update += this.Update;

        this.getTargetIdDelegate = (delegate* unmanaged<long, long>)getTargetIdAddress;

        this.addToScreenLogWithScreenLogKindHook = Hook<AddToScreenLogWithScreenLogKindDelegate>.FromAddress(addToScreenLogWithScreenLogKindAddress, this.AddToScreenLogWithScreenLogKindDetour);
        this.addToScreenLogWithScreenLogKindHook.Enable();

        this.addToScreenLogHook = Hook<AddToScreenLogDelegate>.FromAddress(addToScreenLogAddress, this.AddToScreenLogDetour);
        this.addToScreenLogHook.Enable();
    }

    public static (Vector2 healingGroupPos, Vector2 statusDamageGroupPos) GetDefaultPositions()
    {
        var (width, height) = Util.GetScreenSize();

        return (new Vector2(width * (49.0f / 100.0f), height / 2.0f), new Vector2(width * (11.0f / 20.0f), height / 2.0f));
    }

    public static void ResetPositions()
    {
        var (healingGroupPos, statusDamageGroupPos) = GetDefaultPositions();
        var addon = Service.GameGui.GetAddonByName("_FlyText", 1);
        if (addon == IntPtr.Zero)
        {
            return;
        }

        var flyTextArray = (FlyTextArray*)(addon + 0x2710); // AddonFlyText_Initialize

        (*flyTextArray)[0]->X = healingGroupPos.X;
        (*flyTextArray)[0]->Y = healingGroupPos.Y;

        (*flyTextArray)[1]->X = statusDamageGroupPos.X;
        (*flyTextArray)[1]->Y = statusDamageGroupPos.Y;
    }

    public static void SetPositions()
    {
        var addon = Service.GameGui.GetAddonByName("_FlyText", 1);
        if (addon == IntPtr.Zero)
        {
            return;
        }

        var flyTextArray = (FlyTextArray*)(addon + 0x2710); // AddonFlyText_Initialize
        var posConfig = Service.Configuration.FlyTextAdjustments.FlyTextPositions;

        if (posConfig.HealingGroupX != null)
        {
            (*flyTextArray)[0]->X = posConfig.HealingGroupX.Value;
        }

        if (posConfig.HealingGroupY != null)
        {
            (*flyTextArray)[0]->Y = posConfig.HealingGroupY.Value;
        }

        if (posConfig.StatusDamageGroupX != null)
        {
            (*flyTextArray)[1]->X = posConfig.StatusDamageGroupX.Value;
        }

        if (posConfig.StatusDamageGroupY != null)
        {
            (*flyTextArray)[1]->Y = posConfig.StatusDamageGroupY.Value;
        }
    }

    public static void ResetScaling()
    {
        var agent = (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ScreenLog);
        if (agent == IntPtr.Zero)
        {
            return;
        }

        *(float*)(agent + 0x4C) = 1.0f; // scale fly text
        *(float*)(agent + 0x344) = 1.0f; // scale pop-up text
    }

    public static void SetScaling(IntPtr? agent = null)
    {
        if (agent == null || agent == IntPtr.Zero)
        {
            agent = (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ScreenLog);
            if (agent == IntPtr.Zero)
            {
                return;
            }
        }

        var adjustmentsConfig = Service.Configuration.FlyTextAdjustments;

        if (adjustmentsConfig.FlyTextScale != null)
        {
            *(float*)(agent + 0x4C) = adjustmentsConfig.FlyTextScale.Value; // scale fly text
        }

        if (adjustmentsConfig.PopupTextScale != null)
        {
            *(float*)(agent + 0x344) = adjustmentsConfig.PopupTextScale.Value; // scale pop-up text
        }
    }

    public void CreateFlyText(FlyTextKind flyTextKind, byte sourceStyle, byte targetStyle)
    {
        var localPlayer = Service.ClientState.LocalPlayer?.Address.ToInt64();
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
                Val3 = 0,
            };

            this.val1Preview = flyTextCreation.Val1;

            this.addToScreenLogHook?.Original(targetId, &flyTextCreation);
        }
    }

    public void CreateFlyText(FlyTextLog flyTextLog)
    {
        var localPlayer = Service.ClientState.LocalPlayer?.Address.ToInt64();
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
        Service.Framework.Update -= this.Update;
        Service.FlyTextGui.FlyTextCreated -= this.FlyTextCreate;
        this.addToScreenLogHook?.Dispose();
        this.addToScreenLogWithScreenLogKindHook?.Dispose();
        ResetPositions();
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
        var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;
        if (character == null || localPlayer == null)
        {
            return FlyTextCharCategory.None;
        }

        if (character == localPlayer)
        {
            return FlyTextCharCategory.You;
        }

        if (Util.IsPartyMember(character))
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

    private void Update(Dalamud.Game.Framework framework)
    {
        try
        {
            if (this.limiter-- <= 0)
            {
                SetPositions();
                SetScaling();
                this.limiter = (int)ImGui.GetIO().Framerate * 3;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error in Update");
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
            var adjustedSource = source;
            if ((Service.Configuration.ShouldAdjustDotSource && flyTextKind == FlyTextKind.AutoAttack && option == 0 && actionKind == 0 && target == source)
                || (Service.Configuration.ShouldAdjustPetSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Pet && source->CompanionOwnerID == Service.ClientState.LocalPlayer?.ObjectId)
                || (Service.Configuration.ShouldAdjustChocoboSource && source->GameObject.SubKind == (int)BattleNpcSubKind.Chocobo && source->CompanionOwnerID == Service.ClientState.LocalPlayer?.ObjectId))
            {
                adjustedSource = (Character*)Service.ClientState.LocalPlayer?.Address;
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
                this.addToScreenLogWithScreenLogKindHook!.Original(target, source, flyTextKind, (byte)(option + (shouldFilter ? 150 : 100)), actionKind, actionId, val1, val2, val3);
                return;
            }

            if (shouldFilter)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception in AddScreenLogDetour");
        }

        this.addToScreenLogWithScreenLogKindHook!.Original(target, source, flyTextKind, option, actionKind, actionId, val1, val2, val3);
    }

    private void* AddToScreenLogDetour(long targetId, FlyTextCreation* flyTextCreation)
    {
        try
        {
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
                var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;
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
