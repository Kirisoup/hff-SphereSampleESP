using System;
using System.Reflection;
using System.Collections.Generic;

static class Helper {

        public static void RegCmd(string cmd, Action action, string help = null) => RegCmd(cmd, null, action, help);

        public static void RegCmd(string cmd, Action<string> action, string help = null) => RegCmd(cmd, null, action, help);

        public static void RegCmd(string cmd, string abbr, Action action, string help = null) {

                help = HelpCreate(cmd, abbr, help);

                Shell.RegisterCommand(cmd, action, help);

                if (!string.IsNullOrEmpty(abbr)) Shell.RegisterCommand(abbr, action, null);
        }

        public static void RegCmd(string cmd, string abbr, Action<string> action, string help = null) {

                help = HelpCreate(cmd, abbr, help);

                Shell.RegisterCommand(cmd, action, help);

                if (!string.IsNullOrEmpty(abbr)) Shell.RegisterCommand(abbr, action, null);
        }

        public static string GetHelp(string cmd) => description["cmd"];

        static string HelpCreate(string cmd, string abbr, string help, string val = null) => string.IsNullOrEmpty(help) ? null : HelpPattern(cmd, abbr, help, val);
        static string HelpPattern(string cmd, string abbr, string help, string val = null) => cmd + (string.IsNullOrEmpty(abbr) ? null : $"({abbr})") + (string.IsNullOrEmpty(val) ? null : " " + val) + " - " + help;

        static readonly FieldInfo commandsF = typeof(Shell).GetField("commands", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly CommandRegistry CommandRegistry = (CommandRegistry)commandsF.GetValue(null);

        static readonly FieldInfo descriptionF = typeof(CommandRegistry).GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly Dictionary<string, string> description = (Dictionary<string, string>)descriptionF.GetValue(CommandRegistry);

}
