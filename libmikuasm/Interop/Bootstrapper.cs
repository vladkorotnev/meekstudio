using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    /// <summary>
    /// A bootstrapper launches an application and manages it's process.
    /// </summary>
    class Bootstrapper
    {
        /// <summary>
        /// Path to the application EXE file.
        /// </summary>
        public string Executable { get; private set; }
        /// <summary>
        /// The arguments used to launch the application.
        /// </summary>
        public string Arguments { get; private set; }
        /// <summary>
        /// The working directory the application was launched in.
        /// </summary>
        public string CurDir { get; private set; }

        /// <summary>
        /// Process information of the application process.
        /// </summary>
        public Native.PROCESS_INFORMATION ProcessInfo { get; private set; }

        /// <summary>
        /// Prepare a bootstrapper to launch an application in the specified folder with specified arguments
        /// </summary>
        /// <param name="exe">Path to the EXE file to launch</param>
        /// <param name="args">Arguments to pass when launching</param>
        /// <param name="dir">Working directory to launch inside</param>
        public Bootstrapper(string exe, string args, string dir)
        {
            Executable = exe;
            Arguments = args;
            CurDir = dir;
        }

        /// <summary>
        /// Creates the application process.
        /// </summary>
        /// <returns>Whether the process could be created</returns>
        public bool Initialize()
        {
            Native.STARTUPINFO si = new Native.STARTUPINFO();
            Native.PROCESS_INFORMATION pi = new Native.PROCESS_INFORMATION();
            string argStr = (Executable + " " + Arguments);
            bool rslt = Native.CreateProcess(null, argStr, IntPtr.Zero, IntPtr.Zero, false,
                                             Native.CreateProcessFlags.CREATE_SUSPENDED, IntPtr.Zero, CurDir, ref si, out pi);
            ProcessInfo = pi;
              return rslt;
        }

        /// <summary>
        /// Whether the process is still alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                try
                {
                    var proc = Process.GetProcessById(ProcessInfo.dwProcessId);
                    if (proc == null) return false;
                    return !proc.HasExited;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Whether the process has created a main window.
        /// </summary>
        public bool HasWindow
        {
            get
            {
                if (!IsAlive) return false;
                var proc = Process.GetProcessById(ProcessInfo.dwProcessId);
                if (proc == null) return false;
                return proc.MainWindowHandle != IntPtr.Zero;
            }
        }

        /// <summary>
        /// Unpause the process and continue execution.
        /// </summary>
        /// <returns>Whether the unpause operation was successful</returns>
        public uint Continue()
        {
            uint rslt = Native.ResumeThread(ProcessInfo.hThread);
            return rslt;
        }

        /// <summary>
        /// Pause the process and suspend execution.
        /// </summary>
        /// <returns>Whether the unpause operation was successful</returns>
        public uint Pause()
        {
            uint rslt = Native.SuspendThread(ProcessInfo.hThread);
            return rslt;
        }

        /// <summary>
        /// Kills the process.
        /// </summary>
        public void Kill()
        {
            Native.TerminateProcess(ProcessInfo.hProcess, 0);
        }

        /// <summary>
        /// Reads the memory from the process' address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="result">Byte array to read into</param>
        /// <returns>Whether reading was successful</returns>
        public bool ReadMemory(IntPtr address, ref byte[] result)
        {
            IntPtr read;
            return Native.ReadProcessMemory(ProcessInfo.hProcess, address, result, result.Length, out read);
        }

        /// <summary>
        /// Reads an i32 value from the process' address space.
        /// </summary>
        /// <param name="address">Address to read the i32 value at</param>
        /// <returns>Value at requested address</returns>
        public Int32 ReadInt32(IntPtr address)
        {
            byte[] outp = new byte[4];
            ReadMemory(address, ref outp);
            return BitConverter.ToInt32(outp, 0);
        }

        /// <summary>
        /// Reads an f32 value from the process' address space.
        /// </summary>
        /// <param name="address">Address to read the i32 value at</param>
        /// <returns>Value at requested address</returns>
        public float ReadFloat32(IntPtr address)
        {
            byte[] outp = new byte[4];
            ReadMemory(address, ref outp);
            return BitConverter.ToSingle(outp, 0);
        }

        /// <summary>
        /// Write an i32 value at the specified address in the process' address space.
        /// </summary>
        /// <param name="address">Address to write the i32 value at</param>
        /// <param name="data">i32 value to write</param>
        /// <returns>Whether writing was successful</returns>
        public bool WriteInt32(IntPtr address, Int32 data)
        {
            return WriteMemory(address, BitConverter.GetBytes(data));
        }

        /// <summary>
        /// Write a float value at the specified address in the process' address space.
        /// </summary>
        /// <param name="address">Address to write the float value at</param>
        /// <param name="data">Float value to write</param>
        /// <returns>Whether writing was successful</returns>
        public bool WriteFloat(IntPtr address, float data)
        {
            return WriteMemory(address, BitConverter.GetBytes(data));
        }

        /// <summary>
        /// Write an abstract byte array at the specified address in the process' address space.
        /// </summary>
        /// <param name="address">Address to write the data at</param>
        /// <param name="data">Data to write</param>
        /// <returns>Whether writing was successful</returns>
        public bool WriteMemory(IntPtr address, byte[] data)
        {
            IntPtr written = IntPtr.Zero;
            bool rslt = Native.WriteProcessMemory(ProcessInfo.hProcess, address, data, data.Length, out written);
            return rslt;
        }

        public Process GetProcess()
        {
            return Process.GetProcessById(ProcessInfo.dwProcessId);
        }

        /// <summary>
        /// Detach from the process and cease any operation.
        /// </summary>
        public void End()
        {
            Native.CloseHandle(ProcessInfo.hProcess);
            Native.CloseHandle(ProcessInfo.hThread);
        }

        /// <summary>
        /// Apply a patch to the game application. 
        /// If an error occurs, transition the loader into an errored state and display it.
        /// </summary>
        /// <param name="p">Patch to apply</param>
        /// <returns>Whether the patch was applied.</returns>
        public bool ApplyPatch(Patch p, Manipulator m)
        {
            ProcessPatch pp = p.ToProcess(ProcessInfo.hProcess);
            m.RemoteProtect(pp.BaseAddress.ToInt64(), Convert.ToUInt64(pp.Target.Length));
            return pp.Apply();
        }

        /// <summary>
        /// Apply a set of patches to the game app.
        /// If an error occurs, transition the loader into an errored state and display it.
        /// </summary>
        /// <param name="patches">Set of patches</param>
        /// <returns>Whether all patches were applied successfully</returns>
        public bool ApplyPatchSet(Patch[] patches, Manipulator m)
        {
            foreach (Patch p in patches)
            {
                if (!ApplyPatch(p, m)) return false;
            }
            return true;
        }
    }
}
