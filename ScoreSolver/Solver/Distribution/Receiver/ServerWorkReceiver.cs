using System;
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
    /// Wraps any <see cref="WorkReceiver"/> to also support receiving results over the network
    /// </summary>
    class ServerWorkReceiverWrapper : WorkReceiver
    {
        /// <summary>
        /// The <see cref="WorkReceiver"/> this one is wrapped around
        /// </summary>
        public WorkReceiver Inner { get; set; }
        public void ReceiveSolution(DecisionPathNode finalNode, WorkProvider from)
        {
            Inner.ReceiveSolution(finalNode, from);
        }
        public List<DecisionPathNode> Solutions { get { return Inner.Solutions; } }

        /// <summary>
        /// Wrap a <see cref="WorkReceiver"/> for network support
        /// </summary>
        /// <param name="host">Host address and port to bind server onto</param>
        /// <param name="inner">Work receiver to wrap</param>
        public ServerWorkReceiverWrapper(IPEndPoint host, WorkReceiver inner)
        {
            Inner = inner;
            endpoint = host;
        }

        private IPEndPoint endpoint;
        private TcpListener serverSocket;
        private bool breakFlag;

        /// <summary>
        /// Starts up the server to listen for incoming connections
        /// </summary>
        public void StartServer()
        {
            serverSocket = new TcpListener(endpoint);
            breakFlag = false;
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
                Console.WriteLine("[RECV] Net client: {0}", client.RemoteEndPoint.ToString());

                var childSocketThread = new Thread(() =>
                {
                    try
                    {
                        NetMessage msg = client.ReceiveObject<NetMessage>();
                        while (msg != null)
                        {
                            switch (msg.Kind)
                            {
                                case NetMessageKind.MSG_GIVE_RESULT:
                                    var res = (NetSolutionMessage)msg;
                                    var rslt = res.Solution;
                                    if (rslt != null)
                                    {
                                        Console.Error.WriteLine("[RECV] Got remote solution with score {0}", rslt.state.Score);
                                        Inner.ReceiveSolution(rslt, null);
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("[RECV] NULL solution!!");
                                    }
                                    break;
                                default:
                                    break;
                            }
                            msg = client.ReceiveObject<NetMessage>();
                        }


                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("[PROV] Net {1} error: {0}", e.Message, client.RemoteEndPoint.ToString());
                    }
                });

                childSocketThread.Start();
            }
        }
    }
}
