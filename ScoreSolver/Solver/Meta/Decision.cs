﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreSolver
{
    /// <summary>
    /// A user-readable hint of what happened in a certain decision point of the playthrough
    /// </summary>
    [Serializable]
    abstract class DecisionMeta
    {
        public uint Time { get; set; }
        public uint Combo { get; set; }
        public abstract override string ToString();
    }

    /// <summary>
    /// Arbitrary text hint
    /// </summary>
    [Serializable]
    class FreeTextDecisionMeta : DecisionMeta
    {
        public string Text { get; set;  }
        public FreeTextDecisionMeta(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    [Serializable]
    class HitDecisionMeta : DecisionMeta
    {
        public ButtonState Buttons { get; set; }
        public HitDecisionMeta(ButtonState missedButtons)
        {
            Buttons = missedButtons;
        }

        public override string ToString()
        {
            return "PRESS: " + Util.ButtonsToString(Buttons);
        }
    }

    /// <summary>
    /// A hint telling the user some notes were allowed to be missed
    /// </summary>
    [Serializable]
    class MissDecisionMeta : DecisionMeta
    {
        public ButtonState Buttons { get; set; }
        public MissDecisionMeta(ButtonState missedButtons)
        {
            Buttons = missedButtons;
        }

        public override string ToString()
        {
            return "WORST: " + Util.ButtonsToString(Buttons);
        }
    }

    /// <summary>
    /// A hint telling the user some notes need to be intentionally pressed incorrectly
    /// </summary>
    [Serializable]
    class WrongDecisionMeta : DecisionMeta
    {
        public ButtonState Buttons { get; set; }
        public WrongDecisionMeta(ButtonState missedButtons)
        {
            Buttons = missedButtons;
        }

        public override string ToString()
        {
            return "WRONG: " + Util.ButtonsToString(Buttons);
        }
    }

    /// <summary>
    /// A hint telling the user another button was added to the currently held buttons
    /// </summary>
    [Serializable]
    class HoldDecisionMeta : DecisionMeta
    {
        public ButtonState Buttons { get; set; }
        public HoldDecisionMeta(ButtonState heldButtons)
        {
            Buttons = heldButtons;
        }

        public override string ToString()
        {
            return "HOLD: +" + Util.ButtonsToString(Buttons);
        }
    }

    /// <summary>
    /// A hint telling the user the previous hold was lifted in favor of pressing the new hold
    /// </summary>
    [Serializable]
    class SwitchDecisionMeta : DecisionMeta
    {
        public ButtonState OldButtons { get; set; }
        public ButtonState NewButtons { get; set; }

        public SwitchDecisionMeta(ButtonState old, ButtonState next)
        {
            OldButtons = old;
            NewButtons = next;
        }

        public override string ToString()
        {
            return Util.ButtonsToString(OldButtons) + " -> " + Util.ButtonsToString(NewButtons);
        }
    }
}