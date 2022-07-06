using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuASM
{
    /// <summary>
    /// A bootstrapper launches an application and manages it's process.
    /// </summary>
    public class Manipulator
    {
        Process Process { get; set; }

        public Manipulator(Process p)
        {
            Process = p;
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
                    return !Process.HasExited;
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
                var proc = Process;
                if (proc == null) return false;
                return proc.MainWindowHandle != IntPtr.Zero;
            }
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
            return Native.ReadProcessMemory(Process.Handle, address, result, result.Length, out read);
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
            bool rslt = Native.WriteProcessMemory(Process.Handle, address, data, data.Length, out written);
            return rslt;
        }

        /// <summary>
        /// Change the protection status of the remote process memory region
        /// </summary>
        /// <param name="address">Start of the affected memory region</param>
        /// <param name="size">Length of the affected memory region</param>
        public void RemoteProtect(Int64 address, UInt64 size)
        {
            uint oldProtect;
            Native.VirtualProtectEx(Process.Handle, new IntPtr(address), new UIntPtr(size), 0x40, out oldProtect);
        }

    }
}
