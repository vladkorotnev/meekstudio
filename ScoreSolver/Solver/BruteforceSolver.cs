using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreSolver
{

    /// <summary>
    /// A solver that finds all paths in which the game will be cleared by bruteforce of all possible variants
    /// </summary>
    class BruteforceHappeningSolver : ParallelizedSolverBase
    {
        public BruteforceHappeningSolver(WorkProvider prov, WorkReceiver recv) : base(prov, recv) { }

        override protected void SolveFromNodeEx(DecisionPathNode startNode)
        {
            var node = startNode;
            while (node != null && !interruptFlag)
            {
                var nextHappening = Provider.Timeline.NextHappeningFromTime(node.state.Time);
                if (nextHappening != null)
                {
                    var nextStates = nextHappening.GetPotentialOutcomes(node.state, Provider.System);
                    Interlocked.Increment(ref CheckedOutcomes);
                    for (int i = 0; i < nextStates.Count; i++)
                    {
                        var nextState = nextStates[i];

                        if (nextState.IsFinal && !nextState.IsFailed)
                        {
                            var atn = Provider.System.AttainPoint(nextState);
                            if (atn >= Provider.System.GameRules.ClearAttain)
                            {
                                var finishNode = new DecisionPathNode(node, nextState, Provider.MustKeepTree);
                                Receiver.ReceiveSolution(finishNode, Provider);
                                node = null;
                                Interlocked.Increment(ref CheckedSolutions);
                            }
                            else
                            {
                                if(Program.Verbose)
                                    Console.Error.WriteLine("[SOLVE] found bad solution atn={0}, minimum={1}, discarded", atn, Provider.System.GameRules.ClearAttain);
                                Interlocked.Increment(ref BadSolutions);
                                Interlocked.Increment(ref CheckedSolutions);

                                node = null;
                            }
                            if (RealtimeGC)
                            {
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                        }
                        else if (nextState.IsFailed)
                        {
                            // do not keep data, just tally
                            Interlocked.Increment(ref BadSolutions);
                            Interlocked.Increment(ref CheckedSolutions);
                            if(Program.Verbose)
                                Console.Error.WriteLine("[SOLVE] found bad solution track life={0}", nextState.Life);
                            node = null;
                            if (RealtimeGC)
                            {
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                        }
                        else
                        {
                            if (i == nextStates.Count - 1)
                            {
                                // reusing the same thread is good for the environment
                                node = new DecisionPathNode(Provider.MustKeepHistory ? node : null, nextState, Provider.MustKeepTree);
                            }
                            else
                            {
                                SolveFromNodeAsync(new DecisionPathNode(Provider.MustKeepHistory ? node : null, nextState, Provider.MustKeepTree));
                            }
                        }
                    }
                    if (node == null) node = Provider.DequeueWork();

                }
                else throw new Exception("Did not converge");
            }
        }
    }
}
