using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreSolver
{
    [Serializable]
    [Flags]
    enum NoteHitDecisionKind
    {
        Undecided_Invalid = 0,
        Hit = 1, // -> HitAndHold(resetHold = false)
        Switch = 2, // -> HitAndHold(resetHold = false)
        Wrong = 4, // -> Wrong()
        Miss = 8 // -> Miss()
    }

    static class NoteHitDecisionKindExtz
    {
        static NoteHitDecisionKind[] _Cases = new NoteHitDecisionKind[] { NoteHitDecisionKind.Hit, NoteHitDecisionKind.Switch, NoteHitDecisionKind.Wrong, NoteHitDecisionKind.Miss };
        public static NoteHitDecisionKind[] AllCases(this NoteHitDecisionKind _)
        {
            return _Cases;
        }

        public static uint Count(this NoteHitDecisionKind kind)
        {
            UInt32 v = (UInt32)kind;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            UInt32 c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        public static bool IsHitDecision(this NoteHitDecisionKind kind)
        {
            return kind == NoteHitDecisionKind.Hit || kind == NoteHitDecisionKind.Switch;
        }
    }

    [Serializable]
    ref struct NoteHitDecision
    {
        public int Offset;
        public NoteHitDecisionKind Decisions;
    }

    /// <summary>
    /// A player skill, containing score picking logic
    /// </summary>
    [Serializable]
    abstract class Skill
    {
        /// <summary>
        /// ms in one game frame
        /// </summary>
        protected const int ONE_FRAME = 17;

        /// <summary>
        /// If set, prohibit generating WORST and WRONG notes
        /// </summary>
        public bool ProhibitMisses;

        /// <summary>
        /// Check miss/wrong branches for HOLD events
        /// </summary>
        public bool AllowMissHold = false;

        /// <summary>
        /// How many ms it takes for the player to switch between hold combinations
        /// E.g. when holding O and dropping it for the sake of holding the upcoming X
        /// For high-skilled players this is normally 1 frame at 60FPS = 16.667ms
        /// </summary>
        public uint FrameLossOnSwitch = ONE_FRAME;

        /// <summary>
        /// How many ms it takes for the player to repress the same button
        /// E.g. when holding O and switching to the next O hold
        /// For high-skilled players this is normally twice the Switch loss.
        /// </summary>
        public uint FrameLossOnRepress = 2 * ONE_FRAME;

        /// <summary>
        /// Determine which outcomes are favorable in the current state and at which offset
        /// </summary>
        public abstract NoteHitDecision Decide(SystemState currentState, NoteHappening note, SimulationSystem system);

        public uint FrameLossSwitching(ButtonState original, ButtonState next)
        {
            if((next & original) != ButtonState.None)
            {
                return FrameLossOnRepress;
            } 
            else if(original != ButtonState.None)
            {
                return FrameLossOnSwitch;
            } 
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// A casual player skill, always hits straight Cool, no switches nothing else
    /// </summary>
    [Serializable]
    class CasualSkill : Skill
    {
        public override NoteHitDecision Decide(SystemState currentState, NoteHappening note, SimulationSystem system)
        {
            NoteHitDecisionKind rslt = NoteHitDecisionKind.Undecided_Invalid;

            if (currentState.HeldButtons == ButtonState.None)
            {
                // We are not holding anything right now, so we can hold anything that comes our way
                rslt = NoteHitDecisionKind.Hit;
            }
            else
            {
                if ((currentState.HeldButtons & note.PressButtons) == ButtonState.None)
                {
                    // We are holding something, but it does not interfere with the incoming note
                    rslt = NoteHitDecisionKind.Hit;
                }
                else if ((currentState.HeldButtons & note.PressButtons) != ButtonState.None)
                {
                    // We are holding something and it DOES interfere with the incoming note
                    rslt |= NoteHitDecisionKind.Switch;
                }
            }

            return new NoteHitDecision { Decisions = rslt, Offset = 0 };
        }
    }

    /// <summary>
    /// A generic player skill, tailored to All Cool routes
    /// </summary>
    [Serializable]
    class GeneralSkill : Skill
    {
        public override NoteHitDecision Decide(SystemState currentState, NoteHappening note, SimulationSystem system)
        {
            NoteHitDecisionKind rslt = NoteHitDecisionKind.Undecided_Invalid;

            if(currentState.HeldButtons == ButtonState.None)
            {
                // We are not holding anything right now, so we can hold anything that comes our way
                rslt = NoteHitDecisionKind.Hit;
            }
            else
            {
                if((currentState.HeldButtons & note.PressButtons) == ButtonState.None)
                {
                    // We are holding something, but it does not interfere with the incoming note
                    
                    if(note.HoldButtons != ButtonState.None)
                    {
                        // Incoming note is a hold
                        // So our two options are either add it, or switch to it
                        rslt |= NoteHitDecisionKind.Hit;
                        rslt |= NoteHitDecisionKind.Switch;
                    }
                    else
                    {
                        // Incoming note is not a hold
                        // Best course of action is to hit it
                        rslt = NoteHitDecisionKind.Hit;
                    }
                }
                else if((currentState.HeldButtons & note.PressButtons) != ButtonState.None)
                {
                    // We are holding something and it DOES interfere with the incoming note

                    if(
                        !ProhibitMisses &&
                        (note.HoldButtons == ButtonState.None || AllowMissHold) //<- never yet seen where missing a hold is better
                    )
                    {
                        // Of course, we could switch
                        rslt |= NoteHitDecisionKind.Switch;

                        // But also if we have buttons to spare
                        if(RuleSet.ButtonTotalCount - currentState.HeldButtonCount >= Util.CountButtons(note.PressButtons))
                        {
                            // We can do a WRONG, since it's better in scoring
                            rslt |= NoteHitDecisionKind.Wrong;
                        }
                        else
                        {
                            // Our last resort is the WORST, because it's worse in life bar and score
                            rslt |= NoteHitDecisionKind.Miss;
                        }
                    } 
                    else
                    {
                        // We are in Prohibit Miss mode, so the only option is to hit/switch and let go of the hold
                        rslt = NoteHitDecisionKind.Switch;
                    }
                }
            }

            return new NoteHitDecision { Decisions = rslt, Offset = 0 };
        }
    }

    /// <summary>
    /// A player with a knowledge of the playthrough
    /// Used to optimize offsets when running a second pass of the solver
    /// </summary>
    [Serializable]
    class KnowledgeablePlayerSkill : Skill
    {
        /// <summary>
        /// Previous playthrough decisions, indexed by `NoteNumber`
        /// </summary>
        public SortedList<uint, NoteHitDecisionKind> Decisions { get; set; }

        /// <summary>
        /// Max note number in the level
        /// </summary>
        public uint MaxNoteNumber { get; set; }

        /// <summary>
        /// Time of the end of the level in ticks
        /// </summary>
        public uint EndOfLevelTime { get; set; }

        public HappeningSet Timeline { get; set; }

        public KnowledgeablePlayerSkill(uint maxNoteNo, uint endTime, SortedList<uint, NoteHitDecisionKind> decisions, HappeningSet timeline)
        {
            MaxNoteNumber = maxNoteNo;
            EndOfLevelTime = endTime;
            Decisions = decisions;
            Timeline = timeline;
        }

        private NoteHitDecisionKind LastPlayDecisionAtNumber(uint num)
        {
            if(!Decisions.ContainsKey(num))
            {
                // No recorded meta means just hit
                return NoteHitDecisionKind.Hit;
            }
            else
            {
                return Decisions[num];
            }
        }

        private NoteHappening NextHittedNoteFromTime(uint time, SystemState state)
        {
            uint noteNo = state.NoteNumber + 1;
            NoteHappening n = Timeline.NextNoteFromTime(time);
            while(n != null && !LastPlayDecisionAtNumber(noteNo).IsHitDecision())
            {
                noteNo += 1;
                n = Timeline.NextNoteFromTime(n.Time);
            }

            return n;
        }

        public override NoteHitDecision Decide(SystemState currentState, NoteHappening note, SimulationSystem system)
        {
            #region Special case: Final Hold Note
            if (
                currentState.NoteNumber == MaxNoteNumber && 
                (EndOfLevelTime - currentState.Time) < system.GameRules.MaxTicksInHold && 
                note.HoldButtons != ButtonState.None &&
                LastPlayDecisionAtNumber(currentState.NoteNumber).IsHitDecision()
              )
            {
                // This is a final hold note, that we are hitting, and it doesn't give MAX until end of level
                // What if we can adjust it enough to fit?

                long nominalScore = note.EstimateScoreAtOffset(0, currentState, system) + 
                    (EndOfLevelTime - currentState.Time - FrameLossSwitching(currentState.HeldButtons, note.PressButtons)) * system.GameRules.HoldBonusFactor;

                long potentialBestScore = nominalScore;
                NoteHitDecision bestCourseOfAction = new NoteHitDecision { Offset = 0, Decisions = LastPlayDecisionAtNumber(currentState.NoteNumber) };

                foreach (var kind in (HitKind[]) Enum.GetValues(typeof(HitKind)))
                {
                    if (kind == HitKind.Wrong || kind == HitKind.Worst) continue;
                    int offset = -system.GameRules.NoteTiming.For(kind);

                    // check for interference with previous note
                    if (currentState.Time + offset <= currentState.LastNoteTime) continue;

                    long potentialScore = note.EstimateScoreAtOffset(offset, currentState, system) +
                    (EndOfLevelTime - (currentState.Time + offset) - FrameLossSwitching(currentState.HeldButtons, note.PressButtons)) * system.GameRules.HoldBonusFactor;

                    if(potentialScore > potentialBestScore)
                    {
                        bestCourseOfAction.Offset = offset;
                        potentialBestScore = potentialScore;
                    }
                }

                if(potentialBestScore != nominalScore)
                {
                    Console.Error.WriteLine("[2PASS] Note {0} can be offset by {1} to get {2}pts > {3}pts", currentState.NoteNumber, bestCourseOfAction.Offset, potentialBestScore, nominalScore);
                }

                return bestCourseOfAction;
            }
            #endregion


            #region Simple hold add-remove rules

            if(
                note.HoldButtons != ButtonState.None &&
                // Case 1.1: Increasing number of holds
                (((note.PressButtons & currentState.HeldButtons) == ButtonState.None &&
                LastPlayDecisionAtNumber(currentState.NoteNumber) == NoteHitDecisionKind.Hit) ||

                // Case 1.2: Switch to a bigger number of holds
                (Util.CountButtons(note.HoldButtons) > Util.CountButtons(currentState.HeldButtons) &&
                 LastPlayDecisionAtNumber(currentState.NoteNumber) == NoteHitDecisionKind.Switch)) &&

                 // BUT only if it's not the last one before a surefire MAX
                 // (thus a new hold, or the time between this and next note is > Max [TODO: check for interfering notes only])
                 (currentState.HeldButtons == ButtonState.None || (currentState.NoteNumber != MaxNoteNumber && 
                   (NextHittedNoteFromTime(note.Time, currentState).Time - currentState.Time) < system.GameRules.MaxTicksInHold))
              )
            {
                // Prefer to go earlier
                // For now stub to early cool, TODO more flexibility
                return new NoteHitDecision { Offset = -system.GameRules.NoteTiming.Cool, Decisions = LastPlayDecisionAtNumber(currentState.NoteNumber) };
            }

            if(
                currentState.HeldButtons != ButtonState.None &&
                (
                    // Case 2.1: Reduce number of holds
                    (Util.CountButtons(note.HoldButtons) < Util.CountButtons(currentState.HeldButtons) &&
                     LastPlayDecisionAtNumber(currentState.NoteNumber) == NoteHitDecisionKind.Switch) ||
                     // Case 2.2 Switch to a bigger number of holds AND it's the last one before a MAX
                     (currentState.HeldButtons != ButtonState.None && (currentState.NoteNumber != MaxNoteNumber &&
                   (NextHittedNoteFromTime(note.Time, currentState).Time - note.Time) >= system.GameRules.MaxTicksInHold))
                )
             )
            {
                // Prefer to go late
                return new NoteHitDecision { Offset = system.GameRules.NoteTiming.Cool, Decisions = LastPlayDecisionAtNumber(currentState.NoteNumber) };
            }

            #endregion


            // No other decision was made
            return new NoteHitDecision { Offset = 0, Decisions = LastPlayDecisionAtNumber(currentState.NoteNumber) };
        }
    }
}
