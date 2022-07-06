using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio
{
    public class FileSystemRenameEventArgs : EventArgs
    {
        public FileSystemInfo oldInfo { get; private set; }
        public FileSystemInfo newInfo { get; private set; }
        internal FileSystemRenameEventArgs(FileSystemInfo from, FileSystemInfo to)
        {
            oldInfo = from;
            newInfo = to;
        }
    }

    public class ProjectMetaEditedEventArgs : EventArgs
    {
        public enum MetaType
        {
            Invalid,
            Title,
            PvDb,
            MovieOut,
            MovieSound,
            Entrypoint
        }

        public MetaType changedMeta { get; private set; }
        public bool isSuccess { get; private set; }

        internal ProjectMetaEditedEventArgs(MetaType type, bool success)
        {
            changedMeta = type;
            isSuccess = success;
        }
    }

    public partial class ProjectTreeView
    {
        private ImageList icons = new ImageList();
        private enum IconID
        {
            Dir = 0,
            File = 1,
            Code = 2,
            DSC = 3,
            Sound = 4,
            PVDB = 5
        }

        private Project currProject;
        public Project Project
        {
            get
            {
                return currProject;
            }
            set
            {
                currProject = value;
                if(currProject == null)
                {
                    Nodes.Clear();
                } else
                {
                    Repopulate();
                }
            }
        }

        public void Repopulate()
        {
            Nodes.Clear();
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(currProject.Location);
            if (info.Exists)
            {
                rootNode = new TreeNode(currProject.Name);
                rootNode.Tag = info;
                GetDirectories(info, rootNode);
                Nodes.Add(rootNode);
                rootNode.Expand();
            }
        }

        private void GetDirectories(DirectoryInfo dir,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                if (subDir.Name.StartsWith(".")) continue;
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageIndex = (int)IconID.Dir;
                aNode.SelectedImageIndex = aNode.ImageIndex;
                GetDirectories(subDir, aNode);
                nodeToAddTo.Nodes.Add(aNode);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name.StartsWith(".") || file.Name.EndsWith(".msproj")) continue;
                aNode = CreateNodeForFile(file);
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private TreeNode CreateNodeForFile(FileInfo file)
        {
            var aNode = new TreeNode(file.Name);
            aNode.Tag = file;
            if (file.FullName.Equals(currProject.RelativePathToAbs(currProject.EntryPointFileName)))
            {
                aNode.BackColor = Color.LightGreen;
            }
            else if (file.FullName.Equals(currProject.RelativePathToAbs(currProject.PvDbEntryFileName)) ||
              file.FullName.Equals(currProject.RelativePathToAbs(currProject.AudioFileName)) ||
              file.FullName.Equals(currProject.RelativePathToAbs(currProject.MovieOnlyDscFileName)))
            {
                aNode.BackColor = Color.LightSalmon;
            }

            if (file.Name.Contains("pv_db") || file.FullName.Equals(currProject.RelativePathToAbs(currProject.PvDbEntryFileName)))
            {
                aNode.ImageIndex = (int)IconID.PVDB;
            }
            else if (file.Extension.ToLower() == ".dsc")
            {
                aNode.ImageIndex = (int)IconID.DSC;
            }
            else if (file.Extension.ToLower() == ".mia")
            {
                aNode.ImageIndex = (int)IconID.Code;
            }
            else if (file.Extension.ToLower() == ".ogg")
            {
                aNode.ImageIndex = (int)IconID.Sound;
            }
            else
            {
                aNode.ImageIndex = (int)IconID.File;
            }
            aNode.SelectedImageIndex = aNode.ImageIndex;
            return aNode;
        }

        private enum CreationStatus
        {
            Not,
            File,
            Folder
        }
        private CreationStatus curTreeCreation = CreationStatus.Not;
        private DirectoryInfo curCreationDir = null;
        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFolder();
        }

        public void NewFolder()
        {
            if (currProject == null) return;
            TreeNode createPlace = Nodes[0];
            if (SelectedNode != null)
            {
                if (SelectedNode.Tag is DirectoryInfo)
                {
                    createPlace = SelectedNode;
                }
                else if (SelectedNode.Parent.Tag is DirectoryInfo)
                {
                    createPlace = SelectedNode.Parent;
                }
            }

            curCreationDir = (DirectoryInfo)createPlace.Tag;

            curTreeCreation = CreationStatus.Folder;

            TreeNode newNode = new TreeNode("untitled");
            newNode.ImageIndex = (int)IconID.Dir;
            createPlace.Nodes.Add(newNode);
            Focus();
            SelectedNode = newNode;
            newNode.BeginEdit();
        }

        public event EventHandler<ProjectMetaEditedEventArgs> onProjectMetaChange;
        public event EventHandler<FileSystemRenameEventArgs> onEntityRenamed;

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == Nodes[0])
            {
                currProject.Name = e.Label;
                currProject.SaveMeta();
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.Title, true));
            }
            else
            {
                switch (curTreeCreation)
                {
                    case CreationStatus.Not:
                        // rename
                        {
                            string location = null;
                            string origin = null;
                            bool isDir = false;
                            FileSystemInfo oldInfo = null;

                            if (e.Node.Tag is FileInfo)
                            {
                                var fi = (FileInfo)e.Node.Tag;
                                oldInfo = fi;
                                location = fi.DirectoryName;
                                origin = fi.FullName;
                            }
                            else if (e.Node.Tag is DirectoryInfo)
                            {
                                var di = (DirectoryInfo)e.Node.Tag;
                                oldInfo = di;
                                location = di.Parent.FullName;
                                origin = di.FullName;
                                isDir = true;
                            }
                            else
                            {
                                e.CancelEdit = true;
                                return;
                            }

                            string newname = Util.RemoveInvalidCharacters(e.Label);
                            if(location == null || newname == null)
                            {
                                e.CancelEdit = true;
                                return;
                            }
                            
                            string fname = Path.Combine(location, newname);
                            if (File.Exists(fname) || Directory.Exists(fname))
                            {
                                e.CancelEdit = true;
                            }
                            else
                            {
                                if (isDir)
                                {
                                    Directory.Move(origin, fname);
                                    var newInfo = new DirectoryInfo(fname);
                                    e.Node.Tag = newInfo;
                                    onEntityRenamed.Invoke(this, new FileSystemRenameEventArgs(oldInfo, newInfo));
                                }
                                else
                                {
                                    File.Move(origin, fname);
                                    var newInfo = new FileInfo(fname); 
                                    e.Node.Tag = newInfo;
                                    onEntityRenamed.Invoke(this, new FileSystemRenameEventArgs(oldInfo, newInfo));
                                }
                            }
                        }
                        break;

                    case CreationStatus.File:
                        // new file
                        {
                            if (e.Label == null)
                            {
                                e.Node.Remove();
                                curTreeCreation = CreationStatus.Not;
                                return;
                            }
                            string name = Util.RemoveInvalidCharacters(e.Label);
                            string fname = Path.Combine(curCreationDir.FullName, name);
                            if (File.Exists(fname) || Directory.Exists(fname))
                            {
                                e.Node.Remove();
                            }
                            else
                            {
                                var stream = File.Create(fname);
                                stream.Close();
                                e.Node.Tag = new FileInfo(fname);
                            }
                        }
                        curTreeCreation = CreationStatus.Not;
                        break;

                    case CreationStatus.Folder:
                        // new dir
                        {
                            if (e.Label == null)
                            {
                                e.Node.Remove();
                                curTreeCreation = CreationStatus.Not;
                                return;
                            }
                            string name = Util.RemoveInvalidCharacters(e.Label);
                            string dirname = Path.Combine(curCreationDir.FullName, name);
                            if (Directory.Exists(dirname) || File.Exists(dirname))
                            {
                                e.Node.Remove();
                            }
                            else
                            {
                                Directory.CreateDirectory(dirname);
                                e.Node.Tag = new DirectoryInfo(dirname);
                            }
                        }
                        curTreeCreation = CreationStatus.Not;
                        break;
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null) return;
            SelectedNode.BeginEdit();
        }

        private void setAsEntrypointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null) return;
            if (SelectedNode.Tag is FileInfo)
            {
                var fi = (FileInfo)SelectedNode.Tag;
                currProject.EntryPointFileName = currProject.AbsPathToRelative(fi.FullName);
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.Entrypoint, true));
                currProject.SaveMeta();
            }
            else
            {
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.Entrypoint, false));
            }
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        public void NewFile()
        {
            if (currProject == null) return;
            TreeNode createPlace = Nodes[0];
            if (SelectedNode != null)
            {
                if (SelectedNode.Tag is DirectoryInfo)
                {
                    createPlace = SelectedNode;
                }
                else if (SelectedNode.Parent.Tag is DirectoryInfo)
                {
                    createPlace = SelectedNode.Parent;
                }
            }

            curCreationDir = (DirectoryInfo)createPlace.Tag;

            curTreeCreation = CreationStatus.File;

            TreeNode newNode = new TreeNode("untitled.mia");
            newNode.ImageIndex = (int)IconID.Code;
            createPlace.Nodes.Add(newNode);
            Focus();
            SelectedNode = newNode;
            newNode.BeginEdit();
        }

        public void ImportFile()
        {
            if (SelectedNode == null || !(SelectedNode.Tag is DirectoryInfo)) return;
            var curDir = (DirectoryInfo)SelectedNode.Tag;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = Strings.ImportFileTitle;
            ofd.Filter = "Sensible files|*.DSC; *.TXT; *.OGG; *.MIA; *.PJE|All files|*.*";
            if (ofd.ShowDialog() != DialogResult.Cancel)
            {
                string dstPath = Path.Combine(curDir.FullName, Path.GetFileName(ofd.FileName));
                if (File.Exists(dstPath)) return;
                File.Copy(ofd.FileName, dstPath);
                var node = CreateNodeForFile(new FileInfo(dstPath));
                SelectedNode.Nodes.Add(node);
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFile();
        }

        public event EventHandler<FileSystemInfo> onFileDeleted;

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null || SelectedNode == Nodes[0]) return;
            if (SelectedNode.Tag is FileSystemInfo)
            {
                var fi = (FileSystemInfo)SelectedNode.Tag;
                string relPath = currProject.AbsPathToRelative(fi.FullName);
                if (MessageBox.Show(string.Format(Strings.DeleteMsg, relPath), Strings.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    onFileDeleted.Invoke(this, fi);
                    if (fi is DirectoryInfo)
                    {
                        Directory.Delete(fi.FullName, true);
                    }
                    else if (fi is FileInfo)
                    {
                        File.Delete(fi.FullName);
                    }
                    SelectedNode.Remove();
                }
            }

        }

        private void setAsPVDBEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null) return;
            if (SelectedNode.Tag is FileInfo)
            {
                var fi = (FileInfo)SelectedNode.Tag;
                currProject.PvDbEntryFileName = currProject.AbsPathToRelative(fi.FullName);
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.PvDb, true));
                currProject.SaveMeta();
            }
            else
            {
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.PvDb, false));
            }
        }

        private void movieonlyDSCFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null) return;
            if (SelectedNode.Tag is FileInfo)
            {
                var fi = (FileInfo)SelectedNode.Tag;
                currProject.MovieOnlyDscFileName = currProject.AbsPathToRelative(fi.FullName);
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.MovieOut, true));
                currProject.SaveMeta();
            }
            else
            {
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.MovieOut, false));
            }
        }

        private void backgroundAudioFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currProject == null || SelectedNode == null) return;
            if (SelectedNode.Tag is FileInfo)
            {
                var fi = (FileInfo)SelectedNode.Tag;
                currProject.AudioFileName = currProject.AbsPathToRelative(fi.FullName);
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.MovieSound, true));
                currProject.SaveMeta();
            }
            else
            {
                onProjectMetaChange.Invoke(this, new ProjectMetaEditedEventArgs(ProjectMetaEditedEventArgs.MetaType.MovieSound, false));
            }
        }

        public event EventHandler<FileInfo> onWantFileEvaluation;
        private void EvalFile_Click(object sender, EventArgs e)
        {
            if (currProject == null || !(SelectedNode.Tag is FileInfo)) return;
            if(onWantFileEvaluation != null)
            {
                onWantFileEvaluation.Invoke(this, (FileInfo)SelectedNode.Tag);
            }
        }
    }

}
