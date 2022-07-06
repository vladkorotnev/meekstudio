namespace MeekStudio
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView = new MeekStudio.ProjectTreeView();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tpgWelcome = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bootGameToDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attachToGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reattachToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.executeWhileEditingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evaluateUNTILCurrentLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evaluateCurrentFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.interactiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraMoveWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.charaMoveWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.setGameExePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.playMovieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.русскийToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.日本語ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolbar = new System.Windows.Forms.ToolStrip();
            this.tbbNewProj = new System.Windows.Forms.ToolStripButton();
            this.tbbOpenProj = new System.Windows.Forms.ToolStripButton();
            this.tbbSave = new System.Windows.Forms.ToolStripButton();
            this.tbbSaveAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbbAddFile = new System.Windows.Forms.ToolStripButton();
            this.tbbAddDir = new System.Windows.Forms.ToolStripButton();
            this.tbbImportFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbbBoot = new System.Windows.Forms.ToolStripButton();
            this.tbbRun = new System.Windows.Forms.ToolStripButton();
            this.tbbBuild = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tpgWelcome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabs);
            // 
            // treeView
            // 
            resources.ApplyResources(this.treeView, "treeView");
            this.treeView.LabelEdit = true;
            this.treeView.Name = "treeView";
            this.treeView.Project = null;
            this.treeView.onWantFileEvaluation += new System.EventHandler<System.IO.FileInfo>(this.treeView_onWantFileEvaluation);
            this.treeView.DoubleClick += new System.EventHandler(this.treeView_DoubleClick);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tpgWelcome);
            resources.ApplyResources(this.tabs, "tabs");
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabs_DrawItem);
            this.tabs.SelectedIndexChanged += new System.EventHandler(this.tabs_SelectedIndexChanged);
            this.tabs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabs_MouseDown);
            // 
            // tpgWelcome
            // 
            this.tpgWelcome.Controls.Add(this.pictureBox1);
            this.tpgWelcome.Controls.Add(this.label2);
            this.tpgWelcome.Controls.Add(this.label1);
            resources.ApplyResources(this.tpgWelcome, "tpgWelcome");
            this.tpgWelcome.Name = "tpgWelcome";
            this.tpgWelcome.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::MeekStudio.Properties.Resources.meekstudio;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // menu
            // 
            resources.ApplyResources(this.menu, "menu");
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.debuggerToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menu.Name = "menu";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createToolStripMenuItem,
            this.toolStripMenuItem1,
            this.openToolStripMenuItem,
            this.recentsToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAllToolStripMenuItem,
            this.toolStripMenuItem2,
            this.buildToolStripMenuItem,
            this.toolStripMenuItem3,
            this.exitToolStripMenuItem});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            resources.ApplyResources(this.projectToolStripMenuItem, "projectToolStripMenuItem");
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.newProj1;
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            resources.ApplyResources(this.createToolStripMenuItem, "createToolStripMenuItem");
            this.createToolStripMenuItem.Click += new System.EventHandler(this.createToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.loadProj;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // recentsToolStripMenuItem
            // 
            this.recentsToolStripMenuItem.Name = "recentsToolStripMenuItem";
            resources.ApplyResources(this.recentsToolStripMenuItem, "recentsToolStripMenuItem");
            this.recentsToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recentsToolStripMenuItem_DropDownItemClicked);
            // 
            // saveToolStripMenuItem
            // 
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            resources.ApplyResources(this.saveAllToolStripMenuItem, "saveAllToolStripMenuItem");
            this.saveAllToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.saveAll;
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.build;
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            resources.ApplyResources(this.buildToolStripMenuItem, "buildToolStripMenuItem");
            this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bootGameToDebugToolStripMenuItem,
            this.attachToGameToolStripMenuItem,
            this.reattachToolStripMenuItem,
            this.toolStripMenuItem4,
            this.executeWhileEditingToolStripMenuItem,
            this.evalToolStripMenuItem,
            this.evaluateUNTILCurrentLineToolStripMenuItem,
            this.evaluateCurrentFileToolStripMenuItem,
            this.toolStripMenuItem8,
            this.interactiveToolStripMenuItem,
            this.toolStripMenuItem5,
            this.setGameExePathToolStripMenuItem,
            this.toolStripMenuItem9,
            this.playMovieToolStripMenuItem});
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            resources.ApplyResources(this.debuggerToolStripMenuItem, "debuggerToolStripMenuItem");
            // 
            // bootGameToDebugToolStripMenuItem
            // 
            this.bootGameToDebugToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.boot;
            this.bootGameToDebugToolStripMenuItem.Name = "bootGameToDebugToolStripMenuItem";
            resources.ApplyResources(this.bootGameToDebugToolStripMenuItem, "bootGameToDebugToolStripMenuItem");
            this.bootGameToDebugToolStripMenuItem.Click += new System.EventHandler(this.bootGameToDebugToolStripMenuItem_Click);
            // 
            // attachToGameToolStripMenuItem
            // 
            this.attachToGameToolStripMenuItem.Name = "attachToGameToolStripMenuItem";
            resources.ApplyResources(this.attachToGameToolStripMenuItem, "attachToGameToolStripMenuItem");
            this.attachToGameToolStripMenuItem.Click += new System.EventHandler(this.attachToGameToolStripMenuItem_Click);
            // 
            // reattachToolStripMenuItem
            // 
            this.reattachToolStripMenuItem.Name = "reattachToolStripMenuItem";
            resources.ApplyResources(this.reattachToolStripMenuItem, "reattachToolStripMenuItem");
            this.reattachToolStripMenuItem.Click += new System.EventHandler(this.reattachToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            // 
            // executeWhileEditingToolStripMenuItem
            // 
            this.executeWhileEditingToolStripMenuItem.Checked = global::MeekStudio.Properties.Settings.Default.ExecOnInput;
            this.executeWhileEditingToolStripMenuItem.CheckOnClick = true;
            this.executeWhileEditingToolStripMenuItem.Name = "executeWhileEditingToolStripMenuItem";
            resources.ApplyResources(this.executeWhileEditingToolStripMenuItem, "executeWhileEditingToolStripMenuItem");
            this.executeWhileEditingToolStripMenuItem.Click += new System.EventHandler(this.executeWhileEditingToolStripMenuItem_Click);
            // 
            // evalToolStripMenuItem
            // 
            resources.ApplyResources(this.evalToolStripMenuItem, "evalToolStripMenuItem");
            this.evalToolStripMenuItem.Name = "evalToolStripMenuItem";
            this.evalToolStripMenuItem.Click += new System.EventHandler(this.selectedRangeToolStripMenuItem_Click);
            // 
            // evaluateUNTILCurrentLineToolStripMenuItem
            // 
            resources.ApplyResources(this.evaluateUNTILCurrentLineToolStripMenuItem, "evaluateUNTILCurrentLineToolStripMenuItem");
            this.evaluateUNTILCurrentLineToolStripMenuItem.Name = "evaluateUNTILCurrentLineToolStripMenuItem";
            this.evaluateUNTILCurrentLineToolStripMenuItem.Click += new System.EventHandler(this.evaluateUNTILCurrentLineToolStripMenuItem_Click);
            // 
            // evaluateCurrentFileToolStripMenuItem
            // 
            resources.ApplyResources(this.evaluateCurrentFileToolStripMenuItem, "evaluateCurrentFileToolStripMenuItem");
            this.evaluateCurrentFileToolStripMenuItem.Name = "evaluateCurrentFileToolStripMenuItem";
            this.evaluateCurrentFileToolStripMenuItem.Click += new System.EventHandler(this.evaluateCurrentFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            resources.ApplyResources(this.toolStripMenuItem8, "toolStripMenuItem8");
            // 
            // interactiveToolStripMenuItem
            // 
            this.interactiveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraMoveWizardToolStripMenuItem,
            this.charaMoveWizardToolStripMenuItem});
            resources.ApplyResources(this.interactiveToolStripMenuItem, "interactiveToolStripMenuItem");
            this.interactiveToolStripMenuItem.Name = "interactiveToolStripMenuItem";
            // 
            // cameraMoveWizardToolStripMenuItem
            // 
            this.cameraMoveWizardToolStripMenuItem.Name = "cameraMoveWizardToolStripMenuItem";
            resources.ApplyResources(this.cameraMoveWizardToolStripMenuItem, "cameraMoveWizardToolStripMenuItem");
            this.cameraMoveWizardToolStripMenuItem.Click += new System.EventHandler(this.cameraMoveWizardToolStripMenuItem_Click);
            // 
            // charaMoveWizardToolStripMenuItem
            // 
            this.charaMoveWizardToolStripMenuItem.Name = "charaMoveWizardToolStripMenuItem";
            resources.ApplyResources(this.charaMoveWizardToolStripMenuItem, "charaMoveWizardToolStripMenuItem");
            this.charaMoveWizardToolStripMenuItem.Click += new System.EventHandler(this.charaMoveWizardToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            // 
            // setGameExePathToolStripMenuItem
            // 
            this.setGameExePathToolStripMenuItem.Name = "setGameExePathToolStripMenuItem";
            resources.ApplyResources(this.setGameExePathToolStripMenuItem, "setGameExePathToolStripMenuItem");
            this.setGameExePathToolStripMenuItem.Click += new System.EventHandler(this.setGameExePathToolStripMenuItem_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            resources.ApplyResources(this.toolStripMenuItem9, "toolStripMenuItem9");
            // 
            // playMovieToolStripMenuItem
            // 
            this.playMovieToolStripMenuItem.Image = global::MeekStudio.Properties.Resources.run;
            this.playMovieToolStripMenuItem.Name = "playMovieToolStripMenuItem";
            resources.ApplyResources(this.playMovieToolStripMenuItem, "playMovieToolStripMenuItem");
            this.playMovieToolStripMenuItem.Click += new System.EventHandler(this.playMovieToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.languageToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.русскийToolStripMenuItem,
            this.日本語ToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            resources.ApplyResources(this.englishToolStripMenuItem, "englishToolStripMenuItem");
            this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
            // 
            // русскийToolStripMenuItem
            // 
            this.русскийToolStripMenuItem.Name = "русскийToolStripMenuItem";
            resources.ApplyResources(this.русскийToolStripMenuItem, "русскийToolStripMenuItem");
            this.русскийToolStripMenuItem.Click += new System.EventHandler(this.русскийToolStripMenuItem_Click);
            // 
            // 日本語ToolStripMenuItem
            // 
            this.日本語ToolStripMenuItem.Name = "日本語ToolStripMenuItem";
            resources.ApplyResources(this.日本語ToolStripMenuItem, "日本語ToolStripMenuItem");
            this.日本語ToolStripMenuItem.Click += new System.EventHandler(this.日本語ToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // status
            // 
            this.status.Name = "status";
            resources.ApplyResources(this.status, "status");
            // 
            // toolbar
            // 
            this.toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbbNewProj,
            this.tbbOpenProj,
            this.tbbSave,
            this.tbbSaveAll,
            this.toolStripSeparator1,
            this.tbbAddFile,
            this.tbbAddDir,
            this.tbbImportFile,
            this.toolStripSeparator2,
            this.tbbBoot,
            this.tbbRun,
            this.tbbBuild});
            resources.ApplyResources(this.toolbar, "toolbar");
            this.toolbar.Name = "toolbar";
            // 
            // tbbNewProj
            // 
            this.tbbNewProj.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbbNewProj.Image = global::MeekStudio.Properties.Resources.newProj;
            resources.ApplyResources(this.tbbNewProj, "tbbNewProj");
            this.tbbNewProj.Name = "tbbNewProj";
            this.tbbNewProj.Click += new System.EventHandler(this.createToolStripMenuItem_Click);
            // 
            // tbbOpenProj
            // 
            this.tbbOpenProj.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbOpenProj, "tbbOpenProj");
            this.tbbOpenProj.Name = "tbbOpenProj";
            this.tbbOpenProj.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // tbbSave
            // 
            this.tbbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbSave, "tbbSave");
            this.tbbSave.Name = "tbbSave";
            this.tbbSave.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // tbbSaveAll
            // 
            this.tbbSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbSaveAll, "tbbSaveAll");
            this.tbbSaveAll.Name = "tbbSaveAll";
            this.tbbSaveAll.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tbbAddFile
            // 
            this.tbbAddFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbAddFile, "tbbAddFile");
            this.tbbAddFile.Name = "tbbAddFile";
            this.tbbAddFile.Click += new System.EventHandler(this.tbbAddFile_Click);
            // 
            // tbbAddDir
            // 
            this.tbbAddDir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbAddDir, "tbbAddDir");
            this.tbbAddDir.Name = "tbbAddDir";
            this.tbbAddDir.Click += new System.EventHandler(this.tbbAddDir_Click);
            // 
            // tbbImportFile
            // 
            this.tbbImportFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbImportFile, "tbbImportFile");
            this.tbbImportFile.Name = "tbbImportFile";
            this.tbbImportFile.Click += new System.EventHandler(this.tbbImportFile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // tbbBoot
            // 
            this.tbbBoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbBoot, "tbbBoot");
            this.tbbBoot.Name = "tbbBoot";
            this.tbbBoot.Click += new System.EventHandler(this.bootGameToDebugToolStripMenuItem_Click);
            // 
            // tbbRun
            // 
            this.tbbRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbRun, "tbbRun");
            this.tbbRun.Name = "tbbRun";
            this.tbbRun.Click += new System.EventHandler(this.playMovieToolStripMenuItem_Click);
            // 
            // tbbBuild
            // 
            this.tbbBuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbbBuild, "tbbBuild");
            this.tbbBuild.Name = "tbbBuild";
            this.tbbBuild.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menu);
            this.MainMenuStrip = this.menu;
            this.Name = "frmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.tpgWelcome.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attachToGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem executeWhileEditingToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel status;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private MeekStudio.ProjectTreeView treeView;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tpgWelcome;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem interactiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraMoveWizardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem charaMoveWizardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reattachToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem evalToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem setGameExePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bootGameToDebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem playMovieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem evaluateUNTILCurrentLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem evaluateCurrentFileToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolbar;
        private System.Windows.Forms.ToolStripButton tbbNewProj;
        private System.Windows.Forms.ToolStripButton tbbOpenProj;
        private System.Windows.Forms.ToolStripButton tbbSave;
        private System.Windows.Forms.ToolStripButton tbbSaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbbAddFile;
        private System.Windows.Forms.ToolStripButton tbbAddDir;
        private System.Windows.Forms.ToolStripButton tbbImportFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbbBoot;
        private System.Windows.Forms.ToolStripButton tbbRun;
        private System.Windows.Forms.ToolStripButton tbbBuild;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem русскийToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 日本語ToolStripMenuItem;
    }
}

