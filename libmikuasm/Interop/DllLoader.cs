using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using MikuASM.Common.Locales;

namespace MikuASM
{
    /// <summary>
    /// A loader for loading DLLs into a remote process.
    /// </summary>
    class DllLoader
    {
        private Process process;

        /// <summary>
        /// Create a loader for the specified process.
        /// </summary>
        /// <param name="prcHandle">Handle of the process to load DLLs into</param>
        public DllLoader(Process prc)
        {
            process = prc;
        }

        /// <summary>
        /// Load a DLL into the process.
        /// </summary>
        /// <param name="dllPath">Path to the DLL file desired to be loaded</param>
        /// <returns>Whether the loading was successful</returns>
        public bool Inject(string dllPath)
        {
           if(!File.Exists(dllPath))
            {
                Console.Error.WriteLine(Strings.ErrNotExist, dllPath);
                return false;
            }
            IntPtr processHandle = Native.OpenProcess(process, Native.ProcessAccessFlags.All);
            Console.Error.WriteLine(Strings.GotHandle, processHandle);
            // let's get us some remote memory shall we
            int pathLen = Encoding.Default.GetByteCount(dllPath) + 1;
            byte[] pathBytes = new byte[pathLen];
            Encoding.Default.GetBytes(dllPath).CopyTo(pathBytes, 0);

            IntPtr remoteMemory = Native.VirtualAllocEx(processHandle, IntPtr.Zero, new IntPtr(pathLen),
                                                Native.AllocType.Commit | Native.AllocType.Reserve, Native.MemoryProtection.ReadWrite);
            if (remoteMemory.ToInt64() == 0)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                Console.Error.WriteLine(Strings.ErrRallocFail, error);
                return false;
            }

            // then put some interesting stuff into it
            IntPtr written;
            bool didWrite = Native.WriteProcessMemory(processHandle, remoteMemory, pathBytes,
                                                        pathLen, out written);
            if (!didWrite)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                Console.Error.WriteLine(Strings.ErrRwriteFail, error);
                return false;
            }

            IntPtr threadId;
            IntPtr loadLibraryPtr = Native.GetProcAddress(Native.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            
            IntPtr threadRslt = Native.CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryPtr, remoteMemory, 0, out threadId);

            if (threadRslt.ToInt64() == 0)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                Console.Error.WriteLine(Strings.ErrRthreadFail, error, threadRslt.ToInt32());
                return false;
            }

            Console.Error.WriteLine(Strings.RthreadCreate, threadId.ToInt64());

            return true;
        }
    }
}
