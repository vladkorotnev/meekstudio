using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio
{
    public partial class frmCameraMover : Form
    {
        public frmMain Owner { get; set; }
        public frmCameraMover()
        {
            InitializeComponent();
        }

        private void frmCameraMover_Load(object sender, EventArgs e)
        {
            FetchStart();
            FetchEnd();
            SetStart();

            spX.MouseWheel += CoordinateMouseWheel;
            spY.MouseWheel += CoordinateMouseWheel;
            spZ.MouseWheel += CoordinateMouseWheel;
            sfX.MouseWheel += CoordinateMouseWheel;
            sfY.MouseWheel += CoordinateMouseWheel;
            sfZ.MouseWheel += CoordinateMouseWheel;
            snX.MouseWheel += CoordinateMouseWheel;
            snY.MouseWheel += CoordinateMouseWheel;
            snZ.MouseWheel += CoordinateMouseWheel;
            epX.MouseWheel += CoordinateMouseWheel;
            epY.MouseWheel += CoordinateMouseWheel;
            epZ.MouseWheel += CoordinateMouseWheel;
            efX.MouseWheel += CoordinateMouseWheel;
            efY.MouseWheel += CoordinateMouseWheel;
            efZ.MouseWheel += CoordinateMouseWheel;
            enX.MouseWheel += CoordinateMouseWheel;
            enY.MouseWheel += CoordinateMouseWheel;
            enZ.MouseWheel += CoordinateMouseWheel;

            
        }

        public void LoadCamFromOwner()
        {
            if (Owner != null && Owner.ActiveEditor is Editors.ScriptEditor)
            {
                var curStr = Owner.ActiveEditor.CurrentLine;
                try
                {
                    Owner.PreheatConstCacheIfNeeded();
                    Owner.evaluator.outputBuffer.Clear();
                    Owner.evaluator.ProcessScript(curStr, "camMover", true);
                    if (Owner.evaluator.outputBuffer.Count == 0) return;
                    var cmd = Owner.evaluator.outputBuffer.First();

                    if (cmd is MikuASM.Commands.Command_MOVE_CAMERA)
                    {
                        var cam = (MikuASM.Commands.Command_MOVE_CAMERA)cmd;

                        spX.Text = cam.startX.ToString();
                        spY.Text = cam.startY.ToString();
                        spZ.Text = cam.startZ.ToString();
                        sfX.Text = cam.startFocusX.ToString();
                        sfY.Text = cam.startFocusY.ToString();
                        sfZ.Text = cam.startFocusZ.ToString();
                        snX.Text = cam.startNormalX.ToString();
                        snY.Text = cam.startNormalY.ToString();
                        snZ.Text = cam.startNormalZ.ToString();

                        epX.Text = cam.endX.ToString();
                        epY.Text = cam.endY.ToString();
                        epZ.Text = cam.endZ.ToString();
                        efX.Text = cam.endFocusX.ToString();
                        efY.Text = cam.endFocusY.ToString();
                        efZ.Text = cam.endFocusZ.ToString();
                        enX.Text = cam.endNormalX.ToString();
                        enY.Text = cam.endNormalY.ToString();
                        enZ.Text = cam.endNormalZ.ToString();

                        nudDuration.Value = cam.duration_ms;
                    }
                }
                catch (Exception exc) { }
            }
        }

        private void CoordinateMouseWheel(object sender, MouseEventArgs e)
        {
            if(sender is TextBox)
            {
                TextBox tb = (TextBox)sender;
                try
                {
                    int cur = Convert.ToInt32(tb.Text);
                    cur += e.Delta;
                    tb.Text = cur.ToString();

                    if(tb.Parent.Parent == gbxStart)
                    {
                        SetStart();
                    } 
                    else if(tb.Parent.Parent == gbxEnd)
                    {
                        SetEnd();
                    }
                } 
                catch(Exception ex)
                {
                    tb.Text = "1000";
                }
            }
        }

        private void FetchStart()
        {
            if(MikuASM.DebugBridge.IsConnected)
            {
                var pos = MikuASM.DebugBridge.GetCameraPos();
                var focus = MikuASM.DebugBridge.GetCameraLookat();
                var norm = MikuASM.DebugBridge.GetCameraNormale();

                spX.Text = Math.Round(pos[0]).ToString();
                spY.Text = Math.Round(pos[1]).ToString();
                spZ.Text = Math.Round(pos[2]).ToString();

                sfX.Text = Math.Round(focus[0]).ToString();
                sfY.Text = Math.Round(focus[1]).ToString();
                sfZ.Text = Math.Round(focus[2]).ToString();

                snX.Text = Math.Round(norm[0]).ToString();
                snY.Text = Math.Round(norm[1]).ToString();
                snZ.Text = Math.Round(norm[2]).ToString();
            }
        }

        private void FetchEnd()
        {
            if (MikuASM.DebugBridge.IsConnected)
            {
                var pos = MikuASM.DebugBridge.GetCameraPos();
                var focus = MikuASM.DebugBridge.GetCameraLookat();
                var norm = MikuASM.DebugBridge.GetCameraNormale();

                epX.Text = Math.Round(pos[0]).ToString();
                epY.Text = Math.Round(pos[1]).ToString();
                epZ.Text = Math.Round(pos[2]).ToString();

                efX.Text = Math.Round(focus[0]).ToString();
                efY.Text = Math.Round(focus[1]).ToString();
                efZ.Text = Math.Round(focus[2]).ToString();

                enX.Text = Math.Round(norm[0]).ToString();
                enY.Text = Math.Round(norm[1]).ToString();
                enZ.Text = Math.Round(norm[2]).ToString();
            }
        }

        private void SetStart()
        {
            var sp = GetStartPos();
            var sn = GetStartNormale();
            var sf = GetStartFocus();
            MoveCameraTo(sp, sf, sn);

            gbxStart.BackColor = Color.Aqua;
            gbxEnd.BackColor = this.BackColor;
        }


        private int[] GetStartPos()
        {
            return new int[]
            {
                Convert.ToInt32(spX.Text), Convert.ToInt32(spY.Text), Convert.ToInt32(spZ.Text)
            };
        }

       
        private int[] GetStartFocus()
        {
            return new int[]
           {
                Convert.ToInt32(sfX.Text), Convert.ToInt32(sfY.Text), Convert.ToInt32(sfZ.Text)
           };
        }

        private int[] GetStartNormale()
        {
            return new int[]
                       {
                Convert.ToInt32(snX.Text), Convert.ToInt32(snY.Text), Convert.ToInt32(snZ.Text)
                       };
        }

        private void SetEnd()
        {
            int[] ep = GetEndPos();
            int[] en = GetEndNormale();
            int[] ef = GetEndFocus();
            MoveCameraTo(ep, ef, en);
            gbxEnd.BackColor = Color.Aqua;
            gbxStart.BackColor = this.BackColor;
        }

        private int[] GetEndFocus()
        {
            return new int[]
           {
                Convert.ToInt32(efX.Text), Convert.ToInt32(efY.Text), Convert.ToInt32(efZ.Text)
           };
        }

        private int[] GetEndNormale()
        {
            return new int[]
           {
                Convert.ToInt32(enX.Text), Convert.ToInt32(enY.Text), Convert.ToInt32(enZ.Text)
           };
        }

        private int[] GetEndPos()
        {
            return new int[]
            {
                Convert.ToInt32(epX.Text), Convert.ToInt32(epY.Text), Convert.ToInt32(epZ.Text)
            };
        }

        private void btnFetchStart_Click(object sender, EventArgs e)
        {
            FetchStart();
        }

        private void btnFetchEnd_Click(object sender, EventArgs e)
        {
            FetchEnd();
        }

        private void btnSetStart_Click(object sender, EventArgs e)
        {
            SetStart();
        }

        private void btnSetEnd_Click(object sender, EventArgs e)
        {
            SetEnd();
        }


        private MikuASM.Commands.Command_MOVE_CAMERA CreateCameraCommand(uint dur, int[] sPos, int[] sFocus, int[] sNormale, int[] ePos, int[] eFocus, int[] eNormale)
        {
            var cmd = new MikuASM.Commands.Command_MOVE_CAMERA
            {
                duration_ms = dur,
                startX = (sPos[0]),
                startY = (sPos[1]),
                startZ = (sPos[2]),
                startFocusX = (sFocus[0]),
                startFocusY = (sFocus[1]),
                startFocusZ = (sFocus[2]),
                startNormalX = (sNormale[0]),
                startNormalY = (sNormale[1]),
                startNormalZ = (sNormale[2]),
                endX = (ePos[0]),
                endY = (ePos[1]),
                endZ = (ePos[2]),
                endFocusX = (eFocus[0]),
                endFocusY = (eFocus[1]),
                endFocusZ = (eFocus[2]),
                endNormalX = (eNormale[0]),
                endNormalY = (eNormale[1]),
                endNormalZ = (eNormale[2])
            };
            return cmd;
        }

        private void MoveCameraTo(int[] pos, int[] lookat, int[] normale)
        {
            var prepro = new MikuASM.Compiler();
            var cmd = CreateCameraCommand(100, pos, lookat, normale, pos, lookat, normale);
            prepro.PushCommand(cmd);
            MikuASM.DebugBridge.SendScript(prepro.DumpToByteArray());
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if(Owner != null && Owner.ActiveEditor != null)
            {
                string cmd = CreateCameraCommand(Convert.ToUInt32(nudDuration.Value), GetStartPos(), GetStartFocus(), GetStartNormale(), GetEndPos(), GetEndFocus(), GetEndNormale()).ToString();
                Owner.ActiveEditor.InsertLineAfterCaret(cmd);
            } 
            else
            {
                MessageBox.Show(Strings.ErrNoEditor);
            }
        }

        private void btnAnimate_Click(object sender, EventArgs e)
        {
            btnAnimate.Text = Strings.BtnAnimating;
            this.Enabled = false;

            int[][] start = new int[][]
            {
                GetStartPos(), GetStartFocus(), GetStartNormale()
            };

            int[][] end = new int[][]
            {
                GetEndPos(), GetEndFocus(), GetEndNormale()
            };

            int[][] perFrame = new int[][]
            {
                new int[3], new int[3], new int[3]
            };

            int frames = Convert.ToInt32(nudDuration.Value) / 17;
            for(int i = 0; i < 3; i ++)
            {
                for(int j = 0; j < 3; j++)
                {
                    int curCoordStart = start[i][j];
                    int curCoordEnd = end[i][j];
                    perFrame[i][j] = (curCoordEnd - curCoordStart) / frames;
                }
            }

            int curFrame = 0;
            SetStart();
            gbxStart.BackColor = this.BackColor;
            while (curFrame < frames)
            {
                for(int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        start[i][j] += perFrame[i][j];
                    }
                }
                MoveCameraTo(start[0], start[1], start[2]);
                curFrame++;
                Thread.Sleep(17);
                Application.DoEvents();
            }
            SetEnd();

            this.Enabled = true;
            btnAnimate.Text = Strings.BtnAnimate;
        }

        private void frmCameraMover_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
