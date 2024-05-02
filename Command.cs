using System;
using BepInEx;
using System.Linq;

namespace spsp
{
        public partial class Plugin {

                static class Command {

                        public static void Register() => Helper.RegCmd("spsp", new Action<string>(Interface));

                        public static void Interface(string input) => Interface(input.IsNullOrWhiteSpace() ? new string[1] { null } : SplitInput(input));

                        public static void Interface(string[] args) {

                                switch (args[0]) {

                                        case "on": case "1":
                                                CreateSphereSample(Human.Localplayer.transform.position, type: AnchorType.localplayer);
                                                break;


                                        case "off": case "0":
                                                ClearSphereSamples();
                                                break;

                                        default:
                                                BadSyntax(":(");
                                                return;
                                }
                        }

                        // static class Interfaces {

                        //         public static void Toggle(string arg) {
                                        
                        //                 switch (arg) {

                        //                         case null:
                        //                                 isEnabled = !isEnabled;
                        //                                 break;

                        //                         case "on": case "1":
                        //                                 if (isEnabled) return;
                        //                                 isEnabled = true;
                        //                                 break;

                        //                         case "off": case "0":
                        //                                 if (!isEnabled) return;
                        //                                 isEnabled = false;
                        //                                 break;

                        //                         default: 
                        //                                 BadSyntax("unknown argument");
                        //                                 return;
                        //                 }
                                        
                        //                 Shell.Print("uihax: " + (isEnabled ? "on" : "off"));
                        //         }

                        //         public static void SphereSample(string[] args) {
                                        
                        //                 switch (args[0]) {

                        //                         case "new": case "n":

                        //                                 switch (SafeSelArg(args, 1)) {

                        //                                         case null:
                        //                                                 CreateSphereSample(Human.Localplayer.transform.position);
                        //                                                 break;
                        //                                 }

                        //                                 break;

                        //                         default: 
                        //                                 BadSyntax("unknown argument");
                        //                                 return;
                        //                 }                                        
                        //         }

                        // }

                        static string[] SplitInput(string input) => input.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        static string SafeSelArg(string[] args, int index) => args.Length > index ? args[index] : null;

                        static string[] SafeSelArgs(string[] args, int index) => args.Length > index ? args.Skip(index).ToArray() : new string[1] { null };

                        static void BadSyntax(string error) => Shell.Print("Syntax error: " + error);

                        static bool BadSyntax(string error, bool condition) {
                                if (condition) Shell.Print("Syntax error: " + error + newline + Helper.GetHelp("uihax"));
                                return condition;
                        }

                        static readonly string newline = Environment.NewLine;
                }
        }
}