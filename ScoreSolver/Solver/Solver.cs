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


        protected int CurrentWorkerCount = 0;
        protected Semaphore ThreadLimiter;
        protected bool interruptFlag;

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
            PeakMemoryUse = 0;
            interruptFlag = false;

            ThreadLimiter = new Semaphore(ThreadLimiterCount, ThreadLimiterCount); 

            ThreadPool.SetMinThreads(ParallelWorkerCount, ParallelWorkerCount);
            ThreadPool.SetMaxThreads(ParallelWorkerCount, ParallelWorkerCount);

            Stopwatch sw = new Stopwatch();

            Thread sched = new Thread(new ThreadStart(AsyncSchedulerThread));
            sched.Start();

            int statReprintCounter = 0;
            long lastChkSol = 0;
            long lastChkNode = 0;
            sw.Start();


            try
            {
                GC.TryStartNoGCRegion(10 * 1024L * 1024L);
                GC.RemoveMemoryPressure(1024L * 1024L * 1024L);
            }
            catch (Exception) { }

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
        protected void SolveFromNodeAsync(SystemState node)
        {
            Provider.EnqueueWork(node);
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
                        gotOne = ThreadLimiter.WaitOne(300);
                        if(interruptFlag)
                        {
                            break;
                        }
                       /* if (!gotOne && Program.Verbose)
                        {
                            int wkThreadsAvail = 0;
                            int wkThreadsMax = 0;
                            int dummy = 0;
                            ThreadPool.GetMaxThreads(out wkThreadsMax, out dummy);
                            ThreadPool.GetAvailableThreads(out wkThreadsAvail, out dummy);
                            Console.Error.WriteLine("[SCHED] throttling WorkerCount={0} InUseThreadCount={1} (max={2})", CurrentWorkerCount, wkThreadsMax - wkThreadsAvail, wkThreadsMax);
                        }*/
                    }
                    if(interruptFlag)
                    {
                        break;
                    }

                    // Spawn a new work item in the thread pool
                    SystemState node = Provider.DequeueWork();
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
        abstract protected void SolveFromNodeEx(SystemState node);

    }


}
