using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MikuASM;

namespace DSCEdit
{
    static class InteractivePrompt
    {
        public static void RunInteractivePrompt()
        {
            Console.WriteLine("Interactive console, #exit to exit, ?(COMMAND) for reference on command.");
            string input = "";
            var prepro = new Compiler();
            prepro.Chatty = Environment.GetCommandLineArgs().Contains("-v");
            while (true)
            {
                Console.Write("> ");
                input = AutocompleteInput();
                if (input == null || input.Equals("#exit")) break;
                if (input.Length > 0 && input[0] == '?')
                {
                    string cmd = Translator.ExtractCommandName(input.Substring(1));
                    if (cmd == null) Console.WriteLine("Unknown command");
                    else Translator.ShowCommand(cmd);
                    continue;
                }

                var rslt = prepro.ProcessScript(input, "");
                if (rslt != null)
                {
                    foreach (var cmd in rslt)
                    {
                        Console.WriteLine("OUT: {0}", cmd);
                    }

                    if(DebugBridge.IsConnected)
                    {
                        DebugBridge.SendScript(prepro.DumpToByteArray());
                    }
                }

                Console.WriteLine();
            }
        }
        private static void CursorBack()
        {
            if (Console.CursorLeft == 0)
            {
                Console.CursorLeft = Console.WindowWidth - 1;
                Console.CursorTop--;
            }
            else
            {
                Console.CursorLeft--;
            }
        }

        private static void CursorForward()
        {
            if (Console.CursorLeft == Console.WindowWidth - 1)
            {
                Console.CursorLeft = 0;
                Console.CursorTop++;
            }
            else
            {
                Console.CursorLeft++;
            }
        }

        private static List<string> history = new List<string>();

        private static string AutocompleteInput()
        {
            StringBuilder sb = new StringBuilder();
            int curInputPos = 0;
            int historyPtr = history.Count;
            string prehistoricInputBuffer = null;
            bool isShowingHints = false;
            while (true)
            {
                var cki = Console.ReadKey(true);
                if (isShowingHints)
                {
                    ClearConsoleBelow();
                    isShowingHints = false;
                }
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.CursorVisible = false;
                    for (int i = curInputPos; i < sb.Length; i++) CursorForward();
                    Console.WriteLine();
                    Console.CursorVisible = true;
                    break;
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Remove(curInputPos - 1, 1);
                        curInputPos -= 1;
                        CursorBack();
                        var temp = Console.CursorLeft;
                        var temp2 = Console.CursorTop;
                        Console.Write(sb.ToString().Substring(curInputPos));
                        Console.Write(" ");
                        Console.CursorLeft = temp;
                        Console.CursorTop = temp2;
                    }
                    continue;
                }
                else if (cki.Key == ConsoleKey.Tab)
                {
                    var tempLeft = Console.CursorLeft;
                    var tempTop = Console.CursorTop;

                    string input = sb.ToString();
                    if (input.Length > 0)
                    {
                        ClearConsoleBelow();
                        Console.WriteLine();
                        Translator.ShowCommands(Translator.ExtractFirstWord(input));
                        isShowingHints = true;
                    }
                    Console.CursorLeft = tempLeft;
                    Console.CursorTop = tempTop;
                }
                else if (cki.Key == ConsoleKey.LeftArrow)
                {
                    if (curInputPos > 0)
                    {
                        CursorBack();
                        curInputPos--;
                    }
                }
                else if (cki.Key == ConsoleKey.RightArrow)
                {
                    if (curInputPos < sb.Length)
                    {
                        CursorForward();
                        curInputPos++;
                    }
                }
                else if (cki.Key == ConsoleKey.UpArrow)
                {
                    if (historyPtr == 0) continue;
                    historyPtr--;
                    for (int i = curInputPos; i > 0; i--) CursorBack();
                    for (int i = sb.Length; i > 0; i--) Console.Write(" ");
                    for (int i = sb.Length; i > 0; i--) CursorBack();
                    if (prehistoricInputBuffer == null) prehistoricInputBuffer = sb.ToString();
                    sb.Clear();
                    sb.Append(history[historyPtr]);
                    Console.Write(sb.ToString());
                    curInputPos = sb.Length;
                } else if (cki.Key == ConsoleKey.DownArrow)
                {
                    if (historyPtr == history.Count) continue;
                    historyPtr++;
                    for (int i = curInputPos; i > 0; i--) CursorBack();
                    for (int i = sb.Length; i > 0; i--) Console.Write(" ");
                    for (int i = sb.Length; i > 0; i--) CursorBack();
                    sb.Clear();
                    if(historyPtr < history.Count)
                    {
                        sb.Append(history[historyPtr]);
                    } else
                    {
                        sb.Append(prehistoricInputBuffer);
                        prehistoricInputBuffer = null;
                    }
                    Console.Write(sb.ToString());
                    curInputPos = sb.Length;
                }
                else if (Char.IsLetterOrDigit(cki.KeyChar) || Char.IsWhiteSpace(cki.KeyChar) || Char.IsPunctuation(cki.KeyChar) || Char.IsSymbol(cki.KeyChar))
                {
                    sb.Insert(curInputPos, cki.KeyChar);
                    Console.Write(cki.KeyChar);
                    curInputPos++;
                    if (curInputPos < sb.Length)
                    {
                        var temp = Console.CursorLeft;
                        var temp2 = Console.CursorTop;
                        Console.Write(sb.ToString().Substring(curInputPos));
                        Console.Write(" ");
                        Console.CursorLeft = temp;
                        Console.CursorTop = temp2;
                    }
                }
            }
            string rslt = sb.ToString();
            history.Add(rslt);
            return rslt;
        }

        static void ClearConsoleBelow()
        {
            var tempLeft = Console.CursorLeft;
            var tempTop = Console.CursorTop;
            for (int top = Console.CursorTop + 1; top < Console.WindowHeight - 1; top++)
            {
                Console.CursorTop = top;
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            }
            Console.CursorTop = tempTop;
            Console.CursorLeft = tempLeft;
        }

    }
}
