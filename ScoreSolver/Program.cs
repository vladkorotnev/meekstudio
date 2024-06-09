using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ScoreSolver
{
    class Program
    {
        public static bool Verbose { get; private set; }
        static void Main(string[] args)
        {
            Console.WriteLine("MeekScoreSolver v1.3.3 alpha (why is this not in Rust?)");
            Console.WriteLine("by Akasaka, 2021-2022-2024. Special thanks to Korenkonder.");
            Console.WriteLine();

            bool isLanWorker = GetArg("--lan-worker");
            bool isLanServer = GetArg("--lan-server");
            bool naiveMode = GetArg("--naive");
            Verbose = GetArg("-v");

            string fpath = GetArgParam("--dsc");
            string lvlStr = GetArgParam("--level");
            if(GetArg("--help") || ((fpath == null || lvlStr == null) && !isLanWorker))
            {
                Usage(true); return;
            }

            WorkProvider prov = null;
            WorkReceiver recv = null;
            HappeningSet timeline = null;
            SimulationSystem simSys = null;

            if (!isLanWorker)
            {
                if (!File.Exists(fpath))
                {
                    Console.Error.WriteLine("File {0} does not exist", fpath);
                    return;
                }

                Difficulty diff = Difficulty.Easy;
                if (lvlStr.All(x => Char.IsDigit(x)))
                {
                    diff = (Difficulty)uint.Parse(lvlStr);
                    if (diff < Difficulty.Easy || diff > Difficulty.Extreme)
                    {
                        Console.Error.WriteLine("Difficulty {0} does not exist", lvlStr);
                        return;
                    }
                }
                else
                {
                    var rslt = Enum.Parse(typeof(Difficulty), lvlStr, true);
                    if (rslt == null || rslt.Equals(Difficulty.Encore))
                    {
                        Console.Error.WriteLine("Difficulty {0} does not exist", lvlStr);
                        return;
                    }
                    diff = (Difficulty)rslt;
                }

                int playTime = 1;
                if (GetArg("--play-time"))
                {
                    playTime = int.Parse(GetArgParam("--play-time"));
                }

                RuleSet rs = (GetArg("--no-hold-score") ? new HoldlessFTRuleSet(diff, playTime) : new FutureToneRuleSet(diff, playTime));
                Skill skill = new GeneralSkill();
                skill.ProhibitMisses = GetArg("--no-worst-wrong");
                simSys = new SimulationSystem(0, skill, rs);
                Console.Error.WriteLine("[MAIN] Reading DSC...");
                var loader = new DSCToTimelineReader();
                timeline = loader.TimelineOfDsc(fpath);
                simSys.SetRefscoreFromTimeline(timeline);
                Console.Error.WriteLine("[MAIN] {0} notes, RefScore={1}", timeline.Events.Where(x => x is NoteHappening).Count(), simSys.RefScore);

                Console.Error.WriteLine("[MAIN] Setting up simulation...");

                if (!naiveMode)
                {
                    if(GetArg("--checkpoints-by-time"))
                    {
                        timeline.InsertCheckpointsByTimeFor(rs);
                    }
                    else
                    {
                        timeline.InsertCheckpointsFor(rs);
                    }
                    Console.Error.WriteLine("[MAIN] Created {0} checkpoints", timeline.Events.Where(x => x is OptimizationCheckpointHappening).Count());
                }

                if (GetArg("--score-list-dir"))
                {
                    string dirPath = GetArgParam("--score-list-dir");
                    if (dirPath == null)
                    {
                        dirPath = "output_scores";
                    }
                    recv = new PrintToFileWorkReceiver(dirPath);
                }
                else
                {
                    recv = new MaxScoreWorkReceiver();
                }

                bool hist = !GetArg("--no-keep-history");
                prov = new LocalWorkProvider(timeline, simSys, hist);

                if (isLanServer)
                {
                    Console.Error.WriteLine("[MAIN] LAN Host Mode");

                    prov = new ServerWorkProvider(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3939), prov);
                    ((ServerWorkProvider)prov).StartServer();
                    recv = new ServerWorkReceiverWrapper(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3940), recv);
                    ((ServerWorkReceiverWrapper)recv).StartServer();
                }
                else
                {
                    Console.Error.WriteLine("[MAIN] Local Mode");
                }
            }
            else
            {
                string ipAddr = GetArgParam("--lan-worker");
                if(ipAddr == null)
                {
                    Console.Error.WriteLine("Server IP not specified");
                    return;
                }
                IPAddress parsedAddr = null;
                if(!IPAddress.TryParse(ipAddr, out parsedAddr))
                {
                    Console.Error.WriteLine("Server IP not valid");
                    return;
                }

                prov = new ClientWorkProvider(new IPEndPoint(parsedAddr, 3939));
                ((ClientWorkProvider)prov).Connect();
                if(!((ClientWorkProvider)prov).Connected)
                {
                    Console.Error.WriteLine("Failed to connect to provider port");
                    return;
                }

                if (GetArg("--local-queue-max"))
                {
                    ((ClientWorkProvider)prov).MaxLocalQueueSize = uint.Parse(GetArgParam("--local-queue-max"));
                }

                if (GetArg("--local-queue-get"))
                {
                    ((ClientWorkProvider)prov).UnderrunFetchSize = uint.Parse(GetArgParam("--local-queue-get"));
                }

                ((ClientWorkProvider)prov).StayHydrated = GetArg("--stay-hydrated");

                recv = new ClientWorkReceiver(new IPEndPoint(parsedAddr, 3940));
                ((ClientWorkReceiver)recv).Connect();
                if (!((ClientWorkReceiver)recv).Connected)
                {
                    Console.Error.WriteLine("Failed to connect to provider port");
                    return;
                }
            }

            Stopwatch sw = new Stopwatch();

            ParallelizedSolverBase solver = null;
            if(naiveMode)
            {
                Console.Error.WriteLine("[MAIN] Using strategy: bruteforce");
                solver = new BruteforceHappeningSolver(prov, recv);
            }
            else
            {
                Console.Error.WriteLine("[MAIN] Using strategy: checkpointing");
                var cks = new CheckpointingSolver(prov, recv);
                cks.WidthFirstSearch = GetArg("--broad-first");
                if(cks.WidthFirstSearch)
                    Console.Error.WriteLine("[MAIN] Using mode: broad-first");

                solver = cks;
            }

            if (GetArg("--workers"))
                solver.ParallelWorkerCount = int.Parse(GetArgParam("--workers"));

            if (GetArg("--limiter"))
                solver.ThreadLimiterCount = int.Parse(GetArgParam("--limiter"));

            if(!GetArg("--no-preoptim"))
            {
                Console.Error.WriteLine("[MAIN] Priming the solver by solving for perfect casual playthrough");
                Skill tmp = simSys.PlayerSkill;

                simSys.PlayerSkill = new CasualSkill();
                solver.Solve();

                simSys.PlayerSkill = tmp;
                prov.CreateStartingElementIfNeeded();
            }

            Console.Error.WriteLine("[MAIN] Starting solver");
            sw.Start();
            solver.Solve();
            sw.Stop();
            Console.Error.WriteLine("[MAIN] Completed in {0}s", sw.Elapsed.TotalSeconds);


            var maxSol = recv.Solutions.MaxBy(pn => pn.Score);
            if (maxSol != null)
            {
                Console.WriteLine();
                Console.WriteLine("===== MAX SCORE =====");
                Console.WriteLine("score: {0}, attain {1}", maxSol.Score, prov.System.AttainPoint(maxSol));
                Console.WriteLine(SolutionToPrintableAdvisory(maxSol));
            }

            if(isLanServer)
            {
                ((ServerWorkProvider)prov).StopServer();
                ((ServerWorkReceiverWrapper)recv).StopServer();
            }

            if (!isLanWorker && maxSol != null && !GetArg("--single-pass"))
            {
                sw.Reset();
                Console.Error.WriteLine("[MAIN] Starting optimizer");

                var kb = new SortedList<uint, NoteHitDecisionKind>();
                foreach(var decision in maxSol.DecisionRecord)
                {
                    kb.Add(decision.NoteNumber, decision.DecisionKind);
                }

                KnowledgeablePlayerSkill optSkill = new KnowledgeablePlayerSkill(
                    maxSol.NoteNumber,
                    timeline.EndTime,
                    kb,
                    timeline
                );

                simSys.PlayerSkill = optSkill;

                prov.CreateStartingElementIfNeeded();
                solver = new BruteforceHappeningSolver(prov, recv);

                sw.Start();
                solver.Solve();
                sw.Stop();
                Console.Error.WriteLine("[MAIN] Completed in {0}s", sw.Elapsed.TotalSeconds);

                var optSol = recv.Solutions.MaxBy(pn => pn.Score);
                if (optSol != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("===== MAX SCORE OPTIMIZED =====");
                    Console.WriteLine("score: {0}, attain {1}", optSol.Score, prov.System.AttainPoint(optSol));
                    Console.WriteLine(SolutionToPrintableAdvisory(optSol));
                }
            }

            if (Debugger.IsAttached)
            {
                Console.Error.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }

        private static void Usage(bool header)
        {
            if(header)
            {
                Console.WriteLine("Max score bruteforce finder for Future Tone DSC file.");
                Console.WriteLine("Currently only bruteforces as All Cool, no early-/late-, no other rank.");
                Console.WriteLine("Currently does not support slide/chain slide and encore charts.");
                Console.WriteLine();
            }
            Console.WriteLine("Required arguments:");
            Console.WriteLine("--dsc <path>:              path to DSC file (no need if LAN mode)");
            Console.WriteLine("--level <0|1|2|3>:         level of DSC file Easy/Norm/Hard/Extr (affects scores, no need if LAN mode)");
            Console.WriteLine("--lan-worker <IP>:         use machine as LAN worker for provided server (uses port 3939, 3940)");
            Console.WriteLine();

            Console.WriteLine("Optional arguments:");
            Console.WriteLine("--naive:                   use naive non-optimized solver aka bruteforce mode");
            Console.WriteLine("--single-pass:             don't run the late/fast optimizer after finding the solution");
            Console.WriteLine("--no-preoptim:             don't run the casual baseline route first: e.g. if you want to find hidden switchovers");
            Console.WriteLine("--broad-first:             use broad-first search in optimized solver (save RAM?)");
            Console.WriteLine("--no-worst-wrong:          search the perfect route (without misses or wrong notes)");
            Console.WriteLine("--checkpoints-by-time:     place checkpoints by time rather than combo (can affect precision)");
            Console.WriteLine("--play-time <1|2|3>:       time playing in session (affects safety time on easy/norm, default 1)");
            Console.WriteLine("--workers <1~>:            number of worker threads, default = CPU count");
            Console.WriteLine("--limiter <1~>:            number of preallocated work items, default = workers x2");
            Console.WriteLine("--no-keep-history:         don't save decision history and instead only find max score");
            Console.WriteLine("--no-hold-score:           holds don't give any score");
            Console.WriteLine("--score-list-dir <dir>:    print score lists to files in dir");
            Console.WriteLine("--lan-server:              allow solvers from LAN to connect");
            Console.WriteLine("--local-queue-max <int>:   (LAN worker only) max items in local work queue, default 1000000");
            Console.WriteLine("--local-queue-get <int>:   (LAN worker only) fetch batch on local work queue underrun, default 1000");
            Console.WriteLine("--stay-hydrated:           (LAN worker only) try to keep work queue above --local-queue-get value count");
            Console.WriteLine();

            Console.WriteLine("To save output, pipe stdout somewhere, all logs are in stderr anyway");
        }

        private static string GetArgParam(string arg)
        {
            var args = Environment.GetCommandLineArgs().ToList();
            var idx = args.IndexOf(arg);
            if (idx < 0 || idx == args.Count-1) return null;
            return args[idx + 1];
        }

        private static bool GetArg(string arg)
        {
            return Environment.GetCommandLineArgs().Contains(arg);
        }

        public static String SolutionToPrintableAdvisory(SystemState solution)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var decision in solution.DecisionRecord)
            {
                String d = decision.ToString();
                if (d.Length == 0) continue;
                sb.Append(String.Format("[Time {0} / Combo {1} / Note# {3}] {2}\n", FmtTime(decision.Time), decision.Combo, d, decision.NoteNumber));
            }

            return sb.ToString();
        }

        static string FmtTime(uint time)
        {
            if (time < 1000) return String.Format("{0}ms", time);
            if (time < 60000) return String.Format("{0}s", Math.Round((double)time / 1000.0, 3));

            uint min = time / 60000;
            double sec = Math.Round( ((double)time % 60000.0)/1000.0, 3 );
            return String.Format("{0}:{1}", min, (sec < 10.0) ? "0"+sec : sec.ToString());
        }
    }
}
