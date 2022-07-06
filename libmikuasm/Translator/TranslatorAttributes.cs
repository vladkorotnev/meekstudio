using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuASM
{
    /// <summary>
    /// Designates a mnemonic for the command in the assembly listing
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CmdMnemonic : Attribute
    {
        public CommandNumbers ID { get; private set; }
        public string Mnemonic { get; private set; }
        public uint ArgCount { get; private set; }

        public CmdMnemonic(CommandNumbers number, string mnemonic, uint argCount)
        {
            ID = number;
            Mnemonic = mnemonic;
            ArgCount = argCount;
        }

        public CmdMnemonic(CommandNumbers number, string mnemonic) : this(number, mnemonic, 0) { }
    }

    /// <summary>
    /// Designates a shortcut in the assembly listing
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CmdShortcut : Attribute
    {
        public string Mnemonic { get; private set; }
        public int ArgCount { get; private set; }

        public CmdShortcut(string mnemonic, int argCount)
        {
            Mnemonic = mnemonic;
            ArgCount = argCount;
        }

        public CmdShortcut(string mnemonic) : this(mnemonic, 0) { }
    }

    /// <summary>
    /// Designates an argument to the command
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    class CmdArg : Attribute
    {
        public uint ArgNo { get; private set; }

        public CmdArg(uint argNo)
        {
            ArgNo = argNo;
        }
    }

    /// <summary>
    /// Defines a transform function for the argument to be preprocessed upon translation into bytecode
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    abstract class ArgCompileTransform : Attribute
    {
        public ArgCompileTransform()
        {
        }

        public virtual object TransformInput(object input)
        {
            // Override this somewhere
            return input;
        }
    }

    /// <summary>
    /// Designates an opcode allowed for binary import
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class AllowBinFlt : Attribute
    {
    }

    /// <summary>
    /// Designates an opcode must always be imported for binary import
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CriticalBinFlt : Attribute
    {
    }

    /// <summary>
    /// Designates a command description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CmdDesc : Attribute
    {
        public string Text { get; private set; }

        public CmdDesc(string description)
        {
            Text = description;
        }

    }

}
