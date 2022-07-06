using MeekStudio.Locales;
using MikuASM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MeekStudio
{
    [Serializable]
    public class Project
    {
        const string META_FILE_NAME = "meta.msproj";
        const int DEBUG_PV_ID = 900;

        public string Location { get; private set; }

        public string Name { get; set; }
        public int PvId { get; set; }

        public string EntryPointFileName { get; set; }
        public string PvDbEntryFileName { get; set; }
        public string MovieOnlyDscFileName { get; set; }
        public string AudioFileName { get; set; }


        public string PrepareTempPvDb()
        {
            try
            {
                string pvDbPath = Path.Combine(Path.GetTempPath(), "meeksdev_temp_pvdb.txt");
                var myPvDbSrc = File.ReadAllText(RelativePathToAbs(PvDbEntryFileName));
                var myPvDbDest = File.CreateText(pvDbPath);

                var proc = new Language.PvDbEntryTextProcessor(myPvDbSrc);
                proc.SetPvId(DEBUG_PV_ID);

                // If no easy DSC, move one to easy
                if(proc.GetByKeyPath(proc.KeyPathFromRoot("difficulty.easy.0")) == null)
                {
                    object atLeastSomeLevel = null;
                    foreach (string difficulty in new string[] { "normal", "hard", "extreme", "encore" })
                    {
                        int curDifCount = Convert.ToInt32(proc.GetByKeyPath(proc.KeyPathFromRoot("difficulty." + difficulty + ".length")));
                        if (curDifCount > 0)
                        {
                            atLeastSomeLevel = proc.GetByKeyPath(proc.KeyPathFromRoot("difficulty." + difficulty + ".0"));
                        }
                    }

                    if (atLeastSomeLevel != null)
                    {
                        proc.SetByKeyPath(proc.KeyPathFromRoot("difficulty.easy.0"), atLeastSomeLevel);
                        proc.SetByKeyPath(proc.KeyPathFromRoot("difficulty.easy.0.edition"), "0");
                    }
                }

                myPvDbDest.WriteLine(proc.Content);

                string dummyData = Properties.Resources.dummy_pv_db_txt;
                myPvDbDest.WriteLine(dummyData);
                myPvDbDest.Close();

                return pvDbPath;
            } catch(Exception exc) { }
            return "";
        }

        public void SaveMeta()
        {
            Stream s = File.OpenWrite(Path.Combine(Location, META_FILE_NAME));
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, this);
            s.Close();
        }

        public static Project FromFolder(string folder)
        {
            string metaFilePath = Path.Combine(folder, META_FILE_NAME);
            if(!File.Exists(metaFilePath))
            {
                throw new InvalidOperationException(Strings.ErrNotProject);
            } 
            else
            {
                Stream s = File.OpenRead(metaFilePath);
                BinaryFormatter b = new BinaryFormatter();
                var proj = (Project) b.Deserialize(s);
                s.Close();
                proj.Location = folder;
                return proj;
            }
        }

        public static Project CreateInFolder(string folder, string name)
        {
            string metaFilePath = Path.Combine(folder, META_FILE_NAME);
            if (File.Exists(metaFilePath))
            {
                throw new InvalidOperationException(Strings.ErrAlreadyProject);
            } else
            {
                Project p = new Project();
                p.Location = folder;
                p.Name = name;
                p.EntryPointFileName = "main.mia";

                File.WriteAllText(Path.Combine(p.Location, "main.mia"), Properties.Resources.project_main_tpl_txt);
                
                Directory.CreateDirectory(Path.Combine(p.Location, "output"));
                Directory.CreateDirectory(Path.Combine(p.Location, "charts"));
                Directory.CreateDirectory(Path.Combine(p.Location, "song"));
                Directory.CreateDirectory(Path.Combine(p.Location, "scenes"));
                Directory.CreateDirectory(Path.Combine(p.Location, "pv_db"));

                File.WriteAllText(Path.Combine(p.Location, "pv_db", "pv_db_entry.txt"), Properties.Resources.pv_db_entry_txt);


                p.SaveMeta();

                return p;
            }
        }

        public void Build(Action<CompilerFileEventArgs> fileProcessHandler = null)
        {
            var compiler = new MikuASM.Compiler();
            if(fileProcessHandler != null)
            {
                compiler.onFileProcessed += new EventHandler<CompilerFileEventArgs>(delegate (object sender, CompilerFileEventArgs fe)
                {
                    fileProcessHandler(fe);
                });
            }
            string fpath = RelativePathToAbs(EntryPointFileName);
            var entrypoint = File.ReadAllText(fpath);
            compiler.ProcessScript(entrypoint, fpath, true);
        }

        public string AbsPathToRelative(string abs)
        {
            if (abs == null) return "";
            return abs.Replace(this.Location, "").TrimStart('/', '\\');
        }

        public string RelativePathToAbs(string rel)
        {
            if (rel == null) return "";
            return Path.Combine(this.Location, rel);
        }
    }
}
