using MikuASM.Common.Locales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM
{
    public static class Translator
    {

        public const string COMMENT_MARKER = "//";
        public const string PREPROC_MARKER = "#";
        public static string[] LINE_COMMENTS = new string[] { COMMENT_MARKER, "--" };
        public enum LineKind
        {
            NoOp = 0,
            Command = 1,
            Preprocessor = 2
        }

        private static Dictionary<string, Type> shortcutRegistry = new Dictionary<string, Type>();
        private static Dictionary<string, Type> commandNameRegistry = new Dictionary<string, Type>();
        public static void ShowCommands(string prefix = "")
        {
            BuildCache();
            if (prefix == null) return;
            foreach(KeyValuePair<string, Type> kv in commandNameRegistry)
            {
                if(kv.Key.StartsWith(prefix))
                {
                    ShowCommand(kv.Key);
                    Console.WriteLine("--------------------");
                }
            }
        }

        public static string[] GetKeywords()
        {
            BuildCache();

            return commandNameRegistry.Keys.ToArray();
        }

        public static string CommandNameWithArgs(Type cType, string separator = "\t")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DSCCommand.GetMnemonicForType(cType));
            var args = DSCCommand.GetArgNamesForType(cType);
            if (args.Count > 0)
            {
                sb.Append(separator);
                for (int i = 0; i < args.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(args[i].ToUpper());
                }
            }
            return sb.ToString();
        }

        public static void ShowCommand(string commandName)
        {
            BuildCache();

            if(!commandNameRegistry.ContainsKey(commandName))
            {
                Console.WriteLine(Strings.ErrCmdNotFound, commandName);
                return;
            }
            var cT = commandNameRegistry[commandName];
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNameWithArgs(cT));
            sb.Append("\n");
            sb.Append(DSCCommand.GetHelpForType(cT));
            Console.WriteLine(sb.ToString());
        }

        private static bool hasCache = false;
        private static void BuildCache()
        {
            if (hasCache) return;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(CmdMnemonic), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        // add to a cache.
                        var attr = (CmdMnemonic)attribs.First();
                        commandNameRegistry.Add(attr.Mnemonic, type);

                        var shcs = type.GetCustomAttributes(typeof(CmdShortcut), false);
                        if(shcs != null && shcs.Length > 0)
                        {
                            var shc = (CmdShortcut)shcs.First();
                            shortcutRegistry.Add(shc.Mnemonic, type);
                        }
                    }
                }
            }
            hasCache = true;
        }

        public static bool IsKeyword(string keyword)
        {
            BuildCache();
            return (shortcutRegistry.ContainsKey(keyword) || commandNameRegistry.ContainsKey(keyword));
        }

        public static LineKind ClassifyLine(string line)
        {
            string tmp = line.Trim().ToUpperInvariant();

            if (tmp.Length < 1) return LineKind.NoOp;
            if (tmp.StartsWith(COMMENT_MARKER)) return LineKind.NoOp;
            if (tmp.StartsWith(PREPROC_MARKER)) return LineKind.Preprocessor;
            return LineKind.Command;
        }

        public static string ExtractFirstWord(string line)
        {
            string tmp = line.Trim().ToUpperInvariant();
            string firstWord = tmp.Split(' ').First();
            return firstWord;
        }
        public static Type ExtractCommand(string line)
        {
            BuildCache();

            string firstWord = ExtractFirstWord(line);
            string firstChar = firstWord.Substring(0, 1);
            if (commandNameRegistry.ContainsKey(firstWord)) return commandNameRegistry[firstWord];
            else if (shortcutRegistry.ContainsKey(firstChar))
            {
                var type = shortcutRegistry[firstChar];
                return type;
            }
            return null;
        }
        public static string ExtractCommandName(string line, bool allowAll = false)
        {
            var cmd = ExtractCommand(line);
            if (cmd != null) return DSCCommand.GetMnemonicForType(cmd);
            return null;
        }

        public static string[] ExtractArgs(string line)
        {
            string tmp = line.Trim().ToUpperInvariant();
            string firstWord = tmp.Split(' ').First();
            if (tmp.Length == firstWord.Length) return new string[0];
            string[] argList = tmp.Substring(firstWord.Length + 1).Split(',').Select(x => x.Trim()).ToArray();
            return argList;
        }

        public static DSCCommand ParseCommandLine(LocatedString locLine)
        {
            if (locLine == null) return null;
            string line = locLine.Value;
            if (line == null || ClassifyLine(line) != LineKind.Command) return null;
            Type cType = ExtractCommand(line);
            if (cType == null)
            {
                throw new UnknownCommand(locLine);
            }

            string[] args = ExtractArgs(line);

            if (args.Length != DSCCommand.GetArgCountForType(cType))
            {
                throw new ArgumentCountMismatch(locLine, CommandNameWithArgs(cType) + "\n" + DSCCommand.GetHelpForType(cType));
            }

            var cmd = (DSCCommand)Activator.CreateInstance(cType);
            for (uint i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                cmd.SetArgByNo(i, arg);
            }

            return cmd;
        }

        public static string GetLineError(string line)
        {
            if (line == null) return null;
            if (line == null || ClassifyLine(line) != LineKind.Command) return null;
            Type cType = ExtractCommand(line);
            if (cType == null)
            {
                return Strings.ErrUnknownCmd;
            }

            string[] args = ExtractArgs(line);

            if (args.Length != DSCCommand.GetArgCountForType(cType))
            {
                return String.Format(Strings.ErrArgCountLen, DSCCommand.GetArgCountForType(cType), args.Length);
            }

            return null;
        }
    }
}
