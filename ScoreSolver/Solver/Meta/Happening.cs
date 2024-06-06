using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreSolver
{
    /// <summary>
    /// A game event that changes the state of the system
    /// </summary>
    [Serializable]
    abstract class Happening
    {
        /// <summary>
        /// Time when the event occurs
        /// </summary>
        public uint Time = 0;

        /// <summary>
        /// Get all viable outcomes of the situation after this event happens
        /// </summary>
        /// <param name="currentState">Current system state</param>
        /// <param name="system">System globals</param>
        /// <returns>New system states</returns>
        public abstract List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system);

        internal Happening(uint eventTime)
        {
            Time = eventTime;
        }
    }

    /// <summary>
    /// A game event designating some time has passed
    /// </summary>
    [Serializable]
    class TimePassageHappening : Happening
    {
        public TimePassageHappening(uint evtTime) : base(evtTime) { }

        public override List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system)
        {
            return new List<SystemState>() { system.PassTime(currentState, Time) };
        }
    }

    /// <summary>
    /// An event designating it's time to perform a comparison of scores and keep only the max one
    /// </summary>
    [Serializable]
    class OptimizationCheckpointHappening : TimePassageHappening
    {
        public uint CheckpointID { get; private set; }
        public OptimizationCheckpointHappening(uint time, uint id) : base(time) {
            CheckpointID = id;
        }
    }

    /// <summary>
    /// A game event designating start/end of challenge time
    /// </summary>
    [Serializable]
    class ChallengeChangeHappening : Happening
    {
        public bool IsChallenge;
        public ChallengeChangeHappening(uint evtTime, bool isStart) : base(evtTime) 
        {
            IsChallenge = isStart;
        }

        public override List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system)
        {
            var lst = new List<SystemState> { currentState.Clone() };
            if(system.GameRules.AllowChanceTime)
            {
                // Update chance values
                lst[0].IsInChanceTime = IsChallenge;
                lst[0].LastDecisionMeta = new FreeTextDecisionMeta("CHANCE TIME " + (IsChallenge ? "START" : "END"));
            }
            return lst;
        }
    }

    /// <summary>
    /// A game event designating it's time to press (and optionally hold) a note button
    /// </summary>
    [Serializable]
    class NoteHappening : Happening
    {
        static uint RouteIdCounter = 0;

        /// <summary>
        /// Buttons player need to press
        /// </summary>
        public ButtonState PressButtons { get; private set; }
        /// <summary>
        /// Buttons player can continue holding
        /// </summary>
        public ButtonState HoldButtons { get; private set; }

        public NoteHappening(uint evtTime, ButtonState noteButtons, ButtonState holdButtons) : base(evtTime)
        {
            if((noteButtons & holdButtons) == ButtonState.None && holdButtons != ButtonState.None)
            {
                throw new ArgumentException("holdButtons and noteButtons were provided as unrelated values.");
            }
            if(holdButtons.HasFlag(ButtonState.Arrow1) || holdButtons.HasFlag(ButtonState.Arrow2))
            {
                throw new ArgumentException("Cannot hold arrows");
            }

            PressButtons = noteButtons;
            HoldButtons = holdButtons;
        }

        public override List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system)
        {
            var variants = new List<SystemState>();

            // POC: late cool if holding
            // TODO: proper logic across all other types, i.e. lookback and lookahead
             if(currentState.HeldButtons != ButtonState.None)
             {
                 variants.AddRange(FindOutcomesAtTimeOffset(currentState, system, system.GameRules.NoteTiming.Cool - 1));
             }
             else
             {
                 variants.AddRange(FindOutcomesAtTimeOffset(currentState, system, -(system.GameRules.NoteTiming.Cool - 1)));
             } //*/

            //variants.AddRange(FindOutcomesAtTimeOffset(currentState, system, 0));

            // Increment route IDs to allow to kill off bad routes when checkpoints are hit
            for (int i = 0; i < variants.Count; i++)
            {
                RouteIdCounter += 1;
                variants[i].RouteId = RouteIdCounter;
                if(variants[i].Time < Time)
                {
                    // align early variants to current time to avoid the solver running into this event again
                    variants[i] = system.PassTime(variants[i], Time);
                }
            }

            return variants;
        }

        protected List<SystemState> FindOutcomesAtTimeOffset(SystemState initialState, SimulationSystem system, int timeOffset)
        {
            var variants = new List<SystemState>();
            var currentState = system.PassTime(initialState, (uint) (Time + timeOffset));

            if (currentState.HeldButtons == ButtonState.None)
            {
                // we aren't holding anything, so hit and hold anything that arrives
                // and consider that the only option
                variants.Add(NewStateForHitAndHold(currentState, system, false));
            }
            else
            {
                // we are holding something so ...
                if ((currentState.HeldButtons & PressButtons) == ButtonState.None)
                {
                    // Option 1. No interference between currently held button and incoming, and no hold needed

                    // Option 1.1. just hit it as is, optionally holding
                    variants.Add(NewStateForHitAndHold(currentState, system, false));

                    if (HoldButtons != ButtonState.None)
                    {
                        // Option 1.2. if it has hold, switch to new hold
                        variants.Add(NewStateForHitAndHold(currentState, system, true));
                    }
                }
                else if ((currentState.HeldButtons & PressButtons) != ButtonState.None)
                {
                    // Option 2. There IS interference between buttons to press and currently held

                    // Option 2.1. release old ones and press new ones
                    variants.Add(NewStateForHitAndHold(currentState, system, true));

                    if (!system.PlayerSkill.ProhibitMisses)
                    {
                        // if we have enough empty buttons to press...
                        if (RuleSet.ButtonTotalCount - Util.CountButtons(currentState.HeldButtons) >= Util.CountButtons(PressButtons))
                        {
                            // Option 2.3. hit wrong buttons
                            variants.Add(NewStateForWrong(currentState, system));
                        }
                        else
                        {
                            // option 2.2 moved, only when not enough buttons -- completely miss
                            // because Wrong is better in both Lifebar and Score than Worst
                            variants.Add(NewStateForMiss(currentState, system));
                        }
                    }

                }
                else
                {
                    throw new Exception("Uh oh! Someone forgot something when designing this engine.");
                }
            }
            return variants;
        }

        /// <summary>
        /// Singular hit state transition for all incoming notes
        /// </summary>
        protected SystemState NewStateForJustHitting(SystemState currentState_, SimulationSystem system)
        {
            var currentState = currentState_.Clone();

            // add button score
            currentState.Score += system.PlayerSkill.PickScore(system.GameRules.ButtonScore.Correct) * Util.CountButtons(PressButtons);
            // add button life
            currentState.AccreditLife(system.PlayerSkill.PickScore(system.GameRules.LifeScore.Correct), system.GameRules);

            // increase combo
            currentState.Combo += 1;
            // add combo bonus
            currentState.Score += system.GameRules.ComboBonus(currentState.Combo);

            // add chance bonus
            if (currentState.IsInChanceTime)
            {
                currentState.Score += Math.Min(system.GameRules.MaxChanceCombo, currentState.Combo) * system.GameRules.ChanceComboFactor;
            }

            return currentState;
        }

        /// <summary>
        /// Hit state transition for all incoming notes, which also will hold the newly incoming hold notes
        /// </summary>
        /// <param name="resetHold">Whether to release currently held notes before pressing incoming ones</param>
        private SystemState NewStateForHitAndHold(SystemState currentState, SimulationSystem system, bool resetHold)
        {
            var nextState = NewStateForJustHitting(currentState, system);

            // start holding if anything to hold is present
            if(resetHold)
            {
                // remove old hold, add new hold, keep record of "switch over" if needed
                if(nextState.HeldButtons != ButtonState.None)
                {
                    nextState.LastDecisionMeta = new SwitchDecisionMeta(nextState.HeldButtons, HoldButtons);
                    if((nextState.HeldButtons & HoldButtons) != ButtonState.None)
                    {
                        // there will be a repress in this switchover
                        // account for point loss in the re-press
                        nextState.Time += system.PlayerSkill.FrameLossOnRepress;
                    } 
                    else
                    {
                        // just a switchover
                        // account for point loss in the switchover
                        nextState.Time += system.PlayerSkill.FrameLossOnSwitch;
                    }
                }
                nextState.HeldButtons = HoldButtons;
            }
            else
            {
                // add new hold button to old ones
                if(nextState.HeldButtons != ButtonState.None && HoldButtons != ButtonState.None)
                {
                   // nextState.LastDecisionMeta = new HoldDecisionMeta(HoldButtons);
                }
                nextState.HeldButtons |= HoldButtons;
            }

            // reset hold timer counter if needed
            if (HoldButtons != ButtonState.None)
            {
                nextState.HoldStartTime = nextState.Time;
                nextState.LastHoldRecalcTime = nextState.HoldStartTime;
            }

            return nextState;
        }

        /// <summary>
        /// State transition for missing all notes of this event completely
        /// </summary>
        private SystemState NewStateForMiss(SystemState currentState_, SimulationSystem system)
        {
            var currentState = currentState_.Clone();
            currentState.LastDecisionMeta = new MissDecisionMeta(PressButtons);


            // add button score
            currentState.Score += system.GameRules.ButtonScore.Correct.Worst * Util.CountButtons(PressButtons);
            // add button life
            currentState.AccreditLife(system.GameRules.LifeScore.Correct.Worst, system.GameRules);

            // remove combo
            currentState.Combo = 0;

            return currentState;
        }

        /// <summary>
        /// State transition for pressing a wrong button of this event
        /// </summary>
        private SystemState NewStateForWrong(SystemState currentState_, SimulationSystem system)
        {
            var currentState = currentState_.Clone();
            currentState.LastDecisionMeta = new WrongDecisionMeta(PressButtons);


            // add button score
            currentState.Score += system.PlayerSkill.PickScore(system.GameRules.ButtonScore.Wrong) * Util.CountButtons(PressButtons);
            if(!currentState.IsInChanceTime)
            {
                // add button life
                currentState.Life += system.PlayerSkill.PickScore(system.GameRules.LifeScore.Wrong);
                if (currentState.Life < system.GameRules.SafetyLevel && Time < system.GameRules.SafetyDuration)
                {
                    currentState.Life = system.GameRules.SafetyLevel;
                }
            }

            // remove combo
            currentState.Combo = 0;

            return currentState;
        }
    }

    /// <summary>
    /// An event designating the end of play, thus the end of simulation
    /// </summary>
    [Serializable]
    class EndOfLevelHappening : Happening
    {
        public EndOfLevelHappening(uint evtTime) : base(evtTime) { }

        public override List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system)
        {
            var st = system.PassTime(currentState, Time);
            st.IsFinal = true;
            st.Time = Time;
            return new List<SystemState>() { st };
        }
    }

    /// <summary>
    /// An event designating it's time to slide an arrow. By default assumes max chain, including bonus.
    /// </summary>
    [Serializable]
    class ArrowHappening : NoteHappening
    {
        public int SegmentCount { get; private set; }
        public int ArrowCount { get; private set; }
        public ArrowHappening(uint evtTime, int chainLength, int arrowCount) 
            : base(evtTime, arrowCount == 2 ? (ButtonState.Arrow1 | ButtonState.Arrow2) : ButtonState.Arrow1, ButtonState.None) {
            if (arrowCount > 1 && chainLength > 0) throw new ArgumentException("It's either chain slide or multi slide, not both");
            if (arrowCount > 2 || arrowCount < 0) throw new ArgumentOutOfRangeException("arrowCount");
            ArrowCount = arrowCount;
            SegmentCount = chainLength;
        }

        public override List<SystemState> GetPotentialOutcomes(SystemState currentState, SimulationSystem system)
        {
            SystemState newState = base.NewStateForJustHitting(currentState, system);
            long slideBonus = Math.Min(system.GameRules.SlideSegmentMaxCount, SegmentCount+1) * system.GameRules.SlideSegmentBonus; // Segment bonus
            if(SegmentCount > 0)
            {
                slideBonus += system.GameRules.MaxChainBonus; // Assuming max chain
            }

            currentState.SlideBonus += slideBonus;
            currentState.Score += slideBonus;
            return new List<SystemState>() { newState };
        }
    }

}
