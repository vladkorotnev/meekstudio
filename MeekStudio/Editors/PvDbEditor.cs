using MeekStudio.Locales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio.Editors
{
    class PvDbEditor : TextEditor
    {
        public PvDbEditor(string path) : base(path)
        {
            
        }

        protected override void InitMenu()
        {
            base.InitMenu();
            EditorMenu.Text = Strings.MnuPvDb;

            ToolStripMenuItem sortMenuItem = new ToolStripMenuItem(Strings.MnuPvDbCleanUp);
            sortMenuItem.ShortcutKeys = Keys.Control | Keys.K;

            sortMenuItem.Click += SortMenuItem_Click;
            EditorMenu.DropDownItems.Add(sortMenuItem);

            EditorMenu.Visible = true;
        }

        private void SortMenuItem_Click(object sender, EventArgs e)
        {
            Language.PvDbEntryTextProcessor processor = new Language.PvDbEntryTextProcessor(TextArea.Text);
            TextArea.Text = processor.Content;
            SetStatus(Strings.StsPvDbCleanedUp);
        }
    }
}
