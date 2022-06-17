using System;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FlyTextFilter.Model;

namespace FlyTextFilter
{
    public unsafe class FlyTextHandler
    {
        public FlyTextKind LatestFlyText;

        private delegate long AddonFlyTextOnRefreshDelegate(IntPtr addon, void* a2, void* a3);
        private readonly Hook<AddonFlyTextOnRefreshDelegate> addonFlyTextOnRefreshHook;

        private delegate void AddScreenLogDelegate(
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
        private readonly Hook<AddScreenLogDelegate> addScreenLogHook;

        public FlyTextHandler()
        {
            Service.FlyTextGui.FlyTextCreated += FlyTextCreate;

            var addonFlyTextOnRefreshAddress = Service.SigScanner.ScanText("40 56 48 81 EC ?? ?? ?? ?? 48 8B F1 85 D2");
            this.addonFlyTextOnRefreshHook =
                new Hook<AddonFlyTextOnRefreshDelegate>(addonFlyTextOnRefreshAddress, this.AddonFlyTextOnRefreshDetour);
            this.addonFlyTextOnRefreshHook.Enable();

            var addScreenLogAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 3A");
            this.addScreenLogHook = new Hook<AddScreenLogDelegate>(addScreenLogAddress, this.AddScreenLogDetour);
            this.addScreenLogHook.Enable();
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
            if (Service.Configuration.IsLoggingEnabled)
            {
                PluginLog.Information($"" +
                                      $"Type: {kind.ToString()}" +
                                      $" - Value1: {val1}" +
                                      $" - Value2 : {val2}" +
                                      $" - Text1: {text1}" +
                                      $" - Text2: {text2}");
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

        public void Dispose()
        {
            Service.FlyTextGui.FlyTextCreated -= FlyTextCreate;
            this.addScreenLogHook.Dispose();
            this.addonFlyTextOnRefreshHook.Dispose();
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

        private void AddScreenLogDetour(
            Character* target,
            Character* source,
            FlyTextKind kind,
            int option,
            int actionKind,
            int actionId,
            int val1,
            int val2,
            int val3,
            int val4)
        {
            try
            {
                var localPlayer = Service.ClientState.LocalPlayer?.Address.ToInt64();
                if (localPlayer is not null)
                {
                    var isOnLocalPlayer = localPlayer == (long)target;
                    var isFromLocalPlayer = localPlayer == (long)source;

                    if (Service.Configuration.IsLoggingEnabled)
                    {
                        PluginLog.Information($"" +
                                              $"Type: {kind.ToString()}" +
                                              $" - Value1: {val1}" +
                                              $" - Value2 : {val2}" +
                                              $" - Value3: {val3}" +
                                              $" - Value4: {val4}" +
                                              $" - From: {(isFromLocalPlayer ? "You" : "Others")}" +
                                              $" - On: {(isOnLocalPlayer ? "You" : "Others")}");
                    }

                    if (isFromLocalPlayer && Service.Configuration.HideFlyTextKindPlayer.Contains(kind))
                    {
                        return;
                    }

                    if (!isFromLocalPlayer && Service.Configuration.HideFlyTextKindOthers.Contains(kind))
                    {
                        return;
                    }

                    if (isOnLocalPlayer && Service.Configuration.HideFlyTextKindOnPlayer.Contains(kind))
                    {
                        return;
                    }

                    if (!isOnLocalPlayer && Service.Configuration.HideFlyTextKindOnOthers.Contains(kind))
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Exception in AddScreenLogDetour");
            }

            this.addScreenLogHook.Original(target, source, kind, option, actionKind, actionId, val1, val2, val3, val4);
        }
    }
}
