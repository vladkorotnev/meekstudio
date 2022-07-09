﻿using System;
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

        /// <summary>
        /// Starts up the server to listen for incoming connections
        /// </summary>
        public void StartServer()
        {
            serverSocket = new TcpListener(endpoint);
            breakFlag = false;
            clientCount = 0;
            serverSocket.Start();
            new Thread(new ThreadStart(ServerThread)).Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void StopServer()
        {
            serverSocket.Stop();
        }

        /// <summary>
        /// Main server thread
        /// </summary>
        private void ServerThread()
        {
            while (!breakFlag)
            {
                Socket client = serverSocket.AcceptSocket();
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
                                    var res = new NetParamMessage(MustKeepHistory, MustKeepTree, System, Timeline);
                                    client.SendObject(res);
                                    break;

                                case NetMessageKind.MSG_WANT_WORKLOAD:
                                    var wlQuery = (NetWorkloadQueryMessage)msg;
                                    uint i = 0;
                                    List<DecisionPathNode> workloads = new List<DecisionPathNode>();
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
        public bool MustKeepTree { get { return Inner.MustKeepTree; } }
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

        public DecisionPathNode DequeueWork()
        {
            return Inner.DequeueWork();
        }

        public void EnqueueWork(DecisionPathNode work)
        {
            Inner.EnqueueWork(work);
        }
    }
}