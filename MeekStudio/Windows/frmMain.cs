using MeekStudio.Locales;
using MeekStudio.Properties;
using MeekStudio.Windows;
using MikuASM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MeekStudio
{
    public partial class frmMain : Form
    {
        private static string OriginalTitle = "";
        private ToolStripMenuItem currEditorMenu;
        internal Project currProject;

        public void SetStatus(string text, bool isError = false)
        {
            status.Text = text;
            if(isError)
            {
                status.ForeColor = Color.Red;
                SystemSounds.Exclamation.Play();   
            } else
            {
                status.ForeColor = Color.Black;
            }
        }

        public frmMain()
        {
            InitializeComponent();

            treeView.onFileDeleted += TreeView_onFileDeleted;
            treeView.onEntityRenamed += TreeView_onEntityRenamed;
            treeView.onProjectMetaChange += TreeView_onProjectMetaChange;

            MikuASM.DebugBridge.OnAttached += DebugBridge_OnAttached;
            MikuASM.DebugBridge.OnDetached += DebugBridge_OnDetached;
        }


        private void TreeView_onEntityRenamed(object sender, FileSystemRenameEventArgs e)
        {
            string oldRelPath = currProject.AbsPathToRelative(e.oldInfo.FullName);
            string newRelPath = currProject.AbsPathToRelative(e.newInfo.FullName);
            if(e.oldInfo is DirectoryInfo)
            {
                foreach (var tab in tabs.TabPages)
                {
                    if (tab is Editors.TextEditor)
                    {
                        var te = (Editors.TextEditor)tab;
                        if (te.FileName.StartsWith(oldRelPath))
                        {
                            te.FilePath = e.newInfo.FullName + te.FilePath.Substring(e.oldInfo.FullName.Length);
                            te.Save();
                        }
                    }
                }
            }
            else if (e.oldInfo is FileInfo)
            {
                foreach (var tab in tabs.TabPages)
                {
                    if (tab is Editors.TextEditor)
                    {
                        var te = (Editors.TextEditor)tab;
                        if (te.FilePath.Equals(e.oldInfo.FullName))
                        {
                            te.FilePath = e.newInfo.FullName;
                            te.Save();
                        }
                    }
                }
            }
        }

        private void TreeView_onProjectMetaChange(object sender, ProjectMetaEditedEventArgs e)
        {
            string changedThing = "";
            string changedValue = "";
            switch(e.changedMeta)
            {
                case ProjectMetaEditedEventArgs.MetaType.MovieOut:
                    changedThing = Strings.ThingMovieDsc;
                    changedValue = currProject.MovieOnlyDscFileName;
                    break;

                case ProjectMetaEditedEventArgs.MetaType.MovieSound:
                    changedThing = Strings.ThingMovieAudio;
                    changedValue = currProject.AudioFileName;
                    break;

                case ProjectMetaEditedEventArgs.MetaType.PvDb:
                    changedThing = Strings.ThingPvDb;
                    changedValue = currProject.PvDbEntryFileName;
                    break;

                case ProjectMetaEditedEventArgs.MetaType.Entrypoint:
                    changedThing = Strings.ThingEntrypoint;
                    changedValue = currProject.EntryPointFileName;
                    break;

                case ProjectMetaEditedEventArgs.MetaType.Title:
                    if(e.isSuccess)
                    {
                        UpdateTitle();
                    }
                    return;

                default:
                    return;
            }

            if(e.isSuccess)
            {
                SetStatus(String.Format(Strings.StsChangedThing, changedThing, changedValue));
            } 
            else
            {
                SetStatus(String.Format(Strings.StsCannotChgThing, changedThing), true);
            }
        }

        private void TreeView_onFileDeleted(object sender, FileSystemInfo e)
        {
            string relPath = currProject.AbsPathToRelative(e.FullName);
            if (e is DirectoryInfo)
            {
                foreach (var tab in tabs.TabPages)
                {
                    if (tab is Editors.TextEditor)
                    {
                        var te = (Editors.TextEditor)tab;
                        if (te.FileName.StartsWith(relPath))
                        {
                            tabs.TabPages.Remove(te);
                        }
                    }
                }
            } 
            else if (e is FileInfo)
            {
                foreach (var tab in tabs.TabPages)
                {
                    if (tab is Editors.TextEditor)
                    {
                        var te = (Editors.TextEditor)tab;
                        if (te.FileName.EndsWith(relPath))
                        {
                            tabs.TabPages.Remove(te);
                        }
                    }
                }
            }
        }

        private bool AskLeaveProject()
        {
            if(currProject != null)
            {
                if(MessageBox.Show(String.Format(Strings.CloseProjectMsg, currProject.Name), Strings.CloseProjectTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveAll();
                    currProject.SaveMeta();
                    SetProject(null);
                    return true;
                } else
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveAll()
        {
            foreach(var page in tabs.TabPages)
            {
                if (page is Editors.TextEditor)
                {
                    ((Editors.TextEditor)page).Save();
                }
            }
        }

        private void LoadProject(string folder)
        {
            if (!AskLeaveProject()) return;
            var prefs = Properties.Settings.Default;

            try
            {
                var proj = Project.FromFolder(folder);
                SetProject(proj);

                if(prefs.Recents.Contains(folder))
                {
                    prefs.Recents.Remove(folder);
                }
                prefs.Recents.Insert(0, folder);

                if(prefs.Recents.Count >= 5)
                {
                    prefs.Recents.RemoveAt(prefs.Recents.Count - 1);
                }
                prefs.Save();
            } 
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message, Strings.FailureTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTitle()
        {
            Text = OriginalTitle + (currProject == null ? "" : " - " + currProject.Name + " (" + currProject.Location + ")");
        }

        private void SetProject(Project project)
        {
            currProject = project;
            UpdateTitle();

            if(currProject == null)
            {
                saveAllToolStripMenuItem.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
                buildToolStripMenuItem.Enabled = false;
                treeView.Project = null;

                SetStatus(Strings.StsProjectClose);
            } 
            else
            {
                saveAllToolStripMenuItem.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                buildToolStripMenuItem.Enabled = true;
                treeView.Project = currProject;
                

                SetStatus(String.Format(Strings.StsProjectOpen, project.Name));
            }
        }

        
        private void SpawnTab(TabPage te)
        {
            tabs.TabPages.Remove(tpgWelcome);
            tabs.TabPages.Add(te);
            tabs.SelectedIndex = tabs.TabPages.IndexOf(te);
            if(te is Editors.EditorTab)
            {
                ((Editors.EditorTab)te).OwnerForm = this;
            }
            UpdateEditorMenu();
        }

        private void UpdateEditorMenu()
        {
            if (currEditorMenu != null)
            {
                menu.Items.Remove(currEditorMenu);
                currEditorMenu = null;
            }
            if (tabs.SelectedTab is Editors.TextEditor)
            {
                var tab = (Editors.TextEditor)tabs.SelectedTab;
                currEditorMenu = tab.EditorMenu;
                if (currEditorMenu != null)
                {
                    menu.Items.Insert(1, currEditorMenu);
                }
            }
        }
        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEditorMenu();
        }

        private void tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage thisTab = tabs.TabPages[e.Index];
            string tabTitle = thisTab.Text;
            //Draw Close button
            Point closeLoc = new Point(15, 5);
            e.Graphics.DrawRectangle(Pens.Black, e.Bounds.Right - closeLoc.X, e.Bounds.Top + closeLoc.Y, 10, 12);
            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.Right - closeLoc.X, e.Bounds.Top + closeLoc.Y, 10, 12);
            e.Graphics.DrawString("x", e.Font, Brushes.Black, e.Bounds.Right - (closeLoc.X), e.Bounds.Top + closeLoc.Y - 2);
            // Draw String of Caption
            e.Graphics.DrawString(tabTitle, e.Font, Brushes.Black, e.Bounds.Left + 28, e.Bounds.Top + 4);
            e.DrawFocusRectangle();
        }

        private void tabs_MouseDown(object sender, MouseEventArgs e)
        {
            Point closeLoc = new Point(15, 5);
            Rectangle r = tabs.GetTabRect(tabs.SelectedIndex);
            Rectangle closeButton = new Rectangle(r.Right - closeLoc.X, r.Top + closeLoc.Y, 10, 12);
            if (closeButton.Contains(e.Location))
            {
                if(tabs.SelectedTab is Editors.TextEditor)
                {
                    ((Editors.TextEditor)tabs.SelectedTab).Save();
                }
                tabs.TabPages.Remove(tabs.SelectedTab);
                return; // Don't keep running logic in method
            }
            for (int i = 0; i < tabs.TabCount; i++)
            {
                r = tabs.GetTabRect(i);
                if (r.Contains(e.Location) && e.Button == MouseButtons.Middle)
                {
                    if(tabs.TabPages[i] is Editors.TextEditor)
                        ((Editors.TextEditor)tabs.TabPages[i]).Save();
                    tabs.TabPages.RemoveAt(i);
                }
            }
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AskLeaveProject()) return;

            var creator = new frmProjectCreation();
            if(creator.ShowDialog() == DialogResult.OK)
            {
                SetProject(creator.NewProject);
                SetStatus(String.Format(Strings.StsProjectNew, currProject.Name));
            }
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            OriginalTitle = this.Text + " " + Application.ProductVersion;
            Text = OriginalTitle;

            var Recents = Properties.Settings.Default.Recents;
            if (Recents == null)
            {
                Properties.Settings.Default.Recents = new System.Collections.Specialized.StringCollection();
                Recents = Properties.Settings.Default.Recents;
            }

            foreach(string path in Recents)
            {
                recentsToolStripMenuItem.DropDownItems.Add(path);
                recentsToolStripMenuItem.DropDownItems[recentsToolStripMenuItem.DropDownItems.Count - 1].Tag = path;
            }

            if(Recents.Count > 0 && Directory.Exists(Recents[0]))
            {
                LoadProject(Recents[0]);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!AskLeaveProject())
            {
                e.Cancel = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskLeaveProject())
                Application.Exit();
        }

        private void recentsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(AskLeaveProject())
            {
                LoadProject((string)e.ClickedItem.Tag);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(AskLeaveProject())
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = Strings.OpenProjectTitle;
                if (fbd.ShowDialog() != DialogResult.Cancel)
                {
                    LoadProject(fbd.SelectedPath);
                }
            }
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null) return;
            var curItem = treeView.SelectedNode.Tag;
            if(curItem is FileInfo)
            {
                var curFile = (FileInfo)curItem;

                BringUpEditor(curFile);   
            }
        }

        private void BringUpEditor(FileInfo curFile, int lineNo = -1)
        {
            foreach (var page in tabs.TabPages)
            {
                if (page is Editors.TextEditor)
                {
                    if (((Editors.TextEditor)page).FilePath == curFile.FullName)
                    {
                        tabs.SelectedTab = (TabPage)page;
                        return;
                    }
                }
            }

            Editors.TextEditor edt = null;
            if (curFile.Extension == ".mia")
            {
                edt = new Editors.ScriptEditor(curFile.FullName, this);
                ((Editors.ScriptEditor)edt).onCommandLineSensed += onCommandLineSensed;
                ((Editors.ScriptEditor)edt).onConstLineSensed += onConstLineSensed;
            }
            if (curFile.Extension == ".dsc")
            {
                edt = new Editors.DecompileViewer(curFile.FullName, this);
                ((Editors.DecompileViewer)edt).onCommandLineSensed += onCommandLineSensed;
            }
            else if (curFile.Extension == ".txt")
            {
                if (currProject.AbsPathToRelative(curFile.FullName).Equals(currProject.PvDbEntryFileName) || curFile.Name.Contains("pv_db"))
                {
                    edt = new Editors.PvDbEditor(curFile.FullName);
                }
                else
                {
                    edt = new Editors.TextEditor(curFile.FullName);
                }
            }

            if (edt != null)
            {
                SpawnTab(edt);
                if(lineNo > 0)
                {
                    edt.GoToLine(lineNo);
                }
            }
            else
            {
                try
                {
                    Process.Start(curFile.FullName);
                }
                catch (Exception ex)
                {
                    SetStatus(ex.Message, true);
                }
            }
        }

        private void onConstLineSensed(object sender, string e)
        {
            try
            {
                Editors.ScriptEditor se = (Editors.ScriptEditor)sender;
                evaluator.AllowRedefinitions = true;
                evaluator.ProcessScript(se.Content, se.FilePath, true, true);
            } 
            catch(CodeError err)
            {
                SetStatus(err.Explanation, true);
            }
        }

        private void onCommandLineSensed(object sender, string e)
        {
            if(MikuASM.DebugBridge.IsConnected && MikuASM.DebugBridge.IsWithEngineHook && Settings.Default.ExecOnInput)
            {
                RunScript(e, "autoeval", true);
            }
        }

        frmWaitAttachment waitIndicator = null;
        private void DebugBridge_OnDetached(object sender, EventArgs e)
        {
            Action x = new Action(delegate ()
            {
                attachToGameToolStripMenuItem.Enabled = true;
                attachToGameToolStripMenuItem.Checked = false;
                interactiveToolStripMenuItem.Enabled = false;
                reattachToolStripMenuItem.Enabled = true;
                evalToolStripMenuItem.Enabled = false;
                bootGameToDebugToolStripMenuItem.Enabled = true;
                playMovieToolStripMenuItem.Enabled = true;
                evaluateUNTILCurrentLineToolStripMenuItem.Enabled = false;
                evaluateCurrentFileToolStripMenuItem.Enabled = false;
                tbbBoot.Enabled = true;
                tbbRun.Enabled = true;
                if (camMover != null)
                {
                    camMover.Close();
                    camMover = null;
                }
                if (charMover != null)
                {
                    charMover.Close();
                    charMover = null;
                }
                HideWaitDialog();
                SetStatus(Strings.StsGameDetached, true);
            });

            if (this.InvokeRequired) this.Invoke(x);
            else x();
        }

        private void DebugBridge_OnAttached(object sender, EventArgs e)
        {
            Action x = new Action(delegate ()
            {
                attachToGameToolStripMenuItem.Enabled = false;
                attachToGameToolStripMenuItem.Checked = true;
                interactiveToolStripMenuItem.Enabled = MikuASM.DebugBridge.IsWithEngineHook;
                reattachToolStripMenuItem.Enabled = false;
                evalToolStripMenuItem.Enabled = MikuASM.DebugBridge.IsWithEngineHook;
                evaluateUNTILCurrentLineToolStripMenuItem.Enabled = MikuASM.DebugBridge.IsWithEngineHook;
                evaluateCurrentFileToolStripMenuItem.Enabled = MikuASM.DebugBridge.IsWithEngineHook;
                bootGameToDebugToolStripMenuItem.Enabled = false;
                playMovieToolStripMenuItem.Enabled = false;
                tbbBoot.Enabled = false;
                tbbRun.Enabled = false;
                if (waitIndicator != null)
                {
                    if (MikuASM.DebugBridge.IsWithEngineHook)
                    {
                        HideWaitDialog();
                    }
                    else
                    {
                        waitIndicator.Description = Strings.MsgPreviewing;
                    }
                }
                SetStatus(Strings.StsGameAttached);
            });

            if (this.InvokeRequired) this.Invoke(x);
            else x();
        }

        private void attachToGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show(Strings.MsgWaitSelector, Strings.WaitSelectorTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
            {
                return;
            }
            MikuASM.DebugBridge.Inject("diva.exe");
        }

        frmCameraMover camMover;
        frmCharaMover charMover;
        private void cameraMoveWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MikuASM.DebugBridge.IsConnected && MikuASM.DebugBridge.IsWithEngineHook)
            {
                if(camMover == null || camMover.IsDisposed)
                {
                    camMover = new frmCameraMover();
                    camMover.Owner = this;
                    camMover.Show();
                } else
                {
                    camMover.BringToFront();
                }
                camMover.Top = this.Top + this.Height / 2 - camMover.Height / 2;
                camMover.Left = this.Left + this.Width / 2 - camMover.Width / 2;
                camMover.LoadCamFromOwner();
            }
            else
            {
                SetStatus(Strings.StsGameNotAttached, true);
            }
        }

        internal Editors.TextEditor ActiveEditor
        {
            get
            {
                if (tabs.SelectedTab is Editors.TextEditor) return (Editors.TextEditor)tabs.SelectedTab;
                else return null;
            }
        }

        internal MikuASM.Compiler evaluator = new MikuASM.Compiler();
        private bool constCachePreheated = false;
        internal void PreheatConstCacheIfNeeded()
        {
            if (currProject.EntryPointFileName != null && !constCachePreheated)
            {
                string fpath = currProject.RelativePathToAbs(currProject.EntryPointFileName);
                var entrypoint = File.ReadAllText(fpath);
                evaluator.AllowRedefinitions = true;
                evaluator.ProcessScript(entrypoint, fpath, true, true);
                constCachePreheated = true;
            }
        }
        private void RunScript(string script, string name, bool safeOnly = false)
        {
            if (MikuASM.DebugBridge.IsConnected && MikuASM.DebugBridge.IsWithEngineHook)
            {
                
                
                try
                {
                    PreheatConstCacheIfNeeded();
                    evaluator.AllowRedefinitions = true;
                    evaluator.outputBuffer.Clear();
                    evaluator.ProcessScript(script, name, true);
                    if(safeOnly)
                    {
                        evaluator.outputBuffer = evaluator.outputBuffer.Where(x =>
                             !(x is MikuASM.Commands.Command_PV_END ||
                             x is MikuASM.Commands.Command_PV_BRANCH_MODE ||
                             x is MikuASM.Commands.Command_END ||
                             x is MikuASM.Commands.Command_PV_END_FADEOUT ||
                             x is MikuASM.Commands.Command_EDIT_TARGET ||
                             x is MikuASM.Commands.Command_TARGET ||
                             x is MikuASM.Commands.Command_TIME)
                         ).ToList();
                    }
                    MikuASM.DebugBridge.SendScript(evaluator.DumpToByteArray());
                    SetStatus(String.Format(Strings.StsEval, script.Trim().Replace("\r", "").Replace("\n", " : ")));
                }
                catch (MikuASM.CodeError ce)
                {
                    SetStatus(ce.Message + ". " + ce.Explanation, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            } else
            {
                SetStatus(Strings.StsGameNotAttached, true);
            }
        }


        private void selectedRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveEditor is Editors.ScriptEditor)
            {
                RunScript(ActiveEditor.CurrentLine, ActiveEditor.FileName+":Fragment");
            } 
            else
            {
                SetStatus(Strings.StsNotScript, true);
            }
        }

        private void reattachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWaitDialog(Strings.StsWaitReattach);
            MikuASM.DebugBridge.Reattach("diva.exe");
        }

        private void charaMoveWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MikuASM.DebugBridge.IsConnected && MikuASM.DebugBridge.IsWithEngineHook)
            {
                if (charMover == null || charMover.IsDisposed)
                {
                    charMover = new frmCharaMover();
                    charMover.Owner = this;
                    charMover.Show();
                }
                else
                {
                    charMover.BringToFront();
                }
                charMover.Top = this.Top + this.Height / 2 - charMover.Height / 2;
                charMover.Left = this.Left + this.Width / 2 - charMover.Width / 2;
                charMover.LoadFromOwner();
            }
            else
            {
                SetStatus(Strings.StsGameNotAttached, true);
            }
        }

        private bool Build()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SetStatus(String.Format(Strings.StsBuildStart, currProject.EntryPointFileName));
            try
            {
                SaveAll();
                Action<CompilerFileEventArgs> status = new Action<CompilerFileEventArgs>(delegate (CompilerFileEventArgs cfea)
                {
                    switch(cfea.Type)
                    {
                        case CompilerFileEventArgs.EventType.Consume:
                            SetStatus(String.Format(Strings.StsCompile, currProject.AbsPathToRelative(cfea.FileName)));
                            break;

                        case CompilerFileEventArgs.EventType.Emit:
                            SetStatus(String.Format(Strings.StsEmit, currProject.AbsPathToRelative(cfea.FileName)));
                            break;

                        default:
                            break;
                    }
                    Application.DoEvents();
                });
                currProject.Build(status);
                treeView.Repopulate();
                sw.Stop();
                SetStatus(String.Format(Strings.StsBuildFinish, sw.Elapsed.ToString("ss\\.fff")));
                SystemSounds.Beep.Play();
                return true;
            }
            catch (MikuASM.CodeError cd)
            {
                BringUpEditor(new FileInfo(cd.FaultyLine.Filename), cd.FaultyLine.LineNo);
                MessageBox.Show(String.Format(Strings.ErrBuildFailure+"\n\n{0}:{1}\n{2}\n{3}\n{4}",
                    cd.FaultyLine.Filename, cd.FaultyLine.LineNo,
                    cd.FaultyLine.ExpandedFrom, cd.Explanation, cd.ExtraHelp), Strings.BuildErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(Strings.ErrBuildFailure, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Strings.ErrBuildFailure+"\n\n{0}",
                    ex.Message), Strings.BuildErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(Strings.ErrBuildFailure, true);
            }
            return false;
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Build();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ActiveEditor != null)
            {
                ActiveEditor.Save();
            }
        }


        private void setGameExePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldPath = Settings.Default.GameExe;
            OpenFileDialog ofd = new OpenFileDialog();
            if (oldPath != null) ofd.FileName = oldPath;
            ofd.Filter = "Game exe|diva.exe";
            if(ofd.ShowDialog() != DialogResult.Cancel)
            {
                Settings.Default.GameExe = ofd.FileName;
                Settings.Default.Save();
            }
        }

        private void bootGameToDebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(Settings.Default.GameExe == null || !File.Exists(Settings.Default.GameExe))
            {
                SetStatus(Strings.ErrGameExeNotFound, true);
                return;
            }
            string pvDb = null;
            if(currProject != null && currProject.PvDbEntryFileName != null)
            {
                pvDb = currProject.PrepareTempPvDb();
            }
            string tmpPvTbl = Path.Combine(Path.GetTempPath(), "meeksdev_pv_lst.farc");
            File.WriteAllBytes(tmpPvTbl, Properties.Resources.gm_pv_list_tbl_pv900);
            ShowWaitDialog(Strings.StsStartingDebug);
            new Thread(
                new ThreadStart(delegate ()
                {
                    MikuASM.DebugBridge.Startup(Settings.Default.GameExe, pvDb, null, null, tmpPvTbl);
                })
            ).Start();
        }

  

        private void playMovieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if(currProject == null)
            {
                SetStatus(Strings.ErrNoProject, true);
                return;
            }
            
            if (!Build()) return;

            string pvDb = currProject.PrepareTempPvDb();
            if (currProject.PvDbEntryFileName == null || !File.Exists(pvDb))
            {
                SetStatus(String.Format(Strings.ErrNotSet, Strings.ThingPvDb), true);
                return;
            }
            string audio = currProject.RelativePathToAbs(currProject.AudioFileName);
            if (currProject.AudioFileName == null || !File.Exists(audio))
            {
                SetStatus(String.Format(Strings.ErrNotSet, Strings.ThingMovieAudio), true);
                return;
            }
            string movieDsc = currProject.RelativePathToAbs(currProject.MovieOnlyDscFileName);
            if (currProject.MovieOnlyDscFileName == null || !File.Exists(movieDsc))
            {
                SetStatus(String.Format(Strings.ErrNotSet, Strings.ThingMovieDsc), true);
                return;
            }

            string tmpPvTbl = Path.Combine(Path.GetTempPath(), "meeksdev_pv_lst.farc");
            File.WriteAllBytes(tmpPvTbl, Properties.Resources.gm_pv_list_tbl_pv900);

            if (Settings.Default.GameExe == null || !File.Exists(Settings.Default.GameExe))
            {
                SetStatus(Strings.ErrGameExeNotFound, true);
                return;
            }

            ShowWaitDialog(Strings.StsStartingPreview);
            new Thread(
                new ThreadStart(delegate ()
                {
                    MikuASM.DebugBridge.Startup(Settings.Default.GameExe, pvDb, movieDsc, audio, tmpPvTbl);
                })
            ).Start();
        }

        private void ShowWaitDialog(string text)
        {
            if (waitIndicator != null)
            {
                HideWaitDialog();
            }
            waitIndicator = new frmWaitAttachment();
            waitIndicator.TopLevel = false;
            waitIndicator.Parent = this;
            this.Controls.Add(waitIndicator);
            waitIndicator.Show();
            waitIndicator.Left =  this.Width / 2 - waitIndicator.Width / 2;
            waitIndicator.Top =  this.Height / 2 - waitIndicator.Height / 2;
            waitIndicator.BringToFront();
            this.Cursor = Cursors.WaitCursor;
            waitIndicator.Description = text;
        }

        private void HideWaitDialog()
        {
            if(waitIndicator != null)
            {
                waitIndicator.Close();
                waitIndicator = null;
            }
            this.Enabled = true;
            this.Cursor = Cursors.Default;
            this.BringToFront();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new frmAbout()).ShowDialog();
        }

        private void executeWhileEditingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ExecOnInput = !Settings.Default.ExecOnInput;
            Settings.Default.Save();
            executeWhileEditingToolStripMenuItem.Checked = Settings.Default.ExecOnInput;
        }

        private void evaluateUNTILCurrentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ActiveEditor is Editors.ScriptEditor)
            {
                RunScript(ActiveEditor.ContentAbove, ActiveEditor.FilePath, true);
            }
        }

        private void evaluateCurrentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ActiveEditor is Editors.ScriptEditor)
            {
                RunScript(ActiveEditor.Content, ActiveEditor.FilePath, true);
            }
        }

        private void treeView_onWantFileEvaluation(object sender, FileInfo e)
        {
            if (MikuASM.DebugBridge.IsConnected && MikuASM.DebugBridge.IsWithEngineHook)
            {
                RunScript(File.ReadAllText(e.FullName), e.FullName, true);
            }
            else
            {
                SetStatus(Strings.StsGameNotAttached, true);
            }
        }

        private void tbbImportFile_Click(object sender, EventArgs e)
        {
            if (currProject != null) treeView.ImportFile();
        }

        private void tbbAddFile_Click(object sender, EventArgs e)
        {
            if(currProject != null) treeView.NewFile();
        }

        private void tbbAddDir_Click(object sender, EventArgs e)
        {
            if (currProject != null) treeView.NewFolder();
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("en");
        }

        private void русскийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("ru");
        }

        private void 日本語ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("ja");
        }

        private void SetLanguage(string lang)
        {
            Program.SetLanguage(lang);
            ComponentResourceManager rsmc = new ComponentResourceManager(typeof(frmMain));
            ApplyLang(this, rsmc);
            ApplyLang(this.menu, rsmc);
        }

        private void ApplyLang(Control ctrl, ComponentResourceManager rsmc)
        {
            ctrl.SuspendLayout();
            rsmc.ApplyResources(ctrl, ctrl.Name, Program.Locale);
            foreach (Control c in ctrl.Controls)
            {
                if (c is ToolStrip)
                {
                    var items = ((ToolStrip)c).AllItems().ToList();
                    foreach (var item in items)
                        rsmc.ApplyResources(item, item.Name);
                } 
                else if (c is ProjectTreeView)
                {
                    ((ProjectTreeView)c).UpdateContextText();
                }
                ApplyLang(c, rsmc);
            }
            ctrl.ResumeLayout();
        }

    }
    public static class ToolStripExtensions
    {
        public static IEnumerable<ToolStripItem> AllItems(this ToolStrip toolStrip)
        {
            return toolStrip.Items.Flatten();
        }
        public static IEnumerable<ToolStripItem> Flatten(this ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripDropDownItem)
                    foreach (ToolStripItem subitem in
                        ((ToolStripDropDownItem)item).DropDownItems.Flatten())
                        yield return subitem;
                yield return item;
            }
        }
    }

    public static class ControlExtensions
    {
        public static IEnumerable<Control> AllControls(this Control control)
        {
            foreach (Control c in control.Controls)
            {
                yield return c;
                foreach (Control child in c.Controls)
                    yield return child;
            }
        }
    }
}
