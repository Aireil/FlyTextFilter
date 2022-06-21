using System;
using System.Collections.Concurrent;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FlyTextFilter.Model;

namespace FlyTextFilter;

public unsafe class FlyTextHandler
{
    public bool ShouldLog;
    public ConcurrentQueue<FlyTextLog> Logs = new();
    public FlyTextLog? IgnoreLog;

    private delegate long AddonFlyTextOnRefreshDelegate(IntPtr addon, void* a2, void* a3);
    private readonly Hook<AddonFlyTextOnRefreshDelegate> addonFlyTextOnRefreshHook;

    private delegate void AddToScreenLogWithScreenLogKindDelegate(
        Character* target,
        Character* source,
        FlyTextKind logKind,
        int option,
        int actionKind,
        int actionId,
        int val1,
        int val2,
        int val3,
        int val4);
    private readonly Hook<AddToScreenLogWithScreenLogKindDelegate> addToScreenLogWithScreenLogKindHook;

    private delegate void AddToScreenLogItemDelegate(uint itemId, int count);
    private readonly Hook<AddToScreenLogItemDelegate> addToScreenLogItemHook;

    private delegate void AddToScreenLogCraftingDelegate(Character* source, FlyTextKind flyTextKind, int val);
    private readonly Hook<AddToScreenLogCraftingDelegate> addToScreenLogCraftingHook;

    public FlyTextHandler()
    {
        Service.FlyTextGui.FlyTextCreated += FlyTextCreate;

        var addonFlyTextOnRefreshAddress = Service.SigScanner.ScanText("40 56 48 81 EC ?? ?? ?? ?? 48 8B F1 85 D2");
        this.addonFlyTextOnRefreshHook =
            new Hook<AddonFlyTextOnRefreshDelegate>(addonFlyTextOnRefreshAddress, this.AddonFlyTextOnRefreshDetour);
        this.addonFlyTextOnRefreshHook.Enable();

        var addToScreenLogWithScreenLogKindAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 3A");
        this.addToScreenLogWithScreenLogKindHook = new Hook<AddToScreenLogWithScreenLogKindDelegate>(addToScreenLogWithScreenLogKindAddress, this.AddToScreenLogWithScreenLogKindDetour);
        this.addToScreenLogWithScreenLogKindHook.Enable();

        var addToScreenLogItemAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 5C 24 ?? 66 83 7C 24");
        this.addToScreenLogItemHook = new Hook<AddToScreenLogItemDelegate>(addToScreenLogItemAddress, this.AddToScreenLogItemDetour);
        this.addToScreenLogItemHook.Enable();

        var addToScreenLogCraftingAddress = Service.SigScanner.ScanText("48 85 C9 74 4D 53");
        this.addToScreenLogCraftingHook = new Hook<AddToScreenLogCraftingDelegate>(addToScreenLogCraftingAddress, this.AddToScreenLogCraftingDetour);
        this.addToScreenLogCraftingHook.Enable();
    }

    public static void FlyTextCreate(
        ref FlyTextKind kind,
        ref int val1,
        ref int val2,
        ref SeString text1,
        ref SeString text2,
        ref uint color,
        ref uint icon,
        ref float yOffset,
        ref bool handled)
    {
        // preview
        if (icon == 22601 && val1 == 1111 && val2 == 2222)
        {
            return;
        }

        // status effects
        if (text1.TextValue.StartsWith("+ ") || text1.TextValue.StartsWith("- "))
        {
            if (Service.Configuration.Blacklist.Contains(text1.TextValue[2..])
                || Service.Configuration.Blacklist.Contains(text1.TextValue[2..]))
            {
                handled = true;
            }
        }

        if (Service.Configuration.Blacklist.Contains(text1.TextValue)
            || Service.Configuration.Blacklist.Contains(text2.TextValue))
        {
            handled = true;
        }
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
        if (addon == IntPtr.Zero) return;

        var flyTextArray = (FlyTextArray*)(addon + 0x26C8);

        (*flyTextArray)[0]->X = healingGroupPos.X;
        (*flyTextArray)[0]->Y = healingGroupPos.Y;

        (*flyTextArray)[1]->X = statusDamageGroupPos.X;
        (*flyTextArray)[1]->Y = statusDamageGroupPos.Y;
    }

