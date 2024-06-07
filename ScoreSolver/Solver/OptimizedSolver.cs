using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreSolver
{
    /// <summary>
    /// A solver that finds all paths in which the game will be cleared, comparing the scores on every checkpoint and discarding the least viable variants
    /// </summary>
    class CheckpointingSolver : ParallelizedSolverBase
    {
        public CheckpointingSolver(WorkProvider prov, WorkReceiver recv) : base(prov, recv) { }

        public bool WidthFirstSearch { get; set; }

        override protected void SolveFromNodeEx(DecisionPathNode startNode)
        {
            var node = startNode;
            while (node != null && !interruptFlag)
            {
                if (Provider.IsRouteDead(node.state.RouteId))
                {
                    if(Program.Verbose)
                        Console.Error.WriteLine("[SOLVE] abandoning item of route id {0} because it was killed", node.state.RouteId);
                    node = null;
                    continue;
                }

                var nextHappening = Provider.Timeline.NextHappeningFromTime(node.state.Time);
                if (nextHappening != null)
                {
                    var nextStates = nextHappening.GetPotentialOutcomes(node.state, Provider.System);
                    Interlocked.Increment(ref CheckedOutcomes);
                    if (nextHappening is OptimizationCheckpointHappening)
                    {
                        var ckp = (OptimizationCheckpointHappening)nextHappening;
                        var chkState = nextStates[0];
                        if(!Provider.CheckScoreOfCheckpoint(ckp.CheckpointID, chkState.Score, chkState.RouteId))
                        {
                            // score less than some other route, discard branch
                            if (Program.Verbose)
                                Console.Error.WriteLine("[SOLVE] found worse solution score={0} checkpointID={1}", chkState.Score, ckp.CheckpointID);

                            node = null;
                            continue;
                        } 
                        else
                        {
                            node = new DecisionPathNode(Provider.MustKeepHistory ? node : null, chkState, Provider.MustKeepTree);
                        }
                        continue;
                    }
                    else
                    {
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
                                if (i == nextStates.Count - 1 && !WidthFirstSearch)
                                {
                                    // reusing the same thread is good for the environment
                                    
                                    if(Provider.MustKeepTree)
                                    {
                                        node = new DecisionPathNode(Provider.MustKeepHistory ? node : null, nextState, Provider.MustKeepTree);
                                    }
                                    else
                                    {
                                        node.Update(nextState);
                                    }
                                }
                                else 
                                {
                                    // Put the node onto the queue
                                    SolveFromNodeAsync(new DecisionPathNode(Provider.MustKeepHistory ? node : null, nextState, Provider.MustKeepTree));
                                }
                            }
                        }
                        if(WidthFirstSearch)
                        {
                            node = Provider.DequeueWork();
                        }
                    }
                }
                else throw new Exception("Did not converge");
            }
        }
    }
}
