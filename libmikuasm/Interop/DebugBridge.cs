using MikuASM.Common.Locales;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuASM
{
    /// <summary>
    /// Bridge to an in-game debug server
    /// </summary>
    public static class DebugBridge
    {
        // See dscdebugserver.dll
        private static IPEndPoint UDPEndpoint = new IPEndPoint(IPAddress.Loopback, 39399);
        private static Socket UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static Manipulator Manipulator { get; private set; }
        public static Process Process { get; private set; }

        /// <summary>
        /// Whether the server was injected
        /// </summary>
        public static bool IsConnected { get; private set; }

        public static bool IsWithEngineHook { get; private set; }

        private static Thread watchThread = null;
        private static void StartWatcherThread()
        {
            if (watchThread != null) return;
            watchThread = new Thread(new ThreadStart(delegate ()
            {
                while(Manipulator != null && Manipulator.IsAlive)
                {
                    Thread.Sleep(500);
                }
                OnDetached.Invoke(Manipulator, null);
                watchThread = null;
            }));
            watchThread.Start();
        }

        public static event EventHandler OnAttached;
        public static event EventHandler OnDetached;

        static string dscServer = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DSCDebugServer.dll");

        /// <summary>
        /// Send the binary for the engine to execute
        /// </summary>
        /// <param name="script">Compiled script data</param>
        public static void SendScript(byte[] script)
        {
            if (!IsConnected) return;
            UDPSocket.SendTo(script, UDPEndpoint);
        }

        public static void Startup(string exePath, string virtualPvDb = null, string dsc = null, string audio = null, string gmPvLst = null)
        {
            var bs = new Interop.Bootstrapper(exePath, "-w", Path.GetDirectoryName(exePath));
            bs.Initialize();
            Process = bs.GetProcess();
            Manipulator = new Manipulator(Process);

            bs.ApplyPatchSet(Interop.BarebonesPatchCollection.EssentialPatches, Manipulator);
            bs.ApplyPatchSet(Interop.BarebonesPatchCollection.RenderUsagePatches, Manipulator);

            DllLoader loader = new DllLoader(Process);
            
            bool rslt = loader.Inject(dscServer);
            if (!rslt)
            {
                Console.Error.WriteLine(Strings.ErrInjectionFail);
                return;
            }
            Interop.RemoteProcedure rpc = new Interop.RemoteProcedure(bs.ProcessInfo, dscServer);

            Thread.Sleep(200);

            Semaphore waitReady = new Semaphore(0, 1, "dscDbgAppRdy");

            rpc.SetVfs(virtualPvDb, dsc, audio, gmPvLst);
            Thread.Sleep(200);
            rpc.RunFastBoot();

            Thread.Sleep(500);

            
            bs.Continue();
            
            
            while (true)
            {
                // wait until app is ready
                if (waitReady.WaitOne(100)) break;
                if (!Manipulator.IsAlive)
                {
                    OnDetached.Invoke(Manipulator, null);
                    Process = null;
                    Manipulator = null;
                    return;
                }
            }

            rpc.SetState(Interop.AppState.GS_GAME);
            rpc.SetSubState(Interop.AppSubstate.SUB_GAME_SEL);
            if(dsc == null)
            {
                rpc.InvokeBridge();
                IsWithEngineHook = true;
            } else
            {
                IsWithEngineHook = false;
            }
            rpc.Boink();

            IsConnected = true;
            OnAttached.Invoke(Manipulator, null);
            StartWatcherThread();
        }

        /// <summary>
        /// Inject the server DLL into a process
        /// </summary>
        /// <param name="processName">Name of process image</param>
        public static void Inject(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName.Replace(".exe",""));
            if(processList.Length == 0)
            {
                Console.Error.WriteLine(Strings.ErrProcNotFound, processName);
                return;
            }
            if(processList.Length > 1)
            {
                Console.Error.WriteLine(Strings.ErrProcAmbiguous, processName);
                return;
            }

            Process process = processList[0];
            Inject(process);
        }

        public static void Inject(Process process)
        {
            Console.Error.WriteLine(Strings.StsInjecting, process.Id, process.Handle);
            DllLoader loader = new DllLoader(process);

            bool rslt = loader.Inject(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DSCDebugServer.dll"));

            if (!rslt)
            {
                Console.Error.WriteLine(Strings.ErrInjectionFail);
            }
            else
            {
                IsConnected = true;
                Process = process;
                Manipulator = new Manipulator(process);
                Interop.RemoteProcedure rpc = new Interop.RemoteProcedure(Process, dscServer);
                rpc.InvokeBridge();
                IsWithEngineHook = true;
                StartWatcherThread();
                Console.Error.WriteLine(Strings.StsInjected);
            }
        }

        /// <summary>
        /// Simply pretend we did inject the server and re-enable sending commands to it
        /// </summary>
        public static void Reattach()
        {
            IsConnected = true;
            OnAttached.Invoke(Manipulator, null);
        }


        public static void Reattach(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName.Replace(".exe", ""));
            if (processList.Length == 0)
            {
                Console.Error.WriteLine(Strings.ErrProcNotFound, processName);
                return;
            }
            if (processList.Length > 1)
            {
                Console.Error.WriteLine(Strings.ErrProcAmbiguous, processName);
                return;
            }

            Process process = processList[0];
            Reattach(process);
        }

        public static void Reattach(Process process)
        {
            Process = process;
            IsConnected = true;
            Manipulator = new Manipulator(process);
            OnAttached.Invoke(Manipulator, null);
            StartWatcherThread();
        }

        private static float[] ReadPosVec(Int64 addr)
        {
            if (!IsConnected) return null;

            var x = Manipulator.ReadFloat32(new IntPtr(addr)) / 0.000227f;
            var y = Manipulator.ReadFloat32(new IntPtr(addr + 4)) / 0.000227f;
            var z = Manipulator.ReadFloat32(new IntPtr(addr + 8)) / 0.000227f;
            return new float[] { x, y, z };
        }

        public static float[] GetCameraPos()
        {
            return ReadPosVec(0x140fbc2c0);
        }

        public static float[] GetCameraLookat()
        {
            return ReadPosVec(0x140fbc2cc);
        }

        public static float[] GetCameraNormale()
        {
            if (!IsConnected) return null;

            var x = Manipulator.ReadFloat32(new IntPtr(0x140fbc30c)) * 1000;
            var y = Manipulator.ReadFloat32(new IntPtr(0x140fbc30c + 4)) * 1000;
            var z = Manipulator.ReadFloat32(new IntPtr(0x140fbc30c + 8)) * 1000;
            return new float[] { x, y, z };
        }

        public static float[] GetCharaPos(int charaNo)
        {
            switch(charaNo)
            {
                case 0:
                    return ReadPosVec(0x1411b9830);

                case 1:
                    return ReadPosVec(0x1411c1940);

                case 2:
                    return ReadPosVec(0x1411c9a58);

            }
            return null;
        }

    }
}