    public static void SetPositions(IntPtr? addon = null)
    {
        if (addon == null)
        {
            addon = Service.GameGui.GetAddonByName("_FlyText", 1);
            if (addon == IntPtr.Zero) return;
        }

        var flyTextArray = (FlyTextArray*)(addon + 0x26C8);
        var posConfig = Service.Configuration.FlyTextPositions;

        if (posConfig.HealingGroupX != null)
            (*flyTextArray)[0]->X = posConfig.HealingGroupX.Value;
        if (posConfig.HealingGroupY != null)
            (*flyTextArray)[0]->Y = posConfig.HealingGroupY.Value;

        if (posConfig.StatusDamageGroupX != null)
            (*flyTextArray)[1]->X = posConfig.StatusDamageGroupX.Value;
        if (posConfig.StatusDamageGroupY != null)
            (*flyTextArray)[1]->Y = posConfig.StatusDamageGroupY.Value;
    }

    public void CreateFlyText(FlyTextKind flyTextKind)
    {
        if (flyTextKind == FlyTextKind.NamedIconWithItemOutline)
        {
            this.AddToScreenLogItemDetour(5442, 4);
            return;
        }

        var localPlayer = Service.ClientState.LocalPlayer?.Address.ToInt64();
        if (localPlayer != null)
        {
            this.AddToScreenLogWithScreenLogKindDetour(
                (Character*)(long)localPlayer,
                (Character*)(long)localPlayer,
                flyTextKind,
                12500, // to discriminate during the filtering
                1,
                2555,
                1111,
                2222,
                3333,
                4444);
        }
    }

