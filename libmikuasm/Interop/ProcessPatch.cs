using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    /// <summary>
    /// A patch that is applicable to a certain process.
    /// </summary>
    public class ProcessPatch : Patch
    {
        /// <summary>
        /// The process ID the patch will be applied to.
        /// </summary>
        public IntPtr Process { get; private set; }

        /// <summary>
        /// Check whether the patch can be applied (if original bytes are specified)
        /// </summary>
        /// <returns>Whether the original bytes match the expected data</returns>
        public bool IsApplicable()
        {
            if (Target == null || Target.Length == 0) return false;
            if (Reference == null || Reference.Length == 0) return true; // Wildcard

            IntPtr curr = BaseAddress;
            byte[] buffer = new byte[Reference.Length];
            IntPtr read = IntPtr.Zero;
            if (!Native.ReadProcessMemory(Process, BaseAddress, buffer, Reference.Length, out read)) return false;
            if (read.ToInt32() != Reference.Length) return false;
            for (int i = 0; i < Reference.Length; i++)
            {
                if (Reference[i] != buffer[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Apply the patch to the process.
        /// </summary>
        /// <returns>Whether patching was successful</returns>
        public bool Apply()
        {
            IntPtr written = IntPtr.Zero;
            bool rslt = Native.WriteProcessMemory(Process, BaseAddress, Target, Target.Length, out written);
            return rslt;
        }

        /// <summary>
        /// Create a new patch for a process
        /// </summary>
        /// <param name="pHandle">Process handle to apply the patch onto</param>
        /// <param name="name">Human-readable description of the patch</param>
        /// <param name="at">Location in memory</param>
        /// <param name="from">Original bytes</param>
        /// <param name="to">New bytes</param>
        public ProcessPatch(IntPtr pHandle, string name, IntPtr at, byte[] from, byte[] to)
            : base(name, at, from, to)
        {
            Process = pHandle;
        }

        /// <summary>
        /// Prepare a patch to a process
        /// </summary>
        /// <param name="pHandle">Process handle to apply the patch onto</param>
        /// <param name="p">The patch to apply</param>
        public ProcessPatch(IntPtr pHandle, Patch p)
            : this(pHandle, p.Name, p.BaseAddress, p.Reference, p.Target)
        { }
    }
}
