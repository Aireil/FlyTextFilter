using System;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace FlyTextFilter
{
    public unsafe class FlyTextHandler
    {
        public FlyTextKind LatestFlyText;

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

        public void Dispose()
        {
            Service.FlyTextGui.FlyTextCreated -= FlyTextCreate;
            this.addScreenLogHook.Dispose();
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
