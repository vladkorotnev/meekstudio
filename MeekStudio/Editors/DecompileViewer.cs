using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeekStudio.Editors
{
    class DecompileViewer : ScriptEditor
    {
        public DecompileViewer(string path, frmMain owner) : base(path, owner)
        {

        }

        override public void Save()
        {
            // no-op
        }

        override protected void Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var stringBuilder = new StringBuilder();
                var stringWriter = new StringWriter(stringBuilder);

                var fileContent = MikuASM.DSCReader.ReadFromFile(FilePath);
                foreach(var cmd in fileContent.Commands)
                {
                    var cmdInner = cmd.Unwrap();
                    if(!(cmdInner is MikuASM.Commands.Command_TIME || cmdInner is MikuASM.Commands.Command_PV_BRANCH_MODE))
                    {
                        stringWriter.Write(new string(' ', Math.Max(TextArea.IndentWidth, 4)));
                    } 
                    stringWriter.WriteLine(cmdInner.ToString());
                }
                stringWriter.Close();

                TextArea.Text = stringBuilder.ToString();
                TextArea.ReadOnly = true;
                TextArea.EmptyUndoBuffer();
                sw.Stop();
                SetStatus(string.Format(Strings.StsDecompiled, FileName, sw.Elapsed.ToString("ss\\.fff")));
            } 
            catch(Exception e)
            {
                SetStatus(string.Format(Strings.StsDecompileError, FileName, e.Message), true);
                TextArea.Text = Strings.DecompileError + "\n" + e.Message + "\n" + e.StackTrace;
                TextArea.ReadOnly = true;
            }
        }

        protected override void UpdateTitle()
        {
            base.UpdateTitle();
            Text = String.Format(Strings.DecompileTitle, Text);
        }
    }
}
