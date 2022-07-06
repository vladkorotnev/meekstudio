using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    /// <summary>
    /// Interface to call procedures in a DLL inside the context of a remote process.
    /// </summary>
    class RemoteProcedure
    {
        private string dllPath;
        private Native.PROCESS_INFORMATION process;
        /// <summary>
        /// Connect to the specified process for calling procedures in the specified DLL.
        /// </summary>
        /// <param name="prc">Remote process to connect to</param>
        /// <param name="dll">Path of the DLL to run procedures from</param>
        public RemoteProcedure(Native.PROCESS_INFORMATION prc, string dll)
        {
            process = prc;
            dllPath = dll;
        }

        public RemoteProcedure(Process prc, string dll)
        {
            process = new Native.PROCESS_INFORMATION();
            process.hProcess = prc.Handle;
            
            dllPath = dll;
        }


        static IntPtr lib;

        /// <summary>
        /// Call the specified procedure in the DLL in the remote process context.
        /// </summary>
        /// <param name="procName">Name of the procedure</param>
        /// <param name="argument">Pointer to the argument or numeric argument</param>
        /// <returns></returns>
        public IntPtr Call(string procName, IntPtr argument)
        {
            IntPtr threadId;
            if (lib == IntPtr.Zero)
            {
                lib = Native.LoadLibrary(dllPath);
            }
            IntPtr loadLibraryPtr = Native.GetProcAddress(lib, procName);
            IntPtr threadRslt = Native.CreateRemoteThread(process.hProcess, IntPtr.Zero, 0, loadLibraryPtr, argument, 0, out threadId);

            if (threadRslt.ToInt32() == 0)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                return IntPtr.Zero;
            }

            return threadId;
        }

        /// <summary>
        /// Call the FastBoot routine in the interop DLL.
        /// </summary>
        public void RunFastBoot()
        {
            Call("StartFastboot", IntPtr.Zero);
        }

        /// <summary>
        /// Invoke the debug bridge hooks
        /// </summary>
        public void InvokeBridge()
        {
            Call("InvokeBridge", IntPtr.Zero);
        }

        public void Boink()
        {
            Call("Boink", IntPtr.Zero);
        }

        public void SetState(AppState state)
        {
            Call("SetState", new IntPtr((uint)state));
        }

        public void SetSubState(AppSubstate sub)
        {
            Call("SetSubState", new IntPtr((uint)sub));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VfsInfo
        {
            public IntPtr pvdbPath;
            public  IntPtr dscPath;
            public IntPtr audioPath;
            public IntPtr gmPvLstFarcPath;
        }

        public void SetVfs(string pvDb, string dsc, string audio, string gmPvLstFarcPath)
        {
            if (pvDb == null || !File.Exists(pvDb)) return;

            VfsInfo vfs = new VfsInfo();

            vfs.pvdbPath = RemoteString(pvDb);
            if(dsc == null)
            {
                vfs.dscPath = IntPtr.Zero;
            } 
            else
            {
                vfs.dscPath = RemoteString(dsc);
            }
            if (audio == null)
            {
                vfs.audioPath = IntPtr.Zero;
            }
            else
            {
                vfs.audioPath = RemoteString(audio);
            }
            if (gmPvLstFarcPath == null)
            {
                vfs.gmPvLstFarcPath = IntPtr.Zero;
            }
            else
            {
                vfs.gmPvLstFarcPath = RemoteString(gmPvLstFarcPath);
            }

            int size = Marshal.SizeOf(vfs);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(vfs, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            IntPtr arg = RemoteMemory(arr);
            Call("VirtualPvPd", arg);
        }

        /// <summary>
        /// Create a string in the remote process memory.
        /// </summary>
        /// <param name="data">Contents of the remote string</param>
        /// <returns>Pointer to the string in the remote process memory</returns>
        public IntPtr RemoteString(string data)
        {
            int len = Encoding.UTF8.GetByteCount(data) + 1;
            byte[] dataBytes = new byte[len];
            Encoding.UTF8.GetBytes(data).CopyTo(dataBytes, 0);

            return RemoteMemory(dataBytes);
        }

       public IntPtr RemoteMemory(byte[] dataBytes)
        {
            IntPtr remoteMemory = Native.VirtualAllocEx(process.hProcess, IntPtr.Zero, new IntPtr(dataBytes.Length),
                                                Native.AllocType.Commit | Native.AllocType.Reserve, Native.MemoryProtection.ReadWrite);
            if (remoteMemory.ToInt32() == 0)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                return IntPtr.Zero;
            }

            // then put some interesting stuff into it
            IntPtr written;
            bool didWrite = Native.WriteProcessMemory(process.hProcess, remoteMemory, dataBytes,
                                                        dataBytes.Length, out written);
            if (!didWrite)
            {
                uint error = (uint)Marshal.GetLastWin32Error();
                return IntPtr.Zero;
            }


            return remoteMemory;
        }
    }
}
