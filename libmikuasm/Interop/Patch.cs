using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    /// <summary>
    /// An abstract app memory patch
    /// </summary>
    public class Patch
    {
        /// <summary>
        /// The name of the patch
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The address of the patch
        /// </summary>
        public IntPtr BaseAddress { get; set; }
        /// <summary>
        /// Original bytes of the patch, if any
        /// </summary>
        public byte[] Reference { get; set; }
        /// <summary>
        /// New bytes of the patch
        /// </summary>
        public byte[] Target { get; set; }

        /// <summary>
        /// Create a new patch
        /// </summary>
        /// <param name="name">Human-readable description of the patch</param>
        /// <param name="at">Location in memory</param>
        /// <param name="from">Original bytes</param>
        /// <param name="to">New bytes</param>
        public Patch(string name, IntPtr at, byte[] from, byte[] to)
        {
            Name = name;
            BaseAddress = at;
            Reference = from;
            Target = to;
        }

        /// <summary>
        /// Create a new patch
        /// </summary>
        /// <param name="name">Human-readable description of the patch</param>
        /// <param name="at">Location in memory</param>
        /// <param name="from">Original bytes</param>
        /// <param name="to">New bytes</param>
        public Patch(string name, long at, byte[] from, byte[] to)
            : this(name, new IntPtr(at), from, to)
        { }

        /// <summary>
        /// Create a new patch without the original bytes integrity check
        /// </summary>
        /// <param name="name">Human-readable description of the patch</param>
        /// <param name="at">Location in memory</param>
        /// <param name="to">New bytes</param>
        public Patch(string name, long at, params byte[] to)
            : this(name, new IntPtr(at), null, to) { }

        /// <summary>
        /// Create a patch applicable to the specified process
        /// </summary>
        /// <param name="pid">ID of the process to apply the patch onto</param>
        /// <returns>Process-specific applicable patch</returns>
        public ProcessPatch ToProcess(IntPtr pid)
        {
            return new ProcessPatch(pid, this);
        }

        /// <summary>
        /// Create a byte array repeating a certain byte a certain number of times
        /// </summary>
        /// <param name="what">Byte to repeat</param>
        /// <param name="count">Number of times to repeat</param>
        /// <returns>Array of <c>what</c> repeated <c>count</c> times</returns>
        public static byte[] Repeat(byte what, int count)
        {
            byte[] nops = new byte[count];

            for (int i = 0; i < count; i++)
                nops[i] = what;

            return nops;
        }

        /// <summary>
        /// Create an array of NOP x86 opcodes.
        /// </summary>
        /// <param name="count">Number of bytes to NOP</param>
        /// <returns>Array of <c>count</c> NOP opcodes</returns>
        public static byte[] Nops(int count)
        {
            return Repeat(0x90, count);
        }
    }
}
