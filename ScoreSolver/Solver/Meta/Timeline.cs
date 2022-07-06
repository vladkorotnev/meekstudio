using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSolver
{

    /// <summary>
    /// A timeline of events in the playthrough
    /// </summary>
    [Serializable]
    class HappeningSet
    {
        /// <summary>
        /// List of events in the playthrough
        /// </summary>
        public List<Happening> Events { get; private set; }

        /// <summary>
        /// Get the next event after the specified time or null
        /// </summary>
        public Happening NextHappeningFromTime(uint time)
        {
            return Events.SkipWhile(evt => evt.Time <= time).First();
        }

        /// <summary>
        /// Wrap all of the contained events into a <see cref="HappeningTimePassageWrapper"/> to keep track of time flow
        /// </summary>
        public void WrapInTime()
        {
            var newLst = new List<Happening>();
            foreach (var ev in Events)
            {
                if (ev is TimePassageHappening) newLst.Add(ev);
                else newLst.Add(new HappeningTimePassageWrapper(ev));
            }
            Events = newLst;
        }

        /// <summary>
        /// Add checkpoints according to provided game rules
        /// </summary>
        public void InsertCheckpointsFor(RuleSet rules)
        {
            /**
             * Theory behind optimization based on checkpoints:
             * 
             * Consider the following timeline: Xhold X X X X [max hold moment] X
             * For that it is safe to assume:
             *  - the maximum time the Xhold will affect score is T(Xhold) + THoldMax
             *  - it is worth to break the combo by missing X and keep Xhold, if the score at T(Xhold) + THoldMax + Td(T(Xhold) + THoldMax, MinComboNotes + 1)
             *          where T(x) is time of event x, THoldMax is max time for hold, Td(x, y) is time delta after event x and further y notes
             *      ... because otherwise the combo bonus is considerably better than the max hold bonus so it's better to keep combo than keep holding
             *      
             * Thus we can decide that a more likely score attack branch is the one, in which at a checkpoint, created by the above timing rules,
             * the score is bigger than all other branches, as long as all other requirements (e.g. life and attain point) are met.
             * 
             * The actual score comparison and branch discard logic is located inside <see cref="CheckpointingSolver"/> class.
             **/
            uint lastHoldStartTime = 0;
            uint checkpointIdCounter = 0;
            uint skipCounter = 0;
            bool needCheckpoint = false;

            for (int i = 0; i < Events.Count; i++)
            {
                Happening evt = Events[i];
                if (evt is HappeningTimePassageWrapper) evt = ((HappeningTimePassageWrapper)evt).Inner;

                // No insertions after end
                if (evt is EndOfLevelHappening) break;

                if (evt is NoteHappening)
                {
                    var note = ((NoteHappening)evt);
                    if (note.HoldButtons != ButtonState.None)
                    {
                        // Hold started or continued, reset counters of time and notes
                        if (Program.Verbose)
                        {
                            if (!needCheckpoint)
                            {
                                Console.Error.WriteLine("[PREP] Found hold at {0}", note.Time);
                            }
                            else
                            {
                                Console.Error.WriteLine("[PREP] Continue hold from {0} at {1}", lastHoldStartTime, note.Time);
                            }
                        }
                        lastHoldStartTime = note.Time;
                        needCheckpoint = true;
                        skipCounter = 0;
                    }
                    else if (evt.Time > (lastHoldStartTime + rules.MaxTicksInHold) && needCheckpoint)
                    {
                        // theoretically max hold was reached, add checkpoint after MinCombo+1 notes
                        if (skipCounter >= (rules.ComboBonusMinCombo + 1))
                        {
                            checkpointIdCounter++;
                            var ckp = new OptimizationCheckpointHappening(evt.Time + 1, checkpointIdCounter);
                            if (Program.Verbose)
                                Console.Error.WriteLine("[PREP] Time since last hold is more than {0} (cur={1}, since={2}), skipped {4} notes, insert checkpoint {3}", rules.MaxTicksInHold, evt.Time, lastHoldStartTime, checkpointIdCounter, skipCounter);
                            Events.Insert(i + 1, ckp);
                            i++;
                            needCheckpoint = false;
                        }
                        else skipCounter++;
                    }
                }
            }
        }

        /// <summary>
        /// Add checkpoints according to provided game rules but relying by time for performance
        /// </summary>
        public void InsertCheckpointsByTimeFor(RuleSet rules)
        {
            uint lastHoldStartTime = 0;
            uint checkpointIdCounter = 0;
            bool needCheckpoint = false;
            int optimTime = rules.MaxTicksInHold + (rules.MaxTicksInHold / 2); // 1.5x ticks to account for combo bonus

            for (int i = 0; i < Events.Count; i++)
            {
                Happening evt = Events[i];
                if (evt is HappeningTimePassageWrapper) evt = ((HappeningTimePassageWrapper)evt).Inner;

                if (evt is NoteHappening)
                {
                    var note = ((NoteHappening)evt);
                    if (note.HoldButtons != ButtonState.None)
                    {
                        if (Program.Verbose)
                        {
                            if (!needCheckpoint)
                            {
                                Console.Error.WriteLine("[PREP] Found hold at {0}", note.Time);
                            }
                            else
                            {
                                Console.Error.WriteLine("[PREP] Continue hold from {0} at {1}", lastHoldStartTime, note.Time);
                            }
                        }
                        lastHoldStartTime = note.Time;
                        needCheckpoint = true;

                        continue;
                    }
                }

                if (evt.Time > (lastHoldStartTime + optimTime) && needCheckpoint)
                {
                    checkpointIdCounter++;
                    var ckp = new OptimizationCheckpointHappening(evt.Time + 1, checkpointIdCounter);
                    if (Program.Verbose)
                        Console.Error.WriteLine("[PREP] Time since last hold is more than {0} (cur={1}, since={2}), insert checkpoint {3}", optimTime, evt.Time, lastHoldStartTime, checkpointIdCounter);
                    Events.Insert(i + 1, ckp);
                    i++; //skip freshly inserted checkpoint
                    needCheckpoint = false;
                }
            }
        }

        public HappeningSet()
        {
            Events = new List<Happening>();
        }
        public HappeningSet(params Happening[] happenings) : this()
        {
            Events.AddRange(happenings.OrderBy(x => x.Time));
        }
    }
}
