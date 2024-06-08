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

        public KnowledgeablePlayerSkill(uint maxNoteNo, uint endTime, SortedList<uint, NoteHitDecisionKind> decisions)
        {
            MaxNoteNumber = maxNoteNo;
            EndOfLevelTime = endTime;
            Decisions = decisions;
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

        public override NoteHitDecision Decide(SystemState currentState, NoteHappening note, SimulationSystem system)
        {
            if(
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

            // Simple POC: early catch hold start, late catch old end
            if(
                note.HoldButtons != ButtonState.None &&
                currentState.HeldButtons == ButtonState.None &&
                LastPlayDecisionAtNumber(currentState.NoteNumber) == NoteHitDecisionKind.Hit
            )
            {
                // Hold starting but not switching or adding, do it early
                return new NoteHitDecision { Offset = -system.GameRules.NoteTiming.Cool, Decisions = NoteHitDecisionKind.Hit };
            }

            if (
                note.HoldButtons == ButtonState.None &&
                (currentState.HeldButtons & note.PressButtons) != ButtonState.None &&
                LastPlayDecisionAtNumber(currentState.NoteNumber) == NoteHitDecisionKind.Hit
            )
            {
                // Hold ending but not switching, do it late
                return new NoteHitDecision { Offset = system.GameRules.NoteTiming.Cool, Decisions = NoteHitDecisionKind.Hit };
            }

            return new NoteHitDecision { Offset = 0, Decisions = LastPlayDecisionAtNumber(currentState.NoteNumber) };
        }
    }
}
