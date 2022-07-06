using MikuASM.Common.Locales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM
{
    public class CodeError : Exception
    {
        public LocatedString FaultyLine;
        public string ExtraHelp;
        public string Explanation;

        public CodeError(LocatedString where, string what, string extra, Exception inner = null) : this(what, extra, inner)
        {
            FaultyLine = where;
        }


        public CodeError(string what, string extra, Exception inner = null) : base(what, inner) {
            Explanation = what;
            ExtraHelp = extra;
        }

        public void Print()
        {
            var tbg = Console.BackgroundColor;
            var tfg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(Strings.PrintSrcError, Explanation);
            Console.BackgroundColor = tbg;
            Console.ForegroundColor = tfg;
            if (FaultyLine != null)
            {
                Console.Error.WriteLine(Strings.PrintSrcErrorLoc, FaultyLine.Filename, FaultyLine.LineNo, FaultyLine.Value);
                if(FaultyLine.ExpandedFrom != null && FaultyLine.ExpandedFrom != FaultyLine.Value)
                {
                    Console.Error.WriteLine(Strings.PrintSrcErrorExpand, FaultyLine.ExpandedFrom);
                }
            }
            if(ExtraHelp != null)
            {
                Console.Error.WriteLine(Strings.PrintSrcErrorHelp, ExtraHelp);
            }
            
        }
    }
    /// <summary>
    /// Occurs when the interpreter didn't get enough or got too much input arguments for the command
    /// </summary>
    public class ArgumentCountMismatch : CodeError
    {
        public ArgumentCountMismatch(LocatedString where, string help) : base(where, Strings.ErrArgCount, help) { }
    }

    /// <summary>
    /// Occurs when the command isn't known
    /// </summary>
    public class UnknownCommand : CodeError
    {
        public UnknownCommand(LocatedString where) : base(where, Strings.ErrUnknownCmd, null) { }
    }

    /// <summary>
    /// Occurs when the pre- or post-processor encounter an unknown directive
    /// </summary>
    public class UnknownDirective : CodeError
    {
        public UnknownDirective(LocatedString where) : base(where, Strings.ErrUnknownPrepro, null) { }
    }

    /// <summary>
    /// Occurs when trying to redefine an existing keyword
    /// </summary>
    public class RedefinitionError : CodeError
    {
        public RedefinitionError(LocatedString where) : base(where, Strings.ErrRedef, null) { }
    }

    /// <summary>
    /// Occurs when stumbling upon some shit
    /// </summary>
    public class UnkoError : CodeError
    {
        public UnkoError(LocatedString where) : base(where, Strings.ErrUnko, null) { }
    }
}
