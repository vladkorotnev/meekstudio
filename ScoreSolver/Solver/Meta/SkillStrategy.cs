using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreSolver
{
    /// <summary>
    /// A player skill, containing score picking logic
    /// TBD, for now always assuming COOL when possible
    /// </summary>
    [Serializable]
    abstract class Skill
    {
        public abstract int PickScore(HitScore scores);

        /// <summary>
        /// If set, prohibit generating WORST and WRONG notes
        /// </summary>
        public bool ProhibitMisses;
    }

    /// <summary>
    /// A player skill where only COOL is being hit
    /// </summary>
    [Serializable]
    class AllCoolSkill : Skill
    {
        public override int PickScore(HitScore scores)
        {
            return scores.Cool;
        }
    }
}
