using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreSolver
{
    /// <summary>
    /// Something that can provide workloads
    /// </summary>
    interface WorkProvider
    {
        /// <summary>
        /// Whether the solvers need to keep the decision history
        /// </summary>
        bool MustKeepHistory { get; }
        /// <summary>
        /// Whether the solvers need to keep the whole state tree
        /// </summary>
        bool MustKeepTree { get; }

        /// <summary>
        /// Whether the provider has more workloads
        /// </summary>
        bool HasMoreWork { get; }
        /// <summary>
        /// Get a workload from the provider, if any, or null
        /// </summary>
        DecisionPathNode DequeueWork();
        /// <summary>
        /// Add a workload to the provider for later dequeuing with <see cref="DequeueWork"/>
        /// </summary>
        void EnqueueWork(DecisionPathNode work);
        /// <summary>
        /// The timeline in which work is performed
        /// </summary>
        HappeningSet Timeline { get; }
        /// <summary>
        /// The system which is being simulated
        /// </summary>
        SimulationSystem System { get; }
        /// <summary>
        /// Whether all other entities connected to the provider are done and it's safe to shut down
        /// </summary>
        bool IsSafeToStop { get; }

        /// <summary>
        /// Whether the route should not be considered anymore
        /// </summary>
        bool IsRouteDead(uint routeId);
        /// <summary>
        /// Report the score achieved by the route at a checkpoint
        /// </summary>
        /// <returns>Whether the score was bigger than the current maximum for the checkpoint</returns>
        bool CheckScoreOfCheckpoint(uint checkpointId, long score, uint routeId);
    }

    /// <summary>
    /// A locally hosted workload provider
    /// </summary>
    class LocalWorkProvider : WorkProvider
    {
        /// <summary>
        /// Create a new workload provider
        /// </summary>
        /// <param name="timeline">Timeline to simulate in</param>
        /// <param name="sys">System to simulate</param>
        /// <param name="keepHistory">Keep decision history or sacrifice it to save RAM</param>
        public LocalWorkProvider(HappeningSet timeline, SimulationSystem sys, bool keepHistory = false, bool keepTree = false)
        {
            MustKeepHistory = keepHistory;
            MustKeepTree = keepTree;
            Timeline = timeline;
            System = sys;
            CreateStartingElementIfNeeded();
        }

        /// <summary>
        /// Create a "seed node" with the initial system state
        /// </summary>
        private void CreateStartingElementIfNeeded()
        {
            if(asyncQueue.IsEmpty)
            {
                var startState = new SystemState();
                var startNode = new DecisionPathNode(null, startState, false);
                asyncQueue.Enqueue(startNode);
            }
        }

        private ConcurrentQueue<DecisionPathNode> asyncQueue = new ConcurrentQueue<DecisionPathNode>();
        private Semaphore checkpointSemaphore = new Semaphore(1, 1);
        private Dictionary<uint, long> checkpointToMaxScore = new Dictionary<uint, long>();
        private Dictionary<uint, uint> checkpointToBestRouteId = new Dictionary<uint, uint>();
        private HashSet<uint> deadRouteIds = new HashSet<uint>();

        public bool MustKeepHistory { get; set; }
        public bool MustKeepTree { get; set; }
        public HappeningSet Timeline { get; set;  }
        public SimulationSystem System { get; set; }

        public bool HasMoreWork
        {
            get
            {
                return !asyncQueue.IsEmpty;
            }
        }

        public DecisionPathNode DequeueWork()
        {
            DecisionPathNode work = null;
            bool didSucceed = asyncQueue.TryDequeue(out work);
            if(didSucceed)
            {
                return work;
            }
            return null;
        }

        public void EnqueueWork(DecisionPathNode work)
        {
            asyncQueue.Enqueue(work);
        }

        public bool IsRouteDead(uint routeId)
        {
            return deadRouteIds.Contains(routeId);
        }

        public bool CheckScoreOfCheckpoint(uint checkpointId, long score, uint routeId)
        {
            bool rslt = true;

            checkpointSemaphore.WaitOne();
            
            if(!checkpointToMaxScore.ContainsKey(checkpointId))
            {
                checkpointToMaxScore.Add(checkpointId, score);
                checkpointToBestRouteId.Add(checkpointId, routeId);
                rslt = true;
            } 
            else
            {
                if(checkpointToMaxScore[checkpointId] >= score)
                {
                    // This route is inferior, kill it
                    deadRouteIds.Add(routeId);
                    rslt = false;
                } 
                else if (checkpointToMaxScore[checkpointId] < score)
                {
                    // This route is superior, kill its predecessor
                    checkpointToMaxScore[checkpointId] = score;
                    if(checkpointToBestRouteId.ContainsKey(checkpointId))
                    {
                        deadRouteIds.Add(checkpointToBestRouteId[checkpointId]);
                        checkpointToBestRouteId[checkpointId] = routeId;
                    }
                    rslt = true;
                }
            }

            checkpointSemaphore.Release();
            return rslt;
        }

        public virtual bool IsSafeToStop
        {
            get
            {
                return true;
            }
        }
    }
}
