using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio
{
    public partial class frmCharaMover : Form
    {
        public frmMain Owner { get; set; }

        public frmCharaMover()
        {
            InitializeComponent();
        }

        private void frmCharaMover_Load(object sender, EventArgs e)
        {
            cmbCharaIdxMove.SelectedIndex = 0;
            cmbCharaIdxRot.SelectedIndex = 0;

            cpX.MouseWheel += CoordinateMouseWheel;
            cpY.MouseWheel += CoordinateMouseWheel;
            cpZ.MouseWheel += CoordinateMouseWheel;
            tbRot.MouseWheel += CoordinateMouseWheel;

            FetchCurCharPos();

            LoadFromOwner();
        }

        public void LoadFromOwner()
        {
            if (Owner != null && Owner.ActiveEditor is Editors.ScriptEditor)
            {
                var curStr = Owner.ActiveEditor.CurrentLine;
                try
                {
                    Owner.PreheatConstCacheIfNeeded();
                    Owner.evaluator.outputBuffer.Clear();
                    Owner.evaluator.ProcessScript(curStr, "charaMover", true);
                    if (Owner.evaluator.outputBuffer.Count == 0) return;
                    var cmd = Owner.evaluator.outputBuffer.First();
                    
                    if (cmd is MikuASM.Commands.Command_MIKU_MOVE)
                    {
                        var motion = (MikuASM.Commands.Command_MIKU_MOVE)cmd;

                        if(motion.player_id >= 0 && motion.player_id <= 2)
                        {
                            cpX.Text = motion.x.ToString();
                            cpY.Text = motion.y.ToString();
                            cpZ.Text = motion.z.ToString();
                            cmbCharaIdxMove.SelectedIndex = (int)motion.player_id;
                        }
                    } 
                    else if (cmd is MikuASM.Commands.Command_MIKU_ROT)
                    {
                        var rotation = (MikuASM.Commands.Command_MIKU_ROT)cmd;

                        if (rotation.player_ID >= 0 && rotation.player_ID <= 2)
                        {
                            tbRot.Text = rotation.orientation.ToString();
                            cmbCharaIdxRot.SelectedIndex = (int)rotation.player_ID;
                        }
                    }
                }
                catch (Exception exc) { }
            } 
        }

        private void CoordinateMouseWheel(object sender, MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb = (TextBox)sender;
                try
                {
                    int cur = Convert.ToInt32(tb.Text);
                    cur += tb.Parent == gbxRot ? e.Delta/8 : e.Delta;
                    tb.Text = cur.ToString();

                    if (tb.Parent.Parent == gbxMove)
                    {
                        SetCurCharPos();
                    }
                    else if (tb.Parent == gbxRot)
                    {
                        SetCurCharRot();
                    }
                }
                catch (Exception ex)
                {
                    tb.Text = "1000";
                }
            }
        }

        private void FetchCurCharPos()
        {
            var pos = MikuASM.DebugBridge.GetCharaPos(cmbCharaIdxMove.SelectedIndex);
            if(pos != null)
            {
                cpX.Text = Math.Round(pos[0]).ToString();
                cpY.Text = Math.Round(pos[1]).ToString();
                cpZ.Text = Math.Round(pos[2]).ToString();
            }
        }

        private MikuASM.Commands.Command_MIKU_MOVE MakeCurCharMoveCommand()
        {
            return new MikuASM.Commands.Command_MIKU_MOVE
            {
                player_id = (uint)cmbCharaIdxMove.SelectedIndex,
                x = Convert.ToInt32(cpX.Text),
                y = Convert.ToInt32(cpY.Text),
                z = Convert.ToInt32(cpZ.Text)
            };
        }

        private MikuASM.Commands.Command_MIKU_ROT MakeCurCharRotCommand()
        {
            return new MikuASM.Commands.Command_MIKU_ROT
            {
                player_ID = (uint)cmbCharaIdxRot.SelectedIndex,
                orientation = Convert.ToInt32(tbRot.Text)
            };
        }

        private void SetCurCharPos()
        {
            var prepro = new MikuASM.Compiler();
            var cmd = MakeCurCharMoveCommand();
            prepro.PushCommand(cmd);
            MikuASM.DebugBridge.SendScript(prepro.DumpToByteArray());
        }

        private void SetCurCharRot()
        {
            var prepro = new MikuASM.Compiler();
            var cmd = MakeCurCharRotCommand();
            prepro.PushCommand(cmd);
            MikuASM.DebugBridge.SendScript(prepro.DumpToByteArray());
        }

        private void btnFetchPos_Click(object sender, EventArgs e)
        {
            FetchCurCharPos();
        }

        private void btnInsertMove_Click(object sender, EventArgs e)
        {
            if (Owner != null && Owner.ActiveEditor != null)
            {
                string cmd = MakeCurCharMoveCommand().ToString();
                Owner.ActiveEditor.InsertLineAfterCaret(cmd);
            }
            else
            {
                MessageBox.Show(Strings.ErrNoEditor);
            }
        }

        private void btnInsertRot_Click(object sender, EventArgs e)
        {
            if (Owner != null && Owner.ActiveEditor != null)
            {
                string cmd = MakeCurCharRotCommand().ToString();
                Owner.ActiveEditor.InsertLineAfterCaret(cmd);
            }
            else
            {
                MessageBox.Show(Strings.ErrNoEditor);
            }
        }

        private void btnSetPos_Click(object sender, EventArgs e)
        {
            SetCurCharPos();
        }

        private void btnSendRot_Click(object sender, EventArgs e)
        {
            SetCurCharRot();
        }

        private void frmCharaMover_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
