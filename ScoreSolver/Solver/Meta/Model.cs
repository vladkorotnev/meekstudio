using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ScoreSolver
{
    /// <summary>
    /// Current input panel states
    /// </summary>
    [Flags]
    enum ButtonState
    {
        None = 0,
        Triangle = 1 << 0,
        Circle = 1 << 1,
        Square = 1 << 2,
        Cross = 1 << 3,
        Arrow1 = 1<<6,
        Arrow2 = 1<<7
    }

    /// <summary>
    /// A game system (machine + player + level)
    /// </summary>
    [Serializable]
    class SimulationSystem
    {
        /// <summary>
        /// Game rules in effect
        /// </summary>
        public RuleSet GameRules;
        /// <summary>
        /// Reference score for level now in play
        /// </summary>
        public double RefScore;
        /// <summary>
        /// Player skills strategy
        /// </summary>
        public Skill PlayerSkill;

        public SimulationSystem(int refScore, Skill skill, RuleSet rules)
        {
            GameRules = rules;
            RefScore = refScore;
            PlayerSkill = skill;
        }

        public double AttainPoint(SystemState forState)
        {
            double noteAtn; 

            long tricklessScore = forState.Score - forState.SlideBonus - forState.HoldBonus;
            if (tricklessScore < 0) tricklessScore = 0;

            if (tricklessScore == RefScore) noteAtn = 100.0;
            else noteAtn = tricklessScore / RefScore * 100.0;

            double holdAtn = forState.HoldBonus / RefScore * GameRules.HoldAttainScaleFactor;
            if (holdAtn > 5.0) holdAtn = 5.0;

            return Math.Round(noteAtn + holdAtn, 2);
        }

        public void SetRefscoreFromTimeline(HappeningSet timeline)
        {
            long allCool = timeline.Events.Where(evt => evt is NoteHappening).Select(x => Util.CountButtons(((NoteHappening)x).PressButtons) * GameRules.ButtonScore.Correct.Cool).Sum();
            // allCool already excludes MULTIs since there is only one NoteHappening for each multi

            long chanceBonus = 0;

            int preChanceStartCombo = timeline.Events
            .TakeWhile(evt => !(evt is ChallengeChangeHappening && ((ChallengeChangeHappening)evt).IsChallenge))
            .Where(x => x is NoteHappening)
            .Count();
            int chanceCount = timeline.Events
                .SkipWhile(evt => !(evt is ChallengeChangeHappening && ((ChallengeChangeHappening)evt).IsChallenge))
                .TakeWhile(evt => !(evt is ChallengeChangeHappening && !((ChallengeChangeHappening)evt).IsChallenge))
                .Where(x => x is NoteHappening).Count();
            if (chanceCount > 0)
            {
                if (preChanceStartCombo < 50)
                {
                    chanceBonus = chanceCount * 500 - 12250;
                }
                else
                {
                    chanceBonus = 5 * chanceCount * (chanceCount + 1);
                }
            }

            RefScore = allCool + chanceBonus;
            if (RefScore < 0) RefScore = 0;
        }
    }

    /// <summary>
    /// A game system (machine + player + level) state
    /// </summary>
    [Serializable]
    class SystemState
    {
        public bool IsFinal = false;

        /// <summary>
        /// Buttons currently being held
        /// </summary>
        public ButtonState HeldButtons = ButtonState.None;
        /// <summary>
        /// When did the hold start
        /// </summary>
        public uint HoldStartTime = 0;
        /// <summary>
        /// When was the hold bonus recalculated last time
        /// </summary>
        public uint LastHoldRecalcTime = 0;


        /// <summary>
        /// Whether chance time is currently in action
        /// </summary>
        public bool IsInChanceTime = false;

        /// <summary>
        /// Overall current combo
        /// </summary>
        public uint Combo = 0;

        /// <summary>
        /// Currently achieved score
        /// </summary>
        public long Score = 0;

        /// <summary>
        /// Hold bonus score granted so far
        /// </summary>
        public long HoldBonus = 0;
        /// <summary>
        /// Slide bonus score granted so far
        /// </summary>
        public long SlideBonus = 0;

        /// <summary>
        /// Current life bar
        /// </summary>
        public int Life = 127;
        /// <summary>
        /// Currently achieved attain
        /// </summary>
        public int Attain = 0;
        /// <summary>
        /// Current time
        /// </summary>
        public uint Time = 0;
        
        /// <summary>
        /// Last time decision hint
        /// </summary>
        public DecisionMeta LastDecisionMeta
        {
            get
            {
                return _decisionMeta;
            }
            set
            {
                _decisionMeta = value;
                if(_decisionMeta != null)
                {
                    _decisionMeta.Time = Time;
                    _decisionMeta.Combo = Combo;
                }
            }
        }
        private DecisionMeta _decisionMeta;

        public SystemState() { }

        public SystemState Clone()
        {
            SystemState copy = new SystemState();
            copy.IsFinal = IsFinal;
            copy.HeldButtons = HeldButtons;
            copy.HoldStartTime = HoldStartTime;
            copy.LastHoldRecalcTime = LastHoldRecalcTime;
            copy.IsInChanceTime = IsInChanceTime;
            copy.Combo = Combo;
            copy.Score = Score;
            copy.SlideBonus = SlideBonus;
            copy.HoldBonus = HoldBonus;
            copy.Life = Life;
            copy.Attain = Attain;
            copy.Time = Time;
            return copy;
        }
        
        /// <summary>
        /// Number of currently busy buttons
        /// </summary>
        public uint HeldButtonCount
        {
            get
            {
                return Util.CountButtons(HeldButtons);
            }
        }

        /// <summary>
        /// Whether the state is a guaranteed failure that shouldn't be processed further
        /// </summary>
        public bool IsFailed
        {
            get
            {
                return Life <= 0;
            }
        }
    }
}
