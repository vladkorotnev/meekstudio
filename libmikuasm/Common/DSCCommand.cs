using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization;
using System.Text;
using BinarySerialization;
using MikuASM.Common;
using MikuASM.Common.Locales;

namespace MikuASM
{
    [Serializable]
    public class DSCCommandWrapper
    {
        [FieldOrder(0)]
        public UInt32 CommandID;

        [FieldOrder(1)]
        [SubtypeFactory(nameof(CommandID), typeof(DSCCommandFactory))]
        public DSCCommand Data;

        public override string ToString() { return String.Format("Wrapped 0x{0:X}: {1}", CommandID, Data); }

        public DSCCommand Unwrap()
        {
            return Data;
        }
    }

    class DSCCommandFactory : ISubtypeFactory
    {
        private static Dictionary<UInt32, Type> idToType = new Dictionary<uint, Type>();
        private static Dictionary<string, UInt32> typeNameToId = new Dictionary<string, uint>();
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
                        var attr = (CmdMnemonic) attribs.First();
                        idToType.Add((UInt32)attr.ID, type);
                        typeNameToId.Add(type.Name, (UInt32)attr.ID);
                    }
                }
            }
            hasCache = true;
        }
        public bool TryGetKey(Type valueType, out object key)
        {
            BuildCache();
            if(typeNameToId.ContainsKey(valueType.Name))
            {
                key = typeNameToId[valueType.Name];
                return true;
            }
            key = null;
            return false;
        }

        public bool TryGetType(object key, out Type type)
        {
            BuildCache();
            if(idToType.ContainsKey(Convert.ToUInt32(key)))
            {
                type = idToType[Convert.ToUInt32(key)];
                return true;
            }
            Console.WriteLine("Unknown key {0} !!", key);
            throw new UnknownCommand(null);
            type = null;
            return false;
        }
    }


    [Serializable]
    public class DSCCommand
    {
        public static implicit operator DSCCommandWrapper(DSCCommand func) { return new DSCCommandWrapper { Data = func }; }

        [Ignore]
        public uint ArgCount
        {
            get
            {
                return GetArgCountForType(GetType());
            }
        }

        public static uint GetArgCountForType(Type t)
        {
            var mnemonicAttr =
               (CmdMnemonic)Attribute.GetCustomAttribute(t, typeof(CmdMnemonic));
            if (mnemonicAttr == null)
            {
                return 0;
            }
            else
            {
                return mnemonicAttr.ArgCount;
            }
        }
        public static List<string> GetArgNamesForType(Type t)
        {
            var args = t.GetFields()
                .Where(prop => prop.IsDefined(typeof(CmdArg), false))
                .OrderBy(prop => prop.GetCustomAttribute<CmdArg>().ArgNo)
                .Select(prop => prop.Name)
                .ToList();
            return args;
        }
        public static string GetHelpForType(Type t)
        {
            var descAttr =
               (CmdDesc)Attribute.GetCustomAttribute(t, typeof(CmdDesc));
            if (descAttr == null)
            {
                string mnemonic = GetMnemonicForType(t);
                if(mnemonic != null)
                {
                    string localizedHelp = DSCCommandExplanations.ResourceManager.GetString(mnemonic);
                    if(!String.IsNullOrEmpty(localizedHelp))
                    {
                        return localizedHelp;
                    }
                }
                return DSCCommandExplanations.none;
            }
            else
            {
                return descAttr.Text;
            }
        }
        public static string GetMnemonicForType(Type t)
        {
            var mnemonicAttr =
               (CmdMnemonic)Attribute.GetCustomAttribute(t, typeof(CmdMnemonic));
            if (mnemonicAttr == null)
            {
                return null;
            }
            else
            {
                return mnemonicAttr.Mnemonic;
            }
        }

        public static UInt32 GetCommandIDForType(Type t)
        {
            var mnemonicAttr =
               (CmdMnemonic)Attribute.GetCustomAttribute(t, typeof(CmdMnemonic));
            if (mnemonicAttr == null)
            {
                return (UInt32)CommandNumbers.INVALID;
            }
            else
            {
                return (UInt32) mnemonicAttr.ID;
            }
        }

        public static bool IsAllowedImportableType(Type t)
        {
            var allowFltAttr =
               (AllowBinFlt)Attribute.GetCustomAttribute(t, typeof(AllowBinFlt));
            return (allowFltAttr != null);
        }


        public static bool IsCriticalType(Type t)
        {
            var allowFltAttr =
               (CriticalBinFlt)Attribute.GetCustomAttribute(t, typeof(CriticalBinFlt));
            return (allowFltAttr != null);
        }

        protected virtual object TransformArgByNo(uint argNo, object input)
        {
            var property = GetType().GetFields()
                .Where(prop => prop.IsDefined(typeof(CmdArg), false))
                .Where(prop => prop.GetCustomAttribute<CmdArg>().ArgNo == argNo).First();
            if(property.IsDefined(typeof(ArgCompileTransform), false))
            {
                var transformer = property.GetCustomAttribute<ArgCompileTransform>();
                if(transformer != null)
                {
                    return transformer.TransformInput(input);
                }
            }

            return input;
        }

        // for use in interpreter
        public virtual void SetArgByNo(uint argNo, object val)
        {
            var property = GetType().GetFields()
                .Where(prop => prop.IsDefined(typeof(CmdArg), false))
                .Where(prop => prop.GetCustomAttribute<CmdArg>().ArgNo == argNo).First();
            property.SetValue(this, Convert.ChangeType(TransformArgByNo(argNo, val), property.FieldType));
        }

        public virtual object GetArgByNo(uint argNo)
        {
            var property = GetType().GetFields()
                .Where(prop => prop.IsDefined(typeof(CmdArg), false))
                .Where(prop => prop.GetCustomAttribute<CmdArg>().ArgNo == argNo).First();
            return property.GetValue(this);
        }

        public override string ToString()
        {
            StringBuilder bldr = new StringBuilder();
            bldr.Append(GetMnemonicForType(GetType()));
            if (ArgCount > 0)
            {
                for(uint i = 0; i < ArgCount; i++)
                {
                    if (i>0) bldr.Append(",");
                    bldr.Append(" ");
                    bldr.Append(GetArgByNo(i));
                }
            }
            return bldr.ToString();
        }

        internal virtual DSCCommandWrapper Wrap()
        {
            DSCCommandWrapper wrapper = new DSCCommandWrapper();
            wrapper.CommandID = GetCommandIDForType(GetType());
            wrapper.Data = this;
            return wrapper;
        }
    }

}
