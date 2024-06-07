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

        public SystemState PassTime(SystemState nextState, uint newTime)
        {
            if(newTime < nextState.Time) throw new ArgumentException("Time machines are forbidden at the arcade");

            // If we are holding something
            if (nextState.HeldButtons != ButtonState.None)
            {
                // Add the hold bonus up to this point
                double holdDurTotal = Math.Min(newTime - nextState.HoldStartTime, GameRules.MaxTicksInHold);
                double holdDurSinceLastCalc = Math.Min(newTime - nextState.LastHoldRecalcTime, GameRules.MaxTicksInHold - (nextState.LastHoldRecalcTime - nextState.HoldStartTime));

                int holdDurFrames = (int)Math.Round(holdDurSinceLastCalc / GameRules.TicksPerFrame);
                long holdBonusPts = holdDurFrames * GameRules.HoldBonusFactor * nextState.HeldButtonCount;
                nextState.Score += holdBonusPts;
                nextState.HoldBonus += holdBonusPts;

                // Add max hold bonus and release buttons (because there is no point in holding them more)
                if (holdDurTotal == GameRules.MaxTicksInHold)
                {
                    long maxHoldBonusPts = GameRules.MaxHoldBonus * nextState.HeldButtonCount;
                    nextState.HeldButtons = ButtonState.None;
                    
                    nextState.Score += maxHoldBonusPts;
                    nextState.HoldBonus += maxHoldBonusPts;
                }

                nextState.LastHoldRecalcTime = newTime;
            }

            nextState.Time = newTime;

            return nextState;
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
            Console.Error.WriteLine("[REFS] AllCool={0}", allCool);

            var hasChanceTime = GameRules.Difficulty == Difficulty.Easy || GameRules.Difficulty == Difficulty.Normal;

            var lifeBonusAppliedNoteCount = hasChanceTime ?
                ( // EASY or NORMAL
                    timeline.Events
                        .TakeWhile(evt => !(evt is ChallengeChangeHappening && ((ChallengeChangeHappening)evt).IsChallenge)) // all notes before challenge begins
                        .Concat(
                            timeline.Events
                                .SkipWhile(evt => !(evt is ChallengeChangeHappening && ((ChallengeChangeHappening)evt).IsChallenge)) // skip until start of challenge
                                .SkipWhile(evt => !(evt is ChallengeChangeHappening && !((ChallengeChangeHappening)evt).IsChallenge)) // skip until end of challenge
                        ) // all notes after challenge ends
                        .Where(x => x is NoteHappening).Count() // <- All notes outside of challenge = all notes providing life
                        -
                        ((SystemState.MAX_LIFE - SystemState.DEFAULT_LIFE) / GameRules.LifeScore.Correct.Cool)
                ) : (
                    // Others
                    timeline.Events
                        .Where(x => x is NoteHappening).Count() // <- All notes outside of challenge = all notes providing life
                        -
                        ((SystemState.MAX_LIFE - SystemState.DEFAULT_LIFE) / GameRules.LifeScore.Correct.Cool)
                );

            Console.Error.WriteLine("[REFS] LifeBonusNoteCount={0}", lifeBonusAppliedNoteCount);

            long lifeBonus = lifeBonusAppliedNoteCount * GameRules.LifeBonus;
            Console.Error.WriteLine("[REFS] LifeBonus={0}", lifeBonus);

            long comboBonus = timeline.Events
                .Where(x => x is NoteHappening)
                .Select((_, index) => (long) GameRules.ComboBonus((uint)index + 1))
                .Sum();
            Console.Error.WriteLine("[REFS] ComboBonus={0}", comboBonus);


            long chanceBonus = 0;
            int chanceCount = 0;

            if(hasChanceTime)
            {
                int preChanceStartCombo = timeline.Events
                    .TakeWhile(evt => !(evt is ChallengeChangeHappening && ((ChallengeChangeHappening)evt).IsChallenge))
                    .Where(x => x is NoteHappening)
                    .Count();
                chanceCount = timeline.Events
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
            }

            Console.Error.WriteLine("[REFS] ChanceBonus={0} (ChanceCount={1})", chanceBonus, chanceCount);

            RefScore = allCool + chanceBonus + lifeBonus + comboBonus;
            Console.Error.WriteLine("[REFS] TOTAL REFS={0}", RefScore);

            if (RefScore < 0) RefScore = 0;
        }

        public TimeOffsetDecisionMeta DecideNoteTimingByOffset(int timeOffset)
        {
            int window = Math.Abs(timeOffset);
            HitKind kind = HitKind.Worst;

            if (window <= GameRules.NoteTiming.Cool)
            {
                kind = HitKind.Cool;
            }
            else if (window <= GameRules.NoteTiming.Fine)
            {
                kind = HitKind.Fine;
            }
            else if (window <= GameRules.NoteTiming.Safe)
            {
                kind = HitKind.Safe;
            }
            else if (window <= GameRules.NoteTiming.Sad)
            {
                kind = HitKind.Sad;
            }

            return new TimeOffsetDecisionMeta(kind, timeOffset);
        }
    }

    /// <summary>
    /// A game system (machine + player + level) state
    /// </summary>
    [Serializable]
    class SystemState
    {
        public const int DEFAULT_LIFE = 127;
        public const int MAX_LIFE = 255;
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
        public int Life = DEFAULT_LIFE;
        /// <summary>
        /// Currently achieved attain
        /// </summary>
        public int Attain = 0;
        /// <summary>
        /// Current time
        /// </summary>
        public uint Time = 0;

        /// <summary>
        /// Route identifier for internal tracking
        /// </summary>
        public uint RouteId = 0;


        private List<DecisionMeta> _decisionRecord = new List<DecisionMeta>();

        public void RecordDecision(DecisionMeta d)
        {
            d.Time = Time;
            d.Combo = Combo;
            _decisionRecord.Add(d);
        }

        public List<DecisionMeta> DecisionRecord {  get { return _decisionRecord; } }


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
            copy.RouteId = RouteId;
            copy._decisionRecord = new List<DecisionMeta>(_decisionRecord); // <- shallow copy oughtta be enough 'cause meta is supposedly immutable
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

        public void AccreditLife(int add, RuleSet rules)
        {
            if (IsInChanceTime) return;

            if(add > 0)
            {
                if(Life < MAX_LIFE)
                {
                    Life += add;
                    if (Life > MAX_LIFE) Life = MAX_LIFE;
                } 
                else
                {
                    Score += rules.LifeBonus;
                }
            } 
            else if (add < 0)
            {
                Life += add;
                if(Life < rules.SafetyLevel && Time < rules.SafetyDuration)
                {
                    Life = rules.SafetyLevel;
                }
            }
        }
    }
}
