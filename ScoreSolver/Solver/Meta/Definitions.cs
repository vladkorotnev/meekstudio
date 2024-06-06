using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreSolver
{
    

    /// <summary>
    /// Level Difficulty
    /// </summary>
    enum Difficulty : int
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Extreme = 3,
        Encore = 4
    }

    /// <summary>
    /// Scores for various hit types
    /// </summary>
    [Serializable]
    struct HitScore
    {
        public int Cool;
        public int Fine;
        public int Safe;
        public int Sad;
        public int Worst;
    }

    /// <summary>
    /// Scores depending whether a correct one or wrong one was hit
    /// </summary>
    [Serializable]
    struct HitScoreSet
    {
        public HitScore Correct;
        public HitScore Wrong;
    }

    /// <summary>
    /// Defines the timing windows in milliseconds
    /// </summary>
    [Serializable]
    struct NoteTimingSet
    {
        public int Cool;
        public int Fine;
        public int Safe;
        public int Sad;
    }

    /// <summary>
    /// A description of game rules
    /// </summary>
    [Serializable]
    abstract class RuleSet
    {
        /// <summary>
        /// Total buttons on the system
        /// </summary>
        public const int ButtonTotalCount = 4;

        public Difficulty Difficulty;

        /// <summary>
        /// Scores given per hitting 1 note/button
        /// </summary>
        public HitScoreSet ButtonScore;

        public NoteTimingSet NoteTiming;

        /// <summary>
        /// Life point given per hitting 1 note/button
        /// </summary>
        public HitScoreSet LifeScore;

        /// <summary>
        /// Bonus given for hitting note when life ix max
        /// </summary>
        public int LifeBonus;

        /// <summary>
        /// Life point freeze time from start of play
        /// </summary>
        public int SafetyDuration;

        /// <summary>
        /// Life point freeze point during <see cref="SafetyDuration"/>
        /// </summary>
        public int SafetyLevel;

        /// <summary>
        /// Minimum required attain percent points to pass
        /// </summary>
        public double ClearAttain;
        /// <summary>
        /// Scale factor for holds in attain percent points
        /// </summary>
        public int HoldAttainScaleFactor;

        /// <summary>
        /// Simulation ticks per frame of gameplay
        /// </summary>
        public double TicksPerFrame = 16.667; // 60 FPS
        /// <summary>
        /// Simulation ticks after which to stop counting and give <see cref="MaxHoldBonus"/>
        /// </summary>
        public int MaxTicksInHold;
        /// <summary>
        /// Bonus to give after holding for <see cref="MaxTicksInHold"/>, per 1 note/button
        /// </summary>
        public int MaxHoldBonus;
        /// <summary>
        /// Bonus to give per every tick of holding
        /// </summary>
        public int HoldBonusFactor;

        /// <summary>
        /// Bonus to give per every chain slide note segment
        /// </summary>
        public int SlideSegmentBonus;
        /// <summary>
        /// Max segment cap for <see cref="SlideSegmentBonus"/>
        /// </summary>
        public int SlideSegmentMaxCount;
        /// <summary>
        /// Bonus to give when max chain slide is reached
        /// </summary>
        public int MaxChainBonus;

        /// <summary>
        /// Max note count cap for the Chance Time bonus
        /// </summary>
        public int MaxChanceCombo;
        /// <summary>
        /// Bonus factor for the Chance Time bonus
        /// </summary>
        public int ChanceComboFactor;
        /// <summary>
        /// Whether chance time even exists
        /// </summary>
        public bool AllowChanceTime;

        /// <summary>
        /// Minimum combo to start giving combo bonus
        /// </summary>
        public int ComboBonusMinCombo;
        /// <summary>
        /// Max combo to consider in combo bonus
        /// </summary>
        public uint ComboBonusMaxCombo;
        /// <summary>
        /// Combo count divisor in combo bonus
        /// </summary>
        public uint ComboBonusDivisor;
        /// <summary>
        /// Combo count multiplier in combo bonus
        /// </summary>
        public uint ComboBounsMultiplier;

        /// <summary>
        /// Calculate the Combo Bonus for hitting a note
        /// </summary>
        /// <param name="currentCombo">Combo count (including current note)</param>
        /// <returns>Combo bonus points</returns>
        public uint ComboBonus(uint currentCombo)
        {
            if (currentCombo < ComboBonusMinCombo) return 0;
            return (Math.Min(currentCombo, ComboBonusMaxCombo) / ComboBonusDivisor) * ComboBounsMultiplier;
        }
    }

    /// <summary>
    /// Game rules for the Future Tone version
    /// </summary>
    [Serializable]
    class FutureToneRuleSet : RuleSet
    {
        public FutureToneRuleSet(Difficulty difficulty, int playthroughTime = 1)
        {
            if (playthroughTime < 1 || playthroughTime > 3) throw new ArgumentOutOfRangeException("playthroughTime", "Playthrough count can be 1, 2 or 3");

            Difficulty = difficulty;

            ComboBonusMinCombo = 10;
            ComboBonusMaxCombo = 50;
            ComboBonusDivisor = 10;
            ComboBounsMultiplier = 50;

            MaxHoldBonus = 1500;
            MaxTicksInHold = 5000;
            HoldBonusFactor = 10;

            SlideSegmentBonus = 10;
            SlideSegmentMaxCount = 50;
            MaxChainBonus = 1000;

            MaxChanceCombo = 50;
            ChanceComboFactor = 10;
            LifeBonus = 10;

            ButtonScore = new HitScoreSet
            {
                Correct = new HitScore
                {
                    Cool = 500,
                    Fine = 300,
                    Safe = 100,
                    Sad = 50,
                    Worst = 0
                },
                Wrong = new HitScore
                {
                    Cool = 250,
                    Fine = 150,
                    Safe = 50,
                    Sad = 30,
                    Worst = 0
                }
            };

            NoteTiming = new NoteTimingSet
            {
                Cool = 30,
                Fine = 70,
                Safe = 100,
                Sad = 130
            };

            SafetyLevel = 76;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    HoldAttainScaleFactor = 400;
                    SafetyDuration = (playthroughTime == 1 ? 60000 : 50000);
                    ClearAttain = 30.00;
                    AllowChanceTime = true;
                    LifeScore = new HitScoreSet
                    {
                        Correct = new HitScore
                        {
                            Cool = 3,
                            Fine = 2,
                            Safe = 1,
                            Sad = -10,
                            Worst = -20
                        },
                        Wrong = new HitScore
                        {
                            Cool = 0,
                            Fine = -1,
                            Safe = -3,
                            Sad = -15,
                            Worst = -20
                        }
                    };
                    break;
                case Difficulty.Normal:
                    HoldAttainScaleFactor = 200;
                    SafetyDuration = (playthroughTime == 1 ? 40000 : 30000);
                    AllowChanceTime = true;
                    ClearAttain = 50.00;
                    LifeScore = new HitScoreSet
                    {
                        Correct = new HitScore
                        {
                            Cool = 2,
                            Fine = 1,
                            Safe = 0,
                            Sad = -10,
                            Worst = -20
                        },
                        Wrong = new HitScore
                        {
                            Cool = 0,
                            Fine = -1,
                            Safe = -3,
                            Sad = -15,
                            Worst = -20
                        }
                    };
                    break;
                case Difficulty.Hard:
                    HoldAttainScaleFactor = 50;
                    SafetyDuration = 30000;
                    AllowChanceTime = false;
                    ClearAttain = 60.00;
                    LifeScore = new HitScoreSet
                    {
                        Correct = new HitScore
                        {
                            Cool = 2,
                            Fine = 1,
                            Safe = 0,
                            Sad = -10,
                            Worst = -20
                        },
                        Wrong = new HitScore
                        {
                            Cool = -3,
                            Fine = -6,
                            Safe = -9,
                            Sad = -15,
                            Worst = -20
                        }
                    };
                    break;
                case Difficulty.Extreme:
                    HoldAttainScaleFactor = 20;
                    SafetyDuration = 30000;
                    AllowChanceTime = false;
                    ClearAttain = 70.00;
                    LifeScore = new HitScoreSet
                    {
                        Correct = new HitScore
                        {
                            Cool = 2,
                            Fine = 1,
                            Safe = 0,
                            Sad = -15,
                            Worst = -25
                        },
                        Wrong = new HitScore
                        {
                            Cool = -6,
                            Fine = -8,
                            Safe = -10,
                            Sad = -20,
                            Worst = -25
                        }
                    };
                    break;
                case Difficulty.Encore:
                    HoldAttainScaleFactor = 0;
                    SafetyDuration = 30000;
                    AllowChanceTime = false;
                    ClearAttain = 80.00;
                    LifeScore = new HitScoreSet
                    {
                        Correct = new HitScore
                        {
                            Cool = 1,
                            Fine = 1,
                            Safe = 0,
                            Sad = -15,
                            Worst = -30
                        },
                        Wrong = new HitScore
                        {
                            Cool = -6,
                            Fine = -8,
                            Safe = -10,
                            Sad = -20,
                            Worst = -30
                        }
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException("difficulty");
            }
        }

        
    }
    [Serializable]
    class HoldlessFTRuleSet : FutureToneRuleSet
    {
        public HoldlessFTRuleSet(Difficulty difficulty, int playthroughTime = 1) : base(difficulty, playthroughTime)
        {
            HoldBonusFactor = 0;
            MaxHoldBonus = 0;
            MaxTicksInHold = 0;
        }
    }
}
