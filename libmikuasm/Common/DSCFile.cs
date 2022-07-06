using BinarySerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuASM
{
    /// <summary>
    /// A DSC file containing a magic number and commands.
    /// </summary>
    [Serializable]
    
    public class DSCFile
    {

        /// <summary>
        /// Magic number, usually 0x20 0x02 0x02 0x12
        /// </summary>
        [FieldOrder(0)]
        public UInt32 MagicalChartNumber = 302121504; // 20 02 02 12

        /// <summary>
        /// Commands contained in the script file
        /// </summary>
        [FieldOrder(1)]
        [ItemSerializeUntil(nameof(DSCCommandWrapper.CommandID), CommandNumbers.END)]
        public List<DSCCommandWrapper> Commands = new List<DSCCommandWrapper>();
    }
}
