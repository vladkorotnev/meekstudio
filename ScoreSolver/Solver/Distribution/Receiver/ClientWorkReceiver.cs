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
    /// A <see cref="WorkReceiver"/> that connects and sends all the results to a LAN server
    /// </summary>
    class ClientWorkReceiver : WorkReceiver
    {
        /// <summary>
        /// Create a new networked receiver
        /// </summary>
        /// <param name="host">LAN server to connect to</param>
        public ClientWorkReceiver(IPEndPoint host)
        {
            srvAddr = host;
        }
        private Semaphore semaphore = new Semaphore(1, 1);

        private TcpClient sock;
        private IPEndPoint srvAddr;
        private long maxScore = 0;

        /// <summary>
        /// Whether connection to the server is present
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Establishes a connection to the LAN server
        /// </summary>
        public void Connect()
        {
            if (Connected) return;

            sock = new TcpClient();
            sock.Connect(srvAddr);
            if (sock.Connected)
            {
                Connected = true;
                Console.Error.WriteLine("[RCLI] Connected to server");
            }
        }

        public void ReceiveSolution(SystemState finalNode, WorkProvider from)
        {
            if (finalNode.Score < maxScore) return;
            semaphore.WaitOne();
            Console.Error.WriteLine("[RCLI] Upload solution with score {0}", finalNode.Score);

            NetSolutionMessage msg = new NetSolutionMessage(finalNode);
            sock.SendObject(msg);

            maxScore = finalNode.Score;
            semaphore.Release();
        }

        public List<SystemState> Solutions { get { return new List<SystemState>(); } } //dummy
    }
}
