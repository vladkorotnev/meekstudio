using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using MikuASM;

namespace DSCEdit
{
    
    class Program
    {
        static void ShowUsage()
        {
            Console.WriteLine("Supported arguments:\n" +
                "--nologo: Suppress the about text input\n" +
                "--dict: Output a list of supported commands and exit\n" +
                "--decompile <file>: convert a DSC file into a text file (beta)\n" +
                "-v: verbose logging of internal routines\n" +
                "--interactive: run an interactive console prompt (for syntax check/reference)\n" +
                "--attach [processname.exe]: (only for interactive) inject a debug server into a process\n" +
                "--reattach: (only for interactive) send data to a debug server without injecting\n" +
                "<file>: compile a file (when no interactive args are specified)");
        }
        static void Main(string[] args)
        {
            if(!args.Contains("--nologo"))
            {
                Console.WriteLine("MikuASM {0} by akasaka, 2020\n" +
                    "Based upon research by samyuu, nastys, Waelwindows, korenkonder, Raki, and the other fine folks on discord\n" +
                    "-------------------------------------------------",
                    Assembly.GetExecutingAssembly().GetName().Version);
            }

            if(args.Contains("--help") || args.Contains("/?") || args.Contains("-h"))
            {
                ShowUsage();
                return;
            }

            if(args.Contains("--interactive"))
            {
                if(args.Contains("--reattach"))
                {
                    Console.WriteLine("Debug data sending enabled");
                    DebugBridge.Reattach();
                } else if (args.Contains("--attach"))
                {
                    string attachName = "diva.exe";
                    try
                    {
                        var attachTarget = args[Array.IndexOf(args, "--attach") + 1];
                        if (!attachTarget.StartsWith("-"))
                        {
                            attachName = attachTarget;
                        }
                    }
                    catch (Exception e) { }

                    Console.WriteLine("Attaching to process {0}...", attachName);
                    DebugBridge.Inject(attachName);
                }
                InteractivePrompt.RunInteractivePrompt(); 
                return;
            }

            if (args.Contains("--dict"))
            {
                Translator.ShowCommands(); Console.ReadLine();
                return;
            }

            var fileArgs = args.Where(x => !x.StartsWith("-")).ToArray();
            if (fileArgs.Length == 0)
            {
                Console.WriteLine("No input file specified.");
                ShowUsage();
            } 
            else if (args.Contains("--decompile"))
            {
                foreach (var file in fileArgs)
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine("FATAL: {0} not found", file);
                        return;
                    }

                    var fileContent = DSCReader.ReadFromFile(file);
                    foreach(var cmd in fileContent.Commands)
                    {
                        Console.WriteLine("{0}", cmd.Unwrap());
                    }
                }
            }
            else
            {
                var compiler = new Compiler();
                compiler.Chatty = args.Contains("-v");
                foreach(var file in fileArgs)
                {
                    if(!File.Exists(file))
                    {
                        Console.WriteLine("FATAL: {0} not found", file);
                        return;
                    }

                    var fileContent = File.ReadAllText(file);
                    compiler.ProcessScript(fileContent, file);
                }
            }
        }

        
    }
}
