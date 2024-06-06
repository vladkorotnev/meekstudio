using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuASM;
using MikuASM.Commands;

namespace ScoreSolver
{
    /// <summary>
    /// Target IDs (FT)
    /// </summary>
    enum TargetID : int
    {
        Triangle = 0,
        Circle = 1,
        Cross = 2,
        Square = 3,

        TriHold = 4,
        CirHold = 5,
        CrossHold = 6,
        SquHold = 7,

        Random = 8,
        RandomHold = 9,
        Same = 10,

        SlideL = 12,
        SlideR = 13,
        SlideBitL = 15,
        SlideBitR = 16,

        TriChance = 18,
        CirChance = 19,
        CrossChance = 20,
        SquChance = 21,
        SlideLChance = 23,
        SlideRChance = 24
    }
    class DSCToTimelineReader
    {
        public HappeningSet TimelineOfDsc(string fname)
        {
            return TimelineOfDsc(DSCReader.ReadFromFile(fname));
        }

        public HappeningSet TimelineOfDsc(DSCFile file)
        {
            var cmds = file.Commands;
            var dst = new HappeningSet();
            uint curTime = 0;
            uint curTft = 0;
            var lastTimeTargets = new List<Command_TARGET>();

            foreach(var cmdw in cmds)
            {
                var cmd = cmdw.Unwrap();
                if(cmd is Command_TIME)
                {
                    if(lastTimeTargets.Count > 0)
                    {
                        // NB: handle slides separately
                        // TODO: handle target flying time

                        ButtonState curEvtPress = ButtonState.None;
                        ButtonState curEvtHold = ButtonState.None;
                        ButtonState curEvtArrow = ButtonState.None;
                        uint curEvtArrowSeg = 0;

                        foreach(var tgt in lastTimeTargets)
                        {
                            // NB: not sure if slider segments might appear during other time, then calculation will be incorrect
                            var id = (TargetID)tgt.Type;
                            if (id == TargetID.SlideL || id == TargetID.SlideLChance ||
                                id == TargetID.SlideR || id == TargetID.SlideRChance)
                            {
                                if (curEvtArrow.HasFlag(ButtonState.Arrow2)) throw new Exception("3 arrows linked?!");
                                else if (curEvtArrow.HasFlag(ButtonState.Arrow1)) curEvtArrow |= ButtonState.Arrow2;
                                else curEvtArrow = ButtonState.Arrow1;
                            }
                            else if(id == TargetID.SlideBitL || id == TargetID.SlideBitR)
                            {
                                //if (curEvtArrow.HasFlag(ButtonState.Arrow2)) throw new Exception("2 arrows but with segments?!");
                                //curEvtArrowSeg += 1;
                               // throw new NotImplementedException("Chain slide not yet supported");
                            }
                            else
                            {
                                var press = TargetIDToButtonState(id);
                                var hold = TargetIDToHoldState(id);
                                if (curEvtPress.HasFlag(press) && press != ButtonState.None) throw new Exception("Event press overlap!");
                                if (curEvtHold.HasFlag(hold) && hold != ButtonState.None) throw new Exception("Event hold overlap!");
                                curEvtPress |= press;
                                curEvtHold |= hold;
                            }
                        }
                        
                        if(curEvtPress != ButtonState.None)
                        {
                            var pressEvt = new NoteHappening(curTime + curTft, curEvtPress, curEvtHold);
                            dst.Events.Add(pressEvt);
                        }

                        lastTimeTargets.Clear();
                    }
                    curTime = ((Command_TIME)cmd).Timestamp;
                }
                else if (cmd is Command_BAR_TIME_SET)
                {
                    var btime = ((Command_BAR_TIME_SET)cmd);
                    curTft = (uint) (1000.0 / ((double)btime.bpm / ((btime.notespeed + 1) * 60.0)));
                }
                else if (cmd is Command_TARGET_FLYING_TIME)
                {
                    var btime = ((Command_TARGET_FLYING_TIME)cmd);
                    curTft = btime.flying_time;
                }
                else if (cmd is Command_MODE_SELECT)
                {
                    var msel = ((Command_MODE_SELECT)cmd);
                    var evt = new ChallengeChangeHappening(curTime, msel.mode == 1);
                    Console.Error.WriteLine("[DSC] WARNING: Challenge Time Disabled");
                   // dst.Events.Add(evt);
                }
                else if(cmd is Command_END)
                {
                    dst.Events.Add(new EndOfLevelHappening(curTime));
                }
                else if(cmd is Command_TARGET)
                {
                    var tgt = ((Command_TARGET)cmd);
                    if(tgt.Type != (uint) TargetID.SlideBitL && tgt.Type != (uint) TargetID.SlideBitR)
                       lastTimeTargets.Add(tgt);
                }
            }

            return dst;
        }

        private ButtonState TargetIDToButtonState(TargetID targetID)
        {
            ButtonState rslt = ButtonState.None;
            switch (targetID)
            {
                case TargetID.Circle:
                case TargetID.CirChance:
                case TargetID.CirHold:
                    rslt = ButtonState.Circle;
                    break;

                case TargetID.Cross:
                case TargetID.CrossChance:
                case TargetID.CrossHold:
                    rslt = ButtonState.Cross;
                    break;

                case TargetID.Square:
                case TargetID.SquChance:
                case TargetID.SquHold:
                    rslt = ButtonState.Square;
                    break;

                case TargetID.Triangle:
                case TargetID.TriChance:
                case TargetID.TriHold:
                    rslt = ButtonState.Triangle;
                    break;

                case TargetID.Random:
                case TargetID.RandomHold:
                case TargetID.Same:
                    throw new Exception("Random notes are not supported");

                default:
                    throw new Exception("Unsupported target " + targetID.ToString());
            }
            return rslt;
        }

        private ButtonState TargetIDToHoldState(TargetID targetID)
        {
            ButtonState rslt = ButtonState.None;
            switch (targetID)
            {
                case TargetID.CirHold:
                    rslt = ButtonState.Circle;
                    break;

                case TargetID.CrossHold:
                    rslt = ButtonState.Cross;
                    break;

                case TargetID.SquHold:
                    rslt = ButtonState.Square;
                    break;

                case TargetID.TriHold:
                    rslt = ButtonState.Triangle;
                    break;

                case TargetID.RandomHold:
                    throw new Exception("Random notes are not supported");

                default:
                    break;
            }
            return rslt;
        }
    }
}
