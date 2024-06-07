using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ScoreSolver
{
    /// <summary>
    /// An accumulator for completed simulation results
    /// </summary>
    interface WorkReceiver
    {
        /// <summary>
        /// Keep and process a finished simulation result
        /// </summary>
        /// <param name="finalNode">Resulting node</param>
        /// <param name="from">Provider which originated the start workload (optional)</param>
        void ReceiveSolution(SystemState finalNode, WorkProvider from);
        /// <summary>
        /// A list of accumulated results
        /// </summary>
        List<SystemState> Solutions { get; }
    }

    /// <summary>
    /// A <see cref="WorkReceiver"/> aiming for finding the maximum score
    /// </summary>
    class MaxScoreWorkReceiver  : WorkReceiver
    {
        public MaxScoreWorkReceiver()
        {
        }

        private Semaphore solutionSemaphore = new Semaphore(1, 1);
        private Stopwatch stopwatch;
        private long curMaxScore = 0;
        public SystemState Solution { get; private set; }

        public List<SystemState> Solutions { get { return new List<SystemState> { Solution }; } }

        public void ReceiveSolution(SystemState s, WorkProvider from)
        {
            solutionSemaphore.WaitOne();
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            if (curMaxScore < s.Score)
            {
                Solution = s;
                curMaxScore = Solution.Score;
                Console.Error.WriteLine("[+{2}s] Found solution with score={0}, attain={1}", Solution.Score, (from == null ? "??" : from.System.AttainPoint(Solution).ToString()), Math.Truncate(stopwatch.Elapsed.TotalSeconds));
            }
            else
            {
                //if(Program.Verbose)
                //    Console.Error.WriteLine("[RECV] Found worse solution with score={0}", solution.state.Score);
            }
            solutionSemaphore.Release();
        }
    }

    /// <summary>
    /// A <see cref="WorkReceiver"/> which will print step by step scores into TXT files for all routes
    /// </summary>
    class PrintToFileWorkReceiver : WorkReceiver
    {
        public PrintToFileWorkReceiver(string outDir = "routes_output")
        {
            Solutions = new List<SystemState>();
            outputDir = outDir;
        }

        private Semaphore solutionSemaphore = new Semaphore(1, 1);
        private Stopwatch stopwatch;
        public List<SystemState> Solutions { get; set; }
        private string outputDir;


        public void ReceiveSolution(SystemState solution, WorkProvider from)
        {
            solutionSemaphore.WaitOne();
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
                if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            }
            if (Solutions.All(x => x.Score < solution.Score))
            {
                Solutions.Clear();
            }
            if (!Solutions.Any(x => x.Score > solution.Score))
            {
                Solutions.Add(solution);
                Console.Error.WriteLine("[+{2}s] Found solution with score={0}, attain={1}", solution.Score, (from == null ? "??" : from.System.AttainPoint(solution).ToString()), Math.Truncate(stopwatch.Elapsed.TotalSeconds));

                StringBuilder sb = new StringBuilder();
                foreach(var decision in solution.DecisionRecord)
                {
                    sb.AppendLine(String.Format("Combo {0}/Time {1}: {2}", decision.Combo, decision.Time, decision.ToString()));
                }

                File.WriteAllText(Path.Combine(outputDir, String.Format("{0}_route.txt", solution.Score)), sb.ToString());
            }
            else
            {
                //   Console.Error.WriteLine("Found worse solution with score={0}, attain={1}", solution.state.Score, System.AttainPoint(solution.state));
            }
            solutionSemaphore.Release();
        }
    }
}