    public void CreateFlyText(FlyTextLog flyTextLog)
    {
        var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;

        this.IgnoreLog = flyTextLog;
        switch (flyTextLog.FlyTextCreationSource)
        {
            case FlyTextCreationSource.AddToScreenLogWithScreenLogKind:
                this.AddToScreenLogWithScreenLogKindDetour(
                    localPlayer,
                    localPlayer,
                    flyTextLog.FlyTextKind,
                    flyTextLog.Option,
                    flyTextLog.ActionKind,
                    flyTextLog.ActionId,
                    flyTextLog.Val1,
                    flyTextLog.Val2,
                    flyTextLog.Val3,
                    flyTextLog.Val4);
                break;
            case FlyTextCreationSource.AddToScreenLogItem:
                this.AddToScreenLogItemDetour(flyTextLog.ItemId, flyTextLog.Count);
                break;
            case FlyTextCreationSource.AddToScreenLogCrafting:
                this.AddToScreenLogCraftingDetour(localPlayer, flyTextLog.FlyTextKind, flyTextLog.Val);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(flyTextLog), "Tried creating a FlyTextLog from an unknown source");
        }
    }

    public void Dispose()
    {
        Service.FlyTextGui.FlyTextCreated -= FlyTextCreate;
        this.addToScreenLogCraftingHook.Dispose();
        this.addToScreenLogItemHook.Dispose();
        this.addToScreenLogWithScreenLogKindHook.Dispose();
        this.addonFlyTextOnRefreshHook.Dispose();
        ResetPositions();
    }

    private static bool ShouldFilter(Character* source, Character* target, FlyTextKind flyTextKind)
    {
        if (Service.Configuration.FlyTextSettings.TryGetValue(flyTextKind, out var flyTextSetting))
        {
            switch (GetFlyTextCharCategory(source))
            {
                case FlyTextCharCategory.You:
                    return ShouldFilter(target, flyTextSetting.SourceYou);
                case FlyTextCharCategory.Party:
                    return ShouldFilter(target, flyTextSetting.SourceParty);
                case FlyTextCharCategory.Others:
                default:
                    return ShouldFilter(target, flyTextSetting.SourceOthers);
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
            default:
                return flyTextTargets.HasFlag(FlyTextTargets.Others);
        }
    }

    private static FlyTextCharCategory GetFlyTextCharCategory(Character* character)
    {
        var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;
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

    private long AddonFlyTextOnRefreshDetour(IntPtr addon, void* a2, void* a3)
    {
        try
        {
            SetPositions(addon);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception in AddonFlyTextOnRefreshDetour");
        }

        return this.addonFlyTextOnRefreshHook.Original(addon, a2, a3);
    }

    private void AddToScreenLogWithScreenLogKindDetour(
        Character* target,
        Character* source,
        FlyTextKind flyTextKind,
        int option, // 0 = DoT? / 2 = blocked / 3 = parried / 4 = resisted / 5 = default?
        int actionKind,
        int actionId,
        int val1,
        int val2,
        int val3,
        int val4)
    {
        try
        {
            // preview
            if (option == 12500)
            {
                this.addToScreenLogWithScreenLogKindHook.Original(target, source, flyTextKind, 5, actionKind, actionId, val1, val2, val3, val4);
                return;
            }

            var adjustedSource = source;
            if ((Service.Configuration.ShouldAdjustDotSource && flyTextKind == FlyTextKind.AutoAttack && option == 0 && actionKind == 0 && target == source)
                || (Service.Configuration.ShouldAdjustPetSource && source->CompanionOwnerID == Service.ClientState.LocalPlayer?.ObjectId))
            {
                adjustedSource = (Character*)Service.ClientState.LocalPlayer?.Address;
            }

            var shouldFilter = ShouldFilter(adjustedSource, target, flyTextKind);

            if (this.ShouldLog)
            {
                var flyTextLog = new FlyTextLog
                {
                    FlyTextCreationSource = FlyTextCreationSource.AddToScreenLogWithScreenLogKind,
                    FlyTextKind = flyTextKind,
                    SourceCategory = GetFlyTextCharCategory(adjustedSource),
                    TargetCategory = GetFlyTextCharCategory(target),
                    Option = option,
                    ActionKind = actionKind,
                    ActionId = actionId,
                    Val1 = val1,
                    Val2 = val2,
                    Val3 = val3,
                    Val4 = val4,
                    HasSourceBeenAdjusted = source != adjustedSource,
                };

                if (flyTextLog.Equals(this.IgnoreLog))
                {
                    this.IgnoreLog = null;
                    shouldFilter = false;
                }
                else
                {
                    this.AddLog(flyTextLog);
                }
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

        this.addToScreenLogWithScreenLogKindHook.Original(target, source, flyTextKind, option, actionKind, actionId, val1, val2, val3, val4);
    }

    private void AddToScreenLogItemDetour(uint itemId, int count)
    {
        try
        {
            var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;
            var shouldFilter = ShouldFilter(localPlayer, localPlayer, FlyTextKind.NamedIconWithItemOutline);

            if (this.ShouldLog)
            {
                var flyTextLog = new FlyTextLog
                {
                    FlyTextCreationSource = FlyTextCreationSource.AddToScreenLogItem,
                    FlyTextKind = FlyTextKind.NamedIconWithItemOutline,
                    SourceCategory = FlyTextCharCategory.You,
                    TargetCategory = FlyTextCharCategory.You,
                    ItemId = itemId,
                    Count = count,
                };

                if (flyTextLog.Equals(this.IgnoreLog))
                {
                    this.IgnoreLog = null;
                    shouldFilter = false;
                }
                else
                {
                    this.AddLog(flyTextLog);
                }
            }

            if (shouldFilter)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception in AddToScreenLogItemDetour");
        }

        this.addToScreenLogItemHook.Original(itemId, count);
    }

    private void AddToScreenLogCraftingDetour(Character* source, FlyTextKind flyTextKind, int val)
    {
        try
        {
            var localPlayer = (Character*)Service.ClientState.LocalPlayer?.Address;
            var shouldFilter = ShouldFilter(source, localPlayer, flyTextKind);

            if (this.ShouldLog)
            {
                var flyTextLog = new FlyTextLog
                {
                    FlyTextCreationSource = FlyTextCreationSource.AddToScreenLogCrafting,
                    FlyTextKind = flyTextKind,
                    SourceCategory = GetFlyTextCharCategory(source),
                    TargetCategory = FlyTextCharCategory.You,
                    Val = val,
                };

                if (flyTextLog.Equals(this.IgnoreLog))
                {
                    this.IgnoreLog = null;
                    shouldFilter = false;
                }
                else
                {
                    this.AddLog(flyTextLog);
                }
            }

            if (shouldFilter)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Exception in AddToScreenLogCraftingDetour");
        }

        this.addToScreenLogCraftingHook.Original(source, flyTextKind, val);
    }

    private void AddLog(FlyTextLog flyTextLog)
    {
        this.Logs.Enqueue(flyTextLog);

        while (this.Logs.Count > Service.Configuration.NbOfLogs)
        {
            this.Logs.TryDequeue(out _);
        }
    }
}
