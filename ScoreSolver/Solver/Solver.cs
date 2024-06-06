using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ScoreSolver
{
    /// <summary>
    /// A point in the playthrough decision tree
    /// </summary>
    [Serializable]
    class DecisionPathNode
    {
        /// <summary>
        /// System state at the current point in the tree
        /// </summary>
        public SystemState state;

        /// <summary>
        /// Step of current node
        /// </summary>
        public ulong generation;

        /// <summary>
        /// Decision history leading to this node
        /// </summary>
        public List<DecisionMeta> decisionHistory;

        /// <summary>
        /// Parent node in the game tree
        /// </summary>
        public DecisionPathNode parentNode;

        public DecisionPathNode(DecisionPathNode parent, SystemState sysState, bool keepParent)
        {
            state = sysState;
            decisionHistory = new List<DecisionMeta>();

            if(parent != null)
            {
                generation = parent.generation + 1;
                decisionHistory.AddRange(parent.decisionHistory);
                if (sysState.LastDecisionMeta != null)
                {
                    decisionHistory.Add(sysState.LastDecisionMeta);
                    if(!keepParent)
                        sysState.LastDecisionMeta = null;
                }
                if(keepParent)
                {
                    parentNode = parent;
                }
            }
        }
    }
    

    /// <summary>
    /// An abstract multi-threaded solver
    /// </summary>
    abstract class ParallelizedSolverBase
    {
        /// <summary>
        /// A tally of visited solutions that ended up being failures
        /// </summary>
        public long BadSolutions = 0;
        /// <summary>
        /// Tally of checked solutions
        /// </summary>
        public long CheckedSolutions = 0;
        /// <summary>
        /// Tally of final solutions
        /// </summary>
        public long CheckedOutcomes = 0;

        /// <summary>
        /// Number of parallel threads to use in the thread pool
        /// </summary>
        public int ParallelWorkerCount { get; set; }

        /// <summary>
        /// Number of max allocated work items
        /// </summary>
        public int ThreadLimiterCount { get; set; }

        /// <summary>
        /// Interval between forced GC (in queue allocs)
        /// </summary>
        public int GCInterval { get; set; }

        protected int CurrentWorkerCount = 0;
        private int CurrentGCCount = 0;
        protected Semaphore ThreadLimiter;
        protected bool interruptFlag;

        /// <summary>
        /// Whether to do GC every <see cref="GCInterval"/> queue allocs
        /// </summary>
        public bool PeriodicGC { get; set; }
        /// <summary>
        /// Whether to do GC every time a solution is found
        /// </summary>
        public bool RealtimeGC { get; set; }
        /// <summary>
        /// Peak memory usage in bytes
        /// </summary>
        public long PeakMemoryUse { get; private set; }

        /// <summary>
        /// Create a new solver instance
        /// </summary>
        /// <param name="lvl">Event timeline to solve</param>
        /// <param name="sys">System globals to consider</param>
        public ParallelizedSolverBase(WorkProvider prov, WorkReceiver recv)
        {
            ParallelWorkerCount =  Environment.ProcessorCount;
            ThreadLimiterCount = ParallelWorkerCount * 4;
            GCInterval = 3000000;
            PeriodicGC = true;
            Receiver = recv;
            Provider = prov;
        }

        /// <summary>
        /// Start searching for the solutions synchronously
        /// </summary>
        public void Solve()
        {
            BadSolutions = 0;
            CheckedSolutions = 0;
            CheckedOutcomes = 0;
            CurrentGCCount = 0;
            PeakMemoryUse = 0;
            interruptFlag = false;

            ThreadLimiter = new Semaphore(ThreadLimiterCount, ThreadLimiterCount); 

            ThreadPool.SetMinThreads(1, 0);
            ThreadPool.SetMaxThreads(ParallelWorkerCount, 0);

            GC.AddMemoryPressure(1024 * 1024);

            Stopwatch sw = new Stopwatch();

            Thread sched = new Thread(new ThreadStart(AsyncSchedulerThread));
            sched.Start();

            int statReprintCounter = 0;
            long lastChkSol = 0;
            long lastChkNode = 0;
            sw.Start();
            while(sched.IsAlive)
            {
                Thread.Sleep(300);
                statReprintCounter++;
                if(statReprintCounter == 30)
                {
                    sw.Stop();
                    long nowChkNode = Interlocked.CompareExchange(ref CheckedOutcomes, 0, 0);
                    long nowChkSol = Interlocked.CompareExchange(ref CheckedSolutions, 0, 0);

                    int wkThreadsAvail = 0;
                    int wkThreadsMax = 0;
                    int dummy = 0;
                    ThreadPool.GetMaxThreads(out wkThreadsMax, out dummy);
                    ThreadPool.GetAvailableThreads(out wkThreadsAvail, out dummy);

                    Process currentProcess = Process.GetCurrentProcess();
                    long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
                    if (totalBytesOfMemoryUsed > PeakMemoryUse) PeakMemoryUse = totalBytesOfMemoryUsed;

                    Console.Error.WriteLine("[PERF] {0} node/s, {1} sol/s, threads max={2} avail={3}, {4} sol, {5}MB RAM", Math.Floor((nowChkNode - lastChkNode) / sw.Elapsed.TotalSeconds), Math.Floor((nowChkSol - lastChkSol) / sw.Elapsed.TotalSeconds), wkThreadsMax, wkThreadsAvail, nowChkSol, totalBytesOfMemoryUsed / 1024 / 1024);

                    lastChkNode = nowChkNode;
                    lastChkSol = nowChkSol;

                    statReprintCounter = 0;
                    sw.Restart();
                }
            }
            
            GC.Collect();
        }

        /// <summary>
        /// Provider of workloads
        /// </summary>
        public WorkProvider Provider { get; set; }
        /// <summary>
        /// Receiver of completed workloads (solutions)
        /// </summary>
        public WorkReceiver Receiver { get; set; }

        /// <summary>
        /// Enqueue a decision tree item to be calculated
        /// </summary>
        protected void SolveFromNodeAsync(DecisionPathNode node)
        {
            Provider.EnqueueWork(node);
            if (PeriodicGC) { 
                int nowGcInt = Interlocked.CompareExchange(ref CurrentGCCount, 0, 0);
                if (nowGcInt >= GCInterval)
                {
                    Interlocked.Exchange(ref CurrentGCCount, 0);
                    if(Program.Verbose)
                        Console.Error.WriteLine("[SCHED] Garbage collection enforcement taking place...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                else
                {
                    Interlocked.Increment(ref CurrentGCCount);
                }
            }
        }

        /// <summary>
        /// Scheduler thread that takes workloads from <see cref="Provider"/> and feeds them to the solver core function, printing perf counters every now and then
        /// </summary>
        private void AsyncSchedulerThread()
        {
            Console.Error.WriteLine("[SCHED] Scheduler Thread Started");
            while((Interlocked.CompareExchange(ref CurrentWorkerCount, 0, 0) != 0 || Provider.HasMoreWork) && !interruptFlag)
            {
                if(Provider.HasMoreWork)
                {
                    bool gotOne = false;
                    while (!gotOne && !interruptFlag)
                    {
                        gotOne = ThreadLimiter.WaitOne(3000);
                        if(interruptFlag)
                        {
                            break;
                        }
                        if (!gotOne && Program.Verbose)
                        {
                            int wkThreadsAvail = 0;
                            int wkThreadsMax = 0;
                            int dummy = 0;
                            ThreadPool.GetMaxThreads(out wkThreadsMax, out dummy);
                            ThreadPool.GetAvailableThreads(out wkThreadsAvail, out dummy);
                            Console.Error.WriteLine("[SCHED] throttling WorkerCount={0} InUseThreadCount={1} (max={2})", CurrentWorkerCount, wkThreadsMax - wkThreadsAvail, wkThreadsMax);
                        }
                    }
                    if(interruptFlag)
                    {
                        break;
                    }

                    // Spawn a new work item in the thread pool
                    DecisionPathNode node = Provider.DequeueWork();
                    if (node != null)
                    {
                        Interlocked.Increment(ref CurrentWorkerCount);
                        ThreadPool.QueueUserWorkItem(x => {
                            SolveFromNodeEx(node);
                            Interlocked.Decrement(ref CurrentWorkerCount);
                            ThreadLimiter.Release();
                        });
                    } 
                    else
                    {
                        // did not get any work yet, put back the limiter and wait for a while
                        if(Program.Verbose)
                            Console.Error.WriteLine("[SCHED] Workload underrun!");
                        ThreadLimiter.Release();
                        Thread.Sleep(100);
                    }
                }
            }

            // Local solutions ended, wait for provider to shut down
            int i = 20;
            while(!Provider.IsSafeToStop)
            {
                if(i == 20)
                {
                    Console.Error.WriteLine("[SCHED] Local work finished, waiting for provider to report end of work on sub-machines");
                    i = 0;
                }
                else
                {
                    i++;
                }
                Thread.Sleep(500);
            }
            Console.Error.WriteLine("[SCHED] Scheduler Thread Ended");
        }


        /// <summary>
        /// Iterate through viable outcomes of the specified point in the decision tree and save the solutions in <see cref="Receiver"/>
        /// </summary>
        abstract protected void SolveFromNodeEx(DecisionPathNode node);

    }


}
