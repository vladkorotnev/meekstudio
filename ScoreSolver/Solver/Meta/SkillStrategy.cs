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
        /// <summary>
        /// ms in one game frame
        /// </summary>
        private const int ONE_FRAME = 17;

        /// <summary>
        /// If set, prohibit generating WORST and WRONG notes
        /// </summary>
        public bool ProhibitMisses;

        /// <summary>
        /// Check miss/wrong branches for HOLD events
        /// </summary>
        public bool AllowMissHold;

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
    }

    [Serializable]
    class GeneralSkill : Skill
    {
    }
}
