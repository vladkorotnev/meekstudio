using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio
{
    public partial class ProjectTreeView : TreeView
    {
        public ProjectTreeView() 
        {
            InitializeComponent();
        }
        
        private ContextMenuStrip treeContext;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem newFolderToolStripMenuItem;
        private ToolStripMenuItem newFileToolStripMenuItem;
        private ToolStripMenuItem addFileToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripMenuItem setAsToolStripMenuItem;
        private ToolStripMenuItem pVDBEntryFileToolStripMenuItem;
        private ToolStripMenuItem entrypointSourceFileToolStripMenuItem;
        private ToolStripMenuItem movieonlyDSCFileToolStripMenuItem;
        private ToolStripMenuItem backgroundAudioFileToolStripMenuItem;
        private ToolStripMenuItem evalFile;

        public void UpdateContextText()
        {
            this.newFileToolStripMenuItem.Text = Strings.MnuNewFile;
            this.addFileToolStripMenuItem.Text = Strings.MnuAddFile;
            this.renameToolStripMenuItem.Text = Strings.MnuRenFile;
            this.deleteToolStripMenuItem.Text = Strings.MnuDelFile;
            this.setAsToolStripMenuItem.Text = Strings.MnuSetAs;
            this.pVDBEntryFileToolStripMenuItem.Text = Strings.ThingPvDb;
            this.entrypointSourceFileToolStripMenuItem.Text = Strings.ThingEntrypoint;
            this.movieonlyDSCFileToolStripMenuItem.Text = Strings.ThingMovieDsc;
            this.backgroundAudioFileToolStripMenuItem.Text = Strings.ThingMovieAudio;
            this.evalFile.Text = Strings.MnuEvalFile;

            this.newFolderToolStripMenuItem.Text = Strings.MnuNewFolder;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.setAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pVDBEntryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.entrypointSourceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movieonlyDSCFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundAudioFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evalFile = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeContext
            // 
            this.treeContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFolderToolStripMenuItem,
            this.newFileToolStripMenuItem,
            this.addFileToolStripMenuItem,
            this.toolStripMenuItem6,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.evalFile,
            this.toolStripMenuItem7,
            this.setAsToolStripMenuItem});
            this.treeContext.Name = "treeContext";
            this.treeContext.Size = new System.Drawing.Size(137, 148);
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.newFolderToolStripMenuItem.Click += new EventHandler(this.newFolderToolStripMenuItem_Click);
            // 
            // newFileToolStripMenuItem
            // 
            this.newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            this.newFileToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.newFileToolStripMenuItem.Click += new EventHandler(newFileToolStripMenuItem_Click);
            // 
            // addFileToolStripMenuItem
            // 
            this.addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            this.addFileToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.addFileToolStripMenuItem.Click += new EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(133, 6);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.renameToolStripMenuItem.Click += new EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.deleteToolStripMenuItem.Click += new EventHandler(this.deleteToolStripMenuItem_Click);
            this.deleteToolStripMenuItem.ShortcutKeys = Keys.Delete;
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(133, 6);
            // 
            // setAsToolStripMenuItem
            // 
            this.setAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pVDBEntryFileToolStripMenuItem,
            this.entrypointSourceFileToolStripMenuItem,
            this.movieonlyDSCFileToolStripMenuItem,
            this.backgroundAudioFileToolStripMenuItem});
            this.setAsToolStripMenuItem.Name = "setAsToolStripMenuItem";
            this.setAsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            // 
            // pVDBEntryFileToolStripMenuItem
            // 
            this.pVDBEntryFileToolStripMenuItem.Name = "pVDBEntryFileToolStripMenuItem";
            this.pVDBEntryFileToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.pVDBEntryFileToolStripMenuItem.Click += new EventHandler(this.setAsPVDBEntryToolStripMenuItem_Click);
            // 
            // entrypointSourceFileToolStripMenuItem
            // 
            this.entrypointSourceFileToolStripMenuItem.Name = "entrypointSourceFileToolStripMenuItem";
            this.entrypointSourceFileToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.entrypointSourceFileToolStripMenuItem.Click += new EventHandler(this.setAsEntrypointToolStripMenuItem_Click);
            // 
            // movieonlyDSCFileToolStripMenuItem
            // 
            this.movieonlyDSCFileToolStripMenuItem.Name = "movieonlyDSCFileToolStripMenuItem";
            this.movieonlyDSCFileToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.movieonlyDSCFileToolStripMenuItem.Click += new EventHandler(this.movieonlyDSCFileToolStripMenuItem_Click);
            // 
            // backgroundAudioFileToolStripMenuItem
            // 
            this.backgroundAudioFileToolStripMenuItem.Name = "backgroundAudioFileToolStripMenuItem";
            this.backgroundAudioFileToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.backgroundAudioFileToolStripMenuItem.Click += new EventHandler(this.backgroundAudioFileToolStripMenuItem_Click);
            //
            // evalFile
            //
            this.evalFile.Name = "evalFile";
            this.evalFile.Size = new System.Drawing.Size(190, 22);
            this.evalFile.Click += EvalFile_Click;

            UpdateContextText();

            //
            // icons
            //
            this.icons = new ImageList();
            this.icons.Images.Add(Properties.Resources.dir);
            this.icons.Images.Add(Properties.Resources.file);
            this.icons.Images.Add(Properties.Resources.code);
            this.icons.Images.Add(Properties.Resources.dsc);
            this.icons.Images.Add(Properties.Resources.sound);
            this.icons.Images.Add(Properties.Resources.pvdb);

            // 
            // ProjectTreeView
            // 
            this.ContextMenuStrip = this.treeContext;
            this.NodeMouseClick += ProjectTreeView_NodeMouseClick;
            this.AfterLabelEdit += this.treeView_AfterLabelEdit;
            this.ImageList = this.icons;
            this.treeContext.ResumeLayout(false);
            this.ResumeLayout(false);
        }

 

        private void ProjectTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.SelectedNode = e.Node;

            if(e.Node.Tag is FileInfo)
            {
                var fi = (FileInfo)e.Node.Tag;
                setAsToolStripMenuItem.Enabled = true;

                var ext = fi.Extension.ToLower();
                bool isScript = ext == ".mia";
                bool isSound = ext == ".ogg";
                bool isPvDb = ext == ".txt";
                bool isBinary = ext == ".dsc";

                evalFile.Enabled = isScript;
                pVDBEntryFileToolStripMenuItem.Enabled = isPvDb;
                entrypointSourceFileToolStripMenuItem.Enabled = isScript;
                movieonlyDSCFileToolStripMenuItem.Enabled = isBinary;
                backgroundAudioFileToolStripMenuItem.Enabled = isSound;
            } 
            else if (e.Node.Tag is DirectoryInfo)
            {
                evalFile.Enabled = false;
                setAsToolStripMenuItem.Enabled = false;
            }
        }
    }
}
