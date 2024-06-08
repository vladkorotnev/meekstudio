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

        public uint EndTime { get; private set; }

        private List<uint> EventTimes = null;

        /// <summary>
        /// Get the next event after the specified time or null
        /// </summary>
        public Happening NextHappeningFromTime(uint time)
        {
            if (time == 0) return Events[0];
            time += 1;

            if(EventTimes == null)
            {
                InvalidateIndex();
            }

            int idx = EventTimes.BinarySearch(time);
            if(idx >= 0)
            {
                // exact match
                return Events[idx];
            } 
            else
            {
                /*
                 * If the Array does not contain the specified value, the method returns a negative integer.
                 * You can apply the bitwise complement operator to the negative result to produce an index. 
                 * If this index is one greater than the upper bound of the array, 
                 * there are no elements larger than value in the array. 
                 * Otherwise, it is the index of the first element that is larger than value.
                 * */
                idx = ~idx;
                if (idx >= Events.Count) return null;
                return Events[idx];
            }
        }

        public void InvalidateIndex()
        {
            EventTimes = new List<uint>(Events.Select(x => x.Time));
            EndTime = Events.Find(x => x is EndOfLevelHappening).Time;
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

            /**
             *  UPDATE: 2024-06-07
             *  
             *  Also count potential Worst/Wrong cases and account for lost life recovery, thus we need to put the checkpoint not after MinComboNotes+1 note,
             *  but after max(MinComboNotes+1, LostLifeInSegment/LifePerOneCool + 1)
             */

            uint lastHoldStartTime = 0;
            uint checkpointIdCounter = 0;
            uint skipCounter = 0;
            int lifeLossInSegment = 0;
            ButtonState busyButtons = ButtonState.None;

            bool needCheckpoint = false;

            for (int i = 0; i < Events.Count; i++)
            {
                Happening evt = Events[i];

                // No insertions after end
                if (evt is EndOfLevelHappening) break;

                if (evt is NoteHappening note)
                {
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
                        lifeLossInSegment = 0;
                        busyButtons = note.HoldButtons;
                    }
                    else if (evt.Time > (lastHoldStartTime + rules.MaxTicksInHold) && needCheckpoint)
                    {
                        // theoretically max hold was reached, add checkpoint after MinCombo+1 notes or enough notes to recover potential life loss in the segment
                        if (skipCounter >= Math.Max(rules.ComboBonusMinCombo, lifeLossInSegment / rules.LifeScore.Correct.Cool) + 1)
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
                    else
                    {
                        if (busyButtons != ButtonState.None)
                        {
                            if ((busyButtons & note.PressButtons) != ButtonState.None)
                            {
                                // interference found, add life loss
                                if (RuleSet.ButtonTotalCount - Util.CountButtons(busyButtons) >= Util.CountButtons(note.PressButtons))
                                {
                                    // can do a WRONG, no point in other than cool wrong
                                    lifeLossInSegment += Math.Abs(rules.LifeScore.Wrong.Cool);
                                }
                                else
                                {
                                    // can only WORST
                                    lifeLossInSegment += Math.Abs(rules.LifeScore.Wrong.Worst);
                                }
                            } 
                            else
                            {
                                // No interference, this note can be hit properly
                                lifeLossInSegment -= Math.Abs(rules.LifeScore.Correct.Cool);
                                // Do not count the overflow towards life bonus because we don't have an idea of where we started
                                if (lifeLossInSegment <= 0) lifeLossInSegment = 0;
                            }
                        }
                    }
                }
            }

            InvalidateIndex();
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

            InvalidateIndex();
        }

        public HappeningSet()
        {
            Events = new List<Happening>();
        }
        public HappeningSet(params Happening[] happenings) : this()
        {
            Events.AddRange(happenings.OrderBy(x => x.Time));
            InvalidateIndex();
        }
    }
}
