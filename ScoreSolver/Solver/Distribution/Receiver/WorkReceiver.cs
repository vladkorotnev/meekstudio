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
        void ReceiveSolution(DecisionPathNode finalNode, WorkProvider from);
        /// <summary>
        /// A list of accumulated results
        /// </summary>
        List<DecisionPathNode> Solutions { get; }
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
        private DecisionPathNode solution = null;
        public List<DecisionPathNode> Solutions
        {
            get
            {
                if (solution == null) return new List<DecisionPathNode>();
                else return new List<DecisionPathNode>() { solution };
            }
        }

        public void ReceiveSolution(DecisionPathNode s, WorkProvider from)
        {
            solutionSemaphore.WaitOne();
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            if (curMaxScore < s.state.Score)
            {
                solution = s;
                curMaxScore = solution.state.Score;
                Console.Error.WriteLine("[+{2}s] Found solution with score={0}, attain={1}", solution.state.Score, (from == null ? "??" : from.System.AttainPoint(solution.state).ToString()), Math.Truncate(stopwatch.Elapsed.TotalSeconds));
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
            Solutions = new List<DecisionPathNode>();
            outputDir = outDir;
        }

        private Semaphore solutionSemaphore = new Semaphore(1, 1);
        private Stopwatch stopwatch;
        public List<DecisionPathNode> Solutions { get; set; }
        private string outputDir;


        public void ReceiveSolution(DecisionPathNode solution, WorkProvider from)
        {
            solutionSemaphore.WaitOne();
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
                if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            }
            if (Solutions.All(x => x.state.Score < solution.state.Score))
            {
                Solutions.Clear();
            }
            if (!Solutions.Any(x => x.state.Score > solution.state.Score))
            {
                Solutions.Add(solution);
                Console.Error.WriteLine("[+{2}s] Found solution with score={0}, attain={1}", solution.state.Score, (from == null ? "??" : from.System.AttainPoint(solution.state).ToString()), Math.Truncate(stopwatch.Elapsed.TotalSeconds));

                StringBuilder sb = new StringBuilder();
                var curStep = solution;
                do
                {
                    sb.Insert(0, "\n");
                    sb.Insert(0, curStep.state.Score);
                    curStep = curStep.parentNode;
                } while (curStep != null);
                File.WriteAllText(Path.Combine(outputDir, String.Format("{0}_route.txt", solution.state.Score)), sb.ToString());
            }
            else
            {
                //   Console.Error.WriteLine("Found worse solution with score={0}, attain={1}", solution.state.Score, System.AttainPoint(solution.state));
            }
            solutionSemaphore.Release();
        }
    }
}
