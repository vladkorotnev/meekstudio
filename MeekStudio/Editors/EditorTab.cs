using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio.Editors
{
    class EditorTab : TabPage
    {
        public frmMain OwnerForm { get; set; }

        protected string GetRelativeFileName(string absPath)
        {
            if (OwnerForm == null || OwnerForm.currProject == null) return absPath;
            else
            {
                return OwnerForm.currProject.AbsPathToRelative(absPath);
            }
        }

        protected void SetStatus(string text, bool isError = false)
        {
            if(OwnerForm != null)
            {
                OwnerForm.SetStatus(text, isError);
            }
        }
    }
}
