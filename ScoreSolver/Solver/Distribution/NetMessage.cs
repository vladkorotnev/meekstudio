using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSolver
{
    /// <summary>
    /// Network message types
    /// </summary>
    enum NetMessageKind : int
    {
        /// <summary>
        /// Client wants to know simulated system parameters
        /// </summary>
        MSG_WANT_ENVIRON = 1,
        /// <summary>
        /// Client wants more workloads
        /// </summary>
        MSG_WANT_WORKLOAD = 2,

        /// <summary>
        /// Server gives simulation parameters
        /// </summary>
        MSG_GIVE_ENVIRON = -1,
        /// <summary>
        /// Server gives more workloads
        /// </summary>
        MSG_GIVE_WORKLOAD = -2,

        /// <summary>
        /// Client gives a simulation result
        /// </summary>
        MSG_GIVE_RESULT = -99
    }

    /// <summary>
    /// Some message sent on the network
    /// </summary>
    [Serializable]
    abstract class NetMessage
    {
        /// <summary>
        /// Message type code
        /// </summary>
        public NetMessageKind Kind { get; private set; }
        internal NetMessage(NetMessageKind which)
        {
            Kind = which;
        }
    }

    /// <summary>
    /// Message querying the simulation parameters from the server
    /// </summary>
    [Serializable]
    class NetParamQueryMessage : NetMessage
    {
        public NetParamQueryMessage() : base(NetMessageKind.MSG_WANT_ENVIRON)
        {
        }
    }

    /// <summary>
    /// Message querying the server for more workloads
    /// </summary>
    [Serializable]
    class NetWorkloadQueryMessage : NetMessage
    {
        /// <summary>
        /// How much workloads to get from the server
        /// </summary>
        public uint HowMuch { get; set; }
        public NetWorkloadQueryMessage(uint howMuch) : base(NetMessageKind.MSG_WANT_WORKLOAD)
        {
            HowMuch = howMuch;
        }
        public NetWorkloadQueryMessage() : base(NetMessageKind.MSG_WANT_WORKLOAD)
        {
        }
    }

    /// <summary>
    /// Message notifying the server of a finished solution
    /// </summary>
    [Serializable]
    class NetSolutionMessage : NetMessage
    {
        /// <summary>
        /// The simulation result
        /// </summary>
        public DecisionPathNode Solution { get; set; }
        public NetSolutionMessage(DecisionPathNode rslt) : base(NetMessageKind.MSG_GIVE_RESULT)
        {
            Solution = rslt;
        }
        public NetSolutionMessage() : base(NetMessageKind.MSG_GIVE_RESULT) { }
    }

    /// <summary>
    /// Message giving the server/client a workload to store on the queue
    /// </summary>
    [Serializable]
    class NetWorkloadMessage : NetMessage
    {
        /// <summary>
        /// Workloads to put on the queue
        /// </summary>
        public List<DecisionPathNode> Workloads { get; set; }
        public NetWorkloadMessage(List<DecisionPathNode> rslt) : base(NetMessageKind.MSG_GIVE_WORKLOAD)
        {
            Workloads = rslt;
        }
        public NetWorkloadMessage() : base(NetMessageKind.MSG_GIVE_WORKLOAD) { }
    }

    /// <summary>
    /// Message telling the client the simulation parameters
    /// </summary>
    [Serializable]
    class NetParamMessage : NetMessage
    {
        /// <summary>
        /// Whether the client should keep the decision history
        /// </summary>
        public bool KeepHistory { get; private set; }
        /// <summary>
        /// Whether the client should keep the whole decision tree
        /// </summary>
        public bool KeepTree { get; private set; }

        /// <summary>
        /// Simulated system parameters
        /// </summary>
        public SimulationSystem System { get; private set; }
        /// <summary>
        /// Simulated events
        /// </summary>
        public HappeningSet Timeline { get; private set; }
        public NetParamMessage() : base(NetMessageKind.MSG_GIVE_ENVIRON)
        {
        }

        public NetParamMessage(bool hist, bool tree, SimulationSystem sys, HappeningSet tl) : base(NetMessageKind.MSG_GIVE_ENVIRON)
        {
            System = sys;
            Timeline = tl;
            KeepHistory = hist;
            KeepTree = tree;
        }
    }

}
