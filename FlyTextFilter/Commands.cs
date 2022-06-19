using System;
using Dalamud.Game.Command;

namespace FlyTextFilter
{
    public class Commands
    {
        private const string CommandName = "/flytext";
        private const string CommandNameAlias = "/ftf";

        public Commands()
        {
            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open config",
                ShowInHelp = true,
            });

            Service.CommandManager.AddHandler(CommandNameAlias, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open config",
                ShowInHelp = true,
            });
        }

        public static void Dispose()
        {
            Service.CommandManager.RemoveHandler(CommandName);
            Service.CommandManager.RemoveHandler(CommandNameAlias);
        }

        private static void OnCommand(string command, string args)
        {
            if (args.Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                FlyTextKindTests.RunTests();
                return;
            }

            if (args.Equals("testData", StringComparison.OrdinalIgnoreCase))
            {
                FlyTextKindTests.PrintData();
                return;
            }

            Service.ConfigWindow.Toggle();
        }
    }
}
