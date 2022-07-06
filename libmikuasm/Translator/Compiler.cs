using MikuASM.Common.Locales;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MikuASM
{
    public enum BinFltMode
    {
        NO_FILTER,
        ONLY_CHART,
        WITHOUT_CHART
    }
    public class LocatedString
    {
        public string Filename;
        public int LineNo;
        public string Value;
        public string ExpandedFrom;
        public LocatedString Expand(string expandedForm)
        {
            return new LocatedString { Filename = this.Filename, LineNo = this.LineNo, ExpandedFrom = this.Value, Value = expandedForm };
        }
    }

    internal struct ForLoopDefinition
    {
        public string VarName;
        public int From;
        public int To;
        public int Step;

        public int StepCount
        {
            get
            {
                return (To - From) / Step + 1;
            }
        }

        public bool IsFinite
        {
            get
            {
                return Math.Sign(To - From) == Math.Sign(Step);
            }
        }
    }

    internal class ForLooper
    {
        public ForLooper(ForLoopDefinition def)
        {
            Definition = def;
            Current = def.From - Definition.Step;
        }

        public ForLoopDefinition Definition { get; private set; }
        public int Current { get; private set; }
        public bool WillChange
        {
            get
            {
                return Current != Definition.To + Definition.Step;
            }
        }

        public bool DidChange
        {
            get
            {
                return (Current != Definition.From - Definition.Step) && WillChange;
            }
        }

        public int Advance()
        {
            Current += Definition.Step;
            return Current;
        }
    }

    public class CompilerFileEventArgs : EventArgs
    {
        public enum EventType
        {
            Emit,
            Consume
        }

        public EventType Type { get; private set; }
        public string FileName { get; private set; }

        internal CompilerFileEventArgs(EventType type, string file)
        {
            Type = type;
            FileName = file;
        }
    }

    public class Compiler
    {
        public bool Chatty { get; set; }
        public bool AllowRedefinitions { get; set; }

        public event EventHandler<CompilerFileEventArgs> onFileProcessed;

        private Dictionary<string, string> substitutions = new Dictionary<string, string>();
        private List<LocatedString> inputBuffer = new List<LocatedString>();

        private Stack<List<LocatedString>> forLoopBufferList = new Stack<List<LocatedString>>();
        private Stack<List<ForLoopDefinition>> forLoopContextList = new Stack<List<ForLoopDefinition>>();

        public List<DSCCommand> outputBuffer = new List<DSCCommand>();

        private List<List<DSCCommand>> contexts = new List<List<DSCCommand>>();
        private BinFltMode BinaryFilterMode = BinFltMode.NO_FILTER;
        private Nullable<int> BinaryFilterStartTime = null;
        private Nullable<int> BinaryFilterEndTime = null;
        

        private void ExportOutputBuffer(string savePath)
        {
            if (File.Exists(savePath)) File.Delete(savePath);
            DSCFile outFile = new DSCFile();
            outFile.Commands = outputBuffer.Select(x => x.Wrap()).ToList();
            if (Chatty) Console.Error.WriteLine(Strings.WriteBuf, savePath);
            DSCReader.WriteToFile(savePath, outFile);
            if(onFileProcessed != null)
             onFileProcessed.Invoke(this, new CompilerFileEventArgs(CompilerFileEventArgs.EventType.Emit, savePath));
        }

        public byte[] DumpToByteArray()
        {
            DSCFile outFile = new DSCFile();
            outFile.Commands = outputBuffer.Select(x => x.Wrap()).ToList();
            byte[] memBuf = null;
            using(MemoryStream ms = new MemoryStream())
            {
                DSCReader.WriteToStream(ms, outFile);
                ms.Seek(4, SeekOrigin.Begin); // skip magic 
                memBuf = new byte[ms.Length - 4];
                ms.Read(memBuf, 0, memBuf.Length);
            }
            outputBuffer.Clear();
            return memBuf;
        }

        public void PushCommand(DSCCommand cmd)
        {
            outputBuffer.Add(cmd);
        }

        static string ExtractCommandWord(string line)
        {
            if (line.Length < 1) return "";
            string tmp = line.Trim().ToUpperInvariant().Substring(1);
            string firstWord = tmp.Split(' ').First();
            return firstWord;
        }
        
        public static class PreprocessorDirectives
        {
            /// <summary>
            /// Include a source code file in-place
            /// </summary>
            public const string INCLUDE = "INCLUDE";
            /// <summary>
            /// Include a binary file in-place
            /// </summary>
            public const string INCLUDE_BINARY = "INCBIN";
            /// <summary>
            /// Set binary inclusion filtering mode
            /// </summary>
            public const string SET_BINARY_FILTER = "BINFLT";
            /// <summary>
            /// Enable verbose logging
            /// </summary>
            public const string SET_VERBOSE = "CHATTY";
            /// <summary>
            /// Clear the current output buffer
            /// </summary>
            public const string CLEAR_BUFFER = "CLEAR";
            /// <summary>
            /// Define a preprocessor constant
            /// </summary>
            public const string CONST_DEF = "CONST";
            /// <summary>
            /// Sort the current output buffer in time
            /// </summary>
            public const string SORT_FORCE = "SORT!";
            /// <summary>
            /// Begin a temporary context
            /// </summary>
            public const string PUSH_CONTEXT = "CTXSTART";
            /// <summary>
            /// Destroy a temporary context
            /// </summary>
            public const string POP_CONTEXT = "CTXEND";
            /// <summary>
            /// Save the current output buffer to a binary file
            /// </summary>
            public const string WRITE_FILE = "WRITE";
            /// <summary>
            /// Force an error
            /// </summary>
            public const string FORCE_STOP = "UNKO";
            /// <summary>
            /// Undefine a constant
            /// </summary>
            public const string CONST_UNDEF = "UNDEF";

            /// <summary>
            /// Set the binary inclusion time filter (2 int args, -1 for no filter)
            /// </summary>
            public const string BINFLT_SET_TIME = "BINTIME";

            /// <summary>
            /// Repeat the following code block with variables
            /// </summary>
            public const string FORLOOP_BEGIN = "FOR";
            public const string FORLOOP_END = "ENDFOR";

            /// <summary>
            /// Move the timeline by specified ms
            /// </summary>
            public const string SLIDETIME = "SLIDETIME";

        }

        string PathRelativeFromString(LocatedString initiator, string path)
        {
            string rslt = path;
            if(initiator.Filename != null && initiator.Filename.Length > 0)
            {
                try
                {
                    string dirname = Path.GetDirectoryName(initiator.Filename);
                    rslt = Path.Combine(dirname, rslt);
                }
                catch (Exception) { }
            }
            return rslt;
        }
        void PerformDirective(LocatedString locLine)
        {
            string line = locLine.Value;
            string directive = ExtractCommandWord(line);
            string arg = (line.Length > directive.Length+2) ? line.Substring(directive.Length + 2) : "";
            string unexpandedArg = (locLine.ExpandedFrom.Length > directive.Length + 2) ? locLine.ExpandedFrom.Substring(directive.Length + 2) : "";

            switch (directive)
            {
                case PreprocessorDirectives.INCLUDE:
                    int lineNum = 0;
                    if (Chatty) Console.Error.WriteLine(Strings.Including, arg);
                    string fullPath = PathRelativeFromString(locLine, arg);
                    using (var f = File.OpenText(fullPath))
                    {
                        while (!f.EndOfStream)
                        {
                            inputBuffer.Insert(lineNum, new LocatedString { Filename = fullPath, Value = f.ReadLine(), LineNo = lineNum + 1 });
                            lineNum++;
                        }
                    }
                    if(onFileProcessed != null)
                        onFileProcessed.Invoke(this, new CompilerFileEventArgs(CompilerFileEventArgs.EventType.Consume, fullPath));
                    break;

                case PreprocessorDirectives.CONST_DEF:
                    {
                        string[] spacedArgs = unexpandedArg.Split('=').Select(s => s.Trim()).ToArray();

                        if(spacedArgs.Length != 2)
                        {
                            throw new CodeError(locLine, Strings.ErrConstSyntax, Strings.ErrConstSyntaxDetail);
                        }

                        string definition = spacedArgs[0].ToUpperInvariant();
                        string substitution = spacedArgs[1];

                        if ((substitutions.ContainsKey(definition) && !AllowRedefinitions) || Translator.IsKeyword(definition))
                        {
                            throw new RedefinitionError(locLine);
                        }
                        if (Chatty) Console.Error.WriteLine(Strings.ConstDef, definition, substitution);
                        substitutions[definition] = substitution;
                    }
                    break;

                case PreprocessorDirectives.CONST_UNDEF:
                    {
                        string definition = unexpandedArg.Trim().ToUpperInvariant();
                        if (Translator.IsKeyword(definition))
                        {
                            throw new CodeError(locLine, Strings.ErrKwUndef, null);
                        }
                        if (substitutions.ContainsKey(definition))
                        {
                            substitutions.Remove(definition);
                            if (Chatty) Console.Error.WriteLine(Strings.ConstUndef, definition);
                        }
                    }
                    break;

                case PreprocessorDirectives.SET_BINARY_FILTER:
                    try
                    {
                        BinaryFilterMode = (BinFltMode)Enum.Parse(typeof(BinFltMode), arg, true);
                    }
                    catch(Exception e)
                    {
                        bool isFlt = Convert.ToBoolean(arg);
                        BinaryFilterMode = isFlt ? BinFltMode.ONLY_CHART : BinFltMode.NO_FILTER;
                    }
                    if (Chatty) Console.Error.WriteLine(Strings.BinFltSts, BinaryFilterMode);
                    break;

                case PreprocessorDirectives.INCLUDE_BINARY:
                    string fp = PathRelativeFromString(locLine, arg);
                    DSCFile inclusion = DSCReader.ReadFromFile(fp);
                    if (Chatty) Console.Error.WriteLine(Strings.IncBin, inclusion.Commands.Count, arg);
                    IEnumerable<DSCCommand> inclusionList = null;
                    switch(BinaryFilterMode){
                        case BinFltMode.ONLY_CHART:
                            inclusionList = inclusion.Commands.Select(c => c.Unwrap()).Where(cmd => DSCCommand.IsAllowedImportableType(cmd.GetType()) || DSCCommand.IsCriticalType(cmd.GetType()));
                            break;

                        case BinFltMode.WITHOUT_CHART:
                            inclusionList = inclusion.Commands.Select(c => c.Unwrap()).Where(cmd => (!DSCCommand.IsAllowedImportableType(cmd.GetType())) || DSCCommand.IsCriticalType(cmd.GetType()));
                            break;

                        default:
                            inclusionList = inclusion.Commands.Select(c => c.Unwrap());
                            break;
                    }
                    outputBuffer.AddRange(inclusionList
                        .SkipWhile(x => 
                            (BinaryFilterStartTime != null && 
                                (
                                    !(x is Commands.Command_TIME) || 
                                     (x is Commands.Command_TIME && ((Commands.Command_TIME)x).Timestamp < BinaryFilterStartTime.Value))
                                )
                             )
                        .TakeWhile(x => (BinaryFilterEndTime == null || !(x is Commands.Command_TIME && ((Commands.Command_TIME)x).Timestamp > BinaryFilterEndTime.Value))));
                    if (onFileProcessed != null)
                     onFileProcessed.Invoke(this, new CompilerFileEventArgs(CompilerFileEventArgs.EventType.Consume, fp));
                    break;

                case PreprocessorDirectives.SORT_FORCE:
                    if (Chatty) Console.Error.WriteLine(Strings.Sort);
                    var sorted = TimestampBatchSorter.SortedCommandList(outputBuffer);
                    outputBuffer = sorted;
                    break;

                case PreprocessorDirectives.PUSH_CONTEXT:
                    contexts.Add(outputBuffer.ConvertAll(x => x).ToList());
                    if (Chatty) Console.Error.WriteLine(Strings.CtxPush, contexts.Count);
                    break;

                case PreprocessorDirectives.POP_CONTEXT:
                    if (contexts.Count == 0) throw new CodeError(locLine, Strings.ErrTopCtx, Strings.ErrTopCtxDetail);
                    outputBuffer.Clear();
                    outputBuffer.AddRange(contexts.First());
                    contexts.RemoveAt(0);
                    if (Chatty) Console.Error.WriteLine(Strings.CtxPop, contexts.Count);
                    break;

                case PreprocessorDirectives.FORCE_STOP:
                    if (Chatty) Console.Error.WriteLine(Strings.ForceStop);
                    throw new UnkoError(locLine);

                case PreprocessorDirectives.WRITE_FILE:
                    string foutname = PathRelativeFromString(locLine, arg);
                    ExportOutputBuffer(foutname);
                    break;

                case PreprocessorDirectives.SET_VERBOSE:
                    Chatty = true;
                    Console.Error.WriteLine(Strings.Verbose);
                    break;

                case PreprocessorDirectives.CLEAR_BUFFER:
                    outputBuffer.Clear();
                    if (Chatty) Console.Error.WriteLine(Strings.ClrBuf);
                    break;

                case PreprocessorDirectives.BINFLT_SET_TIME:
                    var span = arg.Split(' ').ToArray();
                    var begin = Convert.ToInt32(span[0]);
                    var end = Convert.ToInt32(span[1]);
                    if (begin < 0)
                        BinaryFilterStartTime = null;
                    else
                        BinaryFilterStartTime = begin;
                    if (end < 0)
                        BinaryFilterEndTime = null;
                    else
                        BinaryFilterEndTime = end;
                    break;

                case PreprocessorDirectives.FORLOOP_BEGIN:
                    var defs = new List<ForLoopDefinition>();
                    var args = arg.Split(' ').ToArray();
                    if(args.Length % 4 != 0)
                    {
                        throw new CodeError(locLine, String.Format(Strings.ErrForArgs, PreprocessorDirectives.FORLOOP_BEGIN), Strings.ErrForArgsDetail);
                    }
                    else
                    {
                        for(int i = 0; i < args.Length; i += 4)
                        {
                            var def = new ForLoopDefinition
                            {
                                VarName = args[i].ToUpperInvariant(),
                                From = Convert.ToInt32(args[i + 1]),
                                To = Convert.ToInt32(args[i + 2]),
                                Step = Convert.ToInt32(args[i + 3])
                            };
                            if ((substitutions.ContainsKey(def.VarName) && !AllowRedefinitions) || Translator.IsKeyword(def.VarName))
                            {
                                throw new RedefinitionError(locLine);
                            }
                            if (!def.IsFinite)
                            {
                                throw new CodeError(locLine, String.Format(Strings.InfForLoop, def.VarName), Strings.InfForLoopDetail);
                            }
                            defs.Add(def) ;
                        }
                    }
                    forLoopBufferList.Push(new List<LocatedString>());
                    forLoopContextList.Push(defs);
                    break;

                case PreprocessorDirectives.FORLOOP_END:
                    if(forLoopBufferList.Count == 0)
                    {
                        throw new CodeError(locLine, String.Format(Strings.ErrUnexpected, PreprocessorDirectives.FORLOOP_END), "");
                    } 
                    else
                    {
                        var curBuf = forLoopBufferList.Pop();
                        var curDef = forLoopContextList.Pop();
                        var interpolatedLines = new List<LocatedString>();

                        int sequenceLength = curDef.Max(ipol => ipol.StepCount);
                        var interpolators = new List<ForLooper>();
                        interpolators.AddRange(curDef.Select(def => new ForLooper(def)));
                        
                        for(int i = 0; i < sequenceLength; i++)
                        {
                            foreach(var ip in interpolators)
                            {
                                if(ip.WillChange)
                                {
                                    interpolatedLines.Add(new LocatedString
                                    {
                                        ExpandedFrom = locLine.Value,
                                        Filename = locLine.Filename,
                                        LineNo = locLine.LineNo,
                                        Value = Translator.PREPROC_MARKER + PreprocessorDirectives.CONST_DEF + " " + ip.Definition.VarName + "=" + ip.Advance().ToString()
                                    });
                                }
                            }
                            interpolatedLines.AddRange(curBuf);
                            foreach (var ip in interpolators)
                            {
                                if (ip.DidChange)
                                {
                                    interpolatedLines.Add(new LocatedString
                                    {
                                        ExpandedFrom = locLine.Value,
                                        Filename = locLine.Filename,
                                        LineNo = locLine.LineNo,
                                        Value = Translator.PREPROC_MARKER + PreprocessorDirectives.CONST_UNDEF + " " + ip.Definition.VarName
                                    }) ;
                                }
                            }
                        }

                        inputBuffer.InsertRange(0, interpolatedLines);
                    }
                    break;

                case PreprocessorDirectives.SLIDETIME:
                    {
                        var slide = Convert.ToInt32(arg);
                        var slideUnsigned = (uint) Math.Abs(slide);
                        var slideDir = Math.Sign(slide);
                        outputBuffer.ForEach(cmd =>
                        {
                            if (cmd is Commands.Command_TIME)
                            {
                                if (slideDir < 0)
                                {
                                    ((Commands.Command_TIME)cmd).Timestamp -= slideUnsigned;
                                }
                                else
                                {
                                    ((Commands.Command_TIME)cmd).Timestamp += slideUnsigned;
                                }
                            }
                        });
                    }
                    break;


                default:
                    throw new UnknownDirective(locLine);
            }
        }

        LocatedString PrepareString(LocatedString input)
        {
            string substitutedLine = input.Value;
            foreach (KeyValuePair<string, string> sub in substitutions)
            {
                substitutedLine = Regex.Replace(substitutedLine, sub.Key, sub.Value, RegexOptions.IgnoreCase);
            }
            substitutedLine = substitutedLine.Split(Translator.LINE_COMMENTS, StringSplitOptions.None).First();
            if(Chatty && substitutedLine != input.Value && substitutedLine.Length > 0)
            {
                Console.Error.WriteLine(Strings.Subst, input.Value, substitutedLine);
            }
            return input.Expand(substitutedLine);
        }
        public DSCCommand ProcessNextLine(LocatedString line)
        {
            var substitutedLine = PrepareString(line);
            var lineKind = Translator.ClassifyLine(substitutedLine.Value);
            if (lineKind == Translator.LineKind.Preprocessor)
            {
                // this is a command for us
                PerformDirective(substitutedLine);

                // and no output will be given
                return null;
            }
            else if (lineKind == Translator.LineKind.Command)
            {
                // this is a command to be translated
                return Translator.ParseCommandLine(substitutedLine);
            }
            else return null;
        }

        public List<DSCCommand> ProcessScript(string lines, string filename, bool rethrows = false, bool onlyConst = false)
        {
            foreach(var line in lines.Split('\n'))
            {
                inputBuffer.Add(new LocatedString { Filename = filename, LineNo = inputBuffer.Count + 1, Value = line.Trim() });
            }

            while(inputBuffer.Count > 0)
            {
                DSCCommand emittedCmd = null;
                LocatedString line = inputBuffer.First();
                inputBuffer.RemoveAt(0);
                try
                {
                    string cmdWord = ExtractCommandWord(line.Value);
                   if (forLoopBufferList.Count == 0 || cmdWord == PreprocessorDirectives.FORLOOP_END)
                    {
                        if (!onlyConst ||
                        (Translator.ClassifyLine(PrepareString(line).Value) == Translator.LineKind.Preprocessor &&
                            (cmdWord == PreprocessorDirectives.CONST_DEF ||
                             cmdWord == PreprocessorDirectives.INCLUDE)))
                        {
                            emittedCmd = ProcessNextLine(line);
                        }
                    } 
                    else
                    {
                        forLoopBufferList.Peek().Add(line);
                    }
                } catch (CodeError ce)
                {
                    ce.Print();
                    if (rethrows) throw ce;
                    return null;
                } catch (Exception e)
                {
                    CodeError ce = new CodeError(line, e.Message, Strings.OutsideError, e);
                    ce.Print();
                    if (rethrows) throw ce;

                    return null;
                }
                if(emittedCmd is DSCCommand)
                {
                    if (Chatty) Console.Error.WriteLine(Strings.Emit, Path.GetFileName(line.Filename), emittedCmd, line.Value);
                    outputBuffer.Add(emittedCmd);
                }
            }

            return outputBuffer;
        }
    }
}
