using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio
{
    public partial class frmProjectCreation : Form
    {
        public frmProjectCreation()
        {
            InitializeComponent();
        }
        public Project NewProject { get; private set; }

        private void btnPickFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = Strings.SelectFolderTitle;
            if (fbd.ShowDialog() != DialogResult.Cancel)
            {
                tbProjectPath.Text = fbd.SelectedPath;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(tbProjectPath.Text)) return;

            try
            {
                NewProject = Project.CreateInFolder(tbProjectPath.Text, tbProjectName.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, Strings.FailureTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmProjectCreation_Load(object sender, EventArgs e)
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

        private void frmProjectCreation_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            e.Graphics.DrawRectangle(Pens.Black, rect);
        }
    }
}
