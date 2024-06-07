using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ScoreSolver
{
    /// <summary>
    /// Wraps any <see cref="WorkProvider"/> to also support distributing workloads over the network
    /// </summary>
    class ServerWorkProvider : WorkProvider
    {
        /// <summary>
        /// Wrap a <see cref="WorkProvider"/> for network support
        /// </summary>
        /// <param name="host">Host address and port to bind server onto</param>
        /// <param name="inner">Work receiver to wrap</param>
        public ServerWorkProvider(IPEndPoint host, WorkProvider inner)
        {
            endpoint = host;
            Inner = inner;
        }

        /// <summary>
        /// The <see cref="WorkProvider"/> this one is wrapped around
        /// </summary>
        public WorkProvider Inner { get; set; }

        private IPEndPoint endpoint;
        private TcpListener serverSocket;
        private bool breakFlag;
        private int clientCount;
        private List<Socket> clients;

        /// <summary>
        /// Starts up the server to listen for incoming connections
        /// </summary>
        public void StartServer()
        {
            serverSocket = new TcpListener(endpoint);
            breakFlag = false;
            clientCount = 0;
            clients = new List<Socket>();
            serverSocket.Start();
            new Thread(new ThreadStart(ServerThread)).Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void StopServer()
        {
            serverSocket.Stop();
            clients.Clear();
        }

        /// <summary>
        /// Main server thread
        /// </summary>
        private void ServerThread()
        {
            while (!breakFlag)
            {
                Socket client = serverSocket.AcceptSocket();
                clients.Add(client);
                Interlocked.Increment(ref clientCount);

                Console.WriteLine("[PROV] Net client: {0}", client.RemoteEndPoint.ToString());

                var childSocketThread = new Thread(() =>
                {
                    try
                    {
                        NetMessage msg = client.ReceiveObject<NetMessage>();
                        while(msg != null)
                        {
                            switch (msg.Kind)
                            {
                                case NetMessageKind.MSG_WANT_ENVIRON:
                                    var res = new NetParamMessage(MustKeepHistory, System, Timeline);
                                    client.SendObject(res);
                                    break;

                                case NetMessageKind.MSG_WANT_WORKLOAD:
                                    var wlQuery = (NetWorkloadQueryMessage)msg;
                                    uint i = 0;
                                    List<SystemState> workloads = new List<SystemState>();
                                    while (i < wlQuery.HowMuch && HasMoreWork)
                                    {
                                        i++;
                                        var wk = DequeueWork();
                                        if (wk != null)
                                            workloads.Add(wk);
                                    }
                                    client.SendObject(new NetWorkloadMessage(workloads));
                                    break;

                                case NetMessageKind.MSG_GIVE_WORKLOAD:
                                    var wlSending = (NetWorkloadMessage)msg;
                                    foreach (var x in wlSending.Workloads)
                                    {
                                        if (x != null)
                                            EnqueueWork(x);
                                    }
                                    break;

                                default:
                                    break;
                            }
                            msg = client.ReceiveObject<NetMessage>();
                        }

         
                        client.Close();
                        clients.Remove(client);
                    } 
                    catch(Exception e)
                    {
                        Console.Error.WriteLine("[PROV] Net {1} error: {0}", e.Message, client.RemoteEndPoint.ToString());
                    }
                    Interlocked.Decrement(ref clientCount);
                });

                childSocketThread.Start();
            }
        }

        // ---- WorkProvider iface

        public bool MustKeepHistory { get { return Inner.MustKeepHistory;  } }
        public bool HasMoreWork { get { return Inner.HasMoreWork; } }
        public HappeningSet Timeline { get { return Inner.Timeline; } }
        public SimulationSystem System { get { return Inner.System; } }


        public bool IsSafeToStop
        {
            get
            {
                return Interlocked.CompareExchange(ref clientCount, 0, 0) == 0 && Inner.IsSafeToStop;
            }
        }

        public SystemState DequeueWork()
        {
            return Inner.DequeueWork();
        }

        public void EnqueueWork(SystemState work)
        {
            Inner.EnqueueWork(work);
        }

        public bool IsRouteDead(uint routeId)
        {
            return Inner.IsRouteDead(routeId);
        }

        public bool CheckScoreOfCheckpoint(uint checkpointId, long score, uint routeId)
        {
            bool isSuperior = Inner.CheckScoreOfCheckpoint(checkpointId, score, routeId);

            if(!isSuperior)
            {
                // this route just got killed, notify clients
                foreach(var client in clients)
                {
                    if(client.Connected)
                    {
                        client.SendObject(new NetRouteDeathMessage(routeId));
                    }
                }
            }

            return isSuperior;
        }
    }
}
