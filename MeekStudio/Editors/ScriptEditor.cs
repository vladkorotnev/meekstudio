using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScintillaNET;
using System.Windows.Forms;
using System.Drawing;
using MikuASM;
using MeekStudio.Locales;

namespace MeekStudio.Editors
{
    class ScriptEditor : TextEditor
    {
        private Language.DSCLexer lexer = new Language.DSCLexer();
        private static string[] commandNames = null;
        private CommandHint hintView = new CommandHint();

        public ScriptEditor() : base()
        {

        }
        public ScriptEditor(string path, frmMain owner) : this()
        {
            OwnerForm = owner;
            OwnerForm.Controls.Add(hintView);
            hintView.Parent = OwnerForm;
            hintView.Visible = false;
            
            FilePath = path;
            Load();
            RestyleFull();
            BuildAutocompleteList();
            TextArea.AutoCIgnoreCase = true;
            TextArea.CharAdded += AutocompletionEvent;
            TextArea.MouseClick += HideAutocompleteEvent;
            TextArea.Delete += HideAutocompleteEvent;
            TextArea.Leave += HideAutocompleteEvent;
            TextArea.LostFocus += HideAutocompleteEvent;
            TextArea.UpdateUI += TextArea_UpdateUI;
        }

        private void TextArea_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            if(e.Change != UpdateChange.Content)
            {
                hintView.Visible = false;
            }
        }

        private void HideAutocompleteEvent(object sender, object e)
        {
            hintView.Visible = false;
        }

        private static void BuildAutocompleteList()
        {
            if(commandNames == null)
            {
                commandNames = Translator.GetKeywords();
                Array.Sort(commandNames, (x, y) => String.Compare(x, y));
            }
        }

        public event EventHandler<string> onCommandLineSensed;
        public event EventHandler<string> onConstLineSensed;

        private void AutocompletionEvent(object sender, CharAddedEventArgs e)
        {
            Scintilla scintilla = (Scintilla)sender;

            // Find the word start
            var currentPos = scintilla.CurrentPosition;
            var wordStartPos = scintilla.WordStartPosition(currentPos, true);
            var lenEntered = currentPos - wordStartPos;


            if (lenEntered > 2)
            {
                scintilla.AutoCShow(lenEntered, string.Join(" ", commandNames));
            }

            var line = scintilla.Lines[scintilla.LineFromPosition(currentPos)];
            if(line.Text.Length > 0)
            {
                if(line.Text.Trim().ToUpper().StartsWith("#" + Compiler.PreprocessorDirectives.CONST_DEF))
                {
                    if(onConstLineSensed != null)
                    {
                        onConstLineSensed.Invoke(this, line.Text);
                    }
                } 
                else
                {
                    try
                    {
                        var cmd = Translator.ExtractCommandName(line.Text);
                        if (cmd != null)
                        {
                            Type cmdType = Translator.ExtractCommand(cmd);
                            if (cmdType != null)
                            {
                                // show quick help
                                string help = DSCCommand.GetHelpForType(cmdType);
                                if (help == null)
                                {
                                    help = Strings.HlpNoneAvail;
                                }

                                onCommandLineSensed.Invoke(this, line.Text);

                                hintView.Syntax = Translator.CommandNameWithArgs(cmdType, " ");
                                hintView.Hint = help;
                                hintView.Visible = true;
                                Point loc = new Point(scintilla.PointXFromPosition(wordStartPos), scintilla.PointYFromPosition(line.Position) + line.Height);
                                Point trans = hintView.Parent.PointToClient(scintilla.PointToScreen(loc));
                                hintView.Location = trans;
                                hintView.BringToFront();
                            }
                        }
                        else
                        {
                            hintView.Visible = false;
                        }
                    }
                    catch (Exception exc)
                    {
                        hintView.Visible = false;
                    }
                }
            }
        }

        private void RestyleFull()
        {
            foreach(var line in TextArea.Lines)
            {
                lexer.Style(TextArea, line.Position, line.EndPosition);
            }
        }

        private void TextArea_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            var startPos = TextArea.GetEndStyled();
            var endPos = e.Position;

            lexer.Style(TextArea, startPos, endPos);
        }


        protected override void InitSyntaxColoring()
        {
            base.InitSyntaxColoring();

            TextArea.Styles[Language.DSCLexer.StyleDefault].ForeColor = Color.Black;
            TextArea.Styles[Language.DSCLexer.StyleKeyword].ForeColor = Color.Blue;
            TextArea.Styles[Language.DSCLexer.StyleIdentifier].ForeColor = Color.Red;
            TextArea.Styles[Language.DSCLexer.StyleNumber].ForeColor = Color.Purple;
            TextArea.Styles[Language.DSCLexer.StylePreprocessor].ForeColor = Color.DarkGray;
            TextArea.Styles[Language.DSCLexer.StyleComment].ForeColor = Color.Green;

            TextArea.Styles[Language.DSCLexer.StylePath].ForeColor = Color.OrangeRed;
            TextArea.Styles[Language.DSCLexer.StyleConst].ForeColor = Color.BlueViolet;


            TextArea.Lexer = Lexer.Container;

            TextArea.StyleNeeded += (this.TextArea_StyleNeeded);
        }
    }
}
