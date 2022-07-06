using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio.Windows
{
    public partial class frmWaitAttachment : Form
    {
        public frmWaitAttachment()
        {
            InitializeComponent();
        }

        private void frmWaitAttachment_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
        }

        public string Description
        {
            get
            {
                return lblDescription.Text;
            }
            set
            {
                lblDescription.Text = value;
                Text = value;
            }
        }

        private void frmWaitAttachment_Load(object sender, EventArgs e)
        {

        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
    }
}
