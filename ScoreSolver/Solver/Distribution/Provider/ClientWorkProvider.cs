using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreSolver
{
    /// <summary>
    /// A <see cref="WorkProvider"/> which connects to a LAN server to pull workloads from
    /// </summary>
    class ClientWorkProvider : WorkProvider
    {
        /// <summary>
        /// Create a new workload provider from a LAN server
        /// </summary>
        /// <param name="host">LAN server</param>
        /// <param name="localQueue">Size of local queue buffer</param>
        /// <param name="fetchSize">Number of items to buffer when a buffer underrun happens</param>
        public ClientWorkProvider(IPEndPoint host, uint localQueue = 3000000, uint fetchSize = 100000, bool stayHydrated = false)
        {
            srvAddr = host;
            MaxLocalQueueSize = localQueue;
            UnderrunFetchSize = fetchSize;
            StayHydrated = stayHydrated;
        }

        private TcpClient sock;
        private IPEndPoint srvAddr;
        private Semaphore semaphore = new Semaphore(1, 1);
        private ConcurrentQueue<SystemState> localQueue = new ConcurrentQueue<SystemState>();

        /// <summary>
        /// Max number of items to keep in a local queue
        /// </summary>
        public uint MaxLocalQueueSize { get; set; }
        /// <summary>
        /// Number of items to buffer when a buffer underrun happens
        /// </summary>
        public uint UnderrunFetchSize { get; set; }
        /// <summary>
        /// Whether a connection is established to the server
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Attempt to keep the queue above <see cref="UnderrunFetchSize"/>
        /// </summary>
        public bool StayHydrated { get; set; }

        /// <summary>
        /// Establish a connection to the LAN server and fetch simulation parameters
        /// </summary>
        public void Connect()
        {
            if (Connected) return;
            sock = new TcpClient();
            sock.Connect(srvAddr);
            if(sock.Connected)
            {
                semaphore.WaitOne();
                var environQuery = new NetParamQueryMessage();
                sock.SendObject(environQuery);
                var environRes = sock.ReceiveObject<NetParamMessage>();
                if(environRes != null)
                {
                    Connected = true;
                    MustKeepHistory = environRes.KeepHistory;
                    System = environRes.System;
                    Timeline = environRes.Timeline;
                    Console.Error.WriteLine("[WCLI] Received system settings from server");
                }
                semaphore.Release();
            }
        }

        public bool MustKeepHistory { get; set; }
        public HappeningSet Timeline { get; set; }
        public SimulationSystem System { get; set; }

        /// <summary>
        /// Buffer <see cref="UnderrunFetchSize"/> more workloads from the server
        /// </summary>
        private void FetchWork()
        {
            bool gotSema = semaphore.WaitOne(1000);
            if(localQueue.Count < UnderrunFetchSize && gotSema)
            {
                Console.Error.WriteLine("[WCLI] Fetch {0} items", UnderrunFetchSize);

                var wantWorkMsg = new NetWorkloadQueryMessage(UnderrunFetchSize);
                sock.SendObject(wantWorkMsg);
                var newWork = sock.ReceiveObject<NetWorkloadMessage>();
                int i = 0;
                if (newWork != null)
                {
                    foreach (var load in newWork.Workloads)
                    {
                        if (load != null)
                        {
                            localQueue.Enqueue(load);
                            i++;
                        }
                    }
                }
                Console.Error.WriteLine("[WCLI] Got {0} items", i);
                semaphore.Release();
            }
        }

        public bool HasMoreWork
        {
            get
            {
                if (!localQueue.IsEmpty) return true;
                FetchWork();
                return !localQueue.IsEmpty;
            }
        }

        public SystemState DequeueWork()
        {
            if(localQueue.IsEmpty || (StayHydrated && localQueue.Count < UnderrunFetchSize))
            {
                FetchWork();
            }

            SystemState wrk = null;
            if (localQueue.TryDequeue(out wrk))
            {
                return wrk;
            }
            
            return null;
        }

        public void EnqueueWork(SystemState work)
        {
            if (work == null) return;
            if(localQueue.Count < MaxLocalQueueSize)
            {
                localQueue.Enqueue(work);
                return;
            }

            semaphore.WaitOne();
            var sendMsg = new NetWorkloadMessage(new List<SystemState>() { work });
            sock.SendObject(sendMsg);
            semaphore.Release();
        }

        public bool IsRouteDead(uint routeId)
        {
            throw new NotImplementedException();
        }

        public bool CheckScoreOfCheckpoint(uint checkpointId, long score, uint routeId)
        {
            throw new NotImplementedException();
        }

        public bool IsSafeToStop
        {
            get
            {
                return true;
            }
        }
    }
}
