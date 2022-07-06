using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MeekStudio.Editors
{
    class TextEditor : EditorTab 
    {
        protected Scintilla TextArea;
		public ToolStripMenuItem EditorMenu = new ToolStripMenuItem("Editor");

        public TextEditor() : base()
        {
			TextArea = new Scintilla();
			this.Controls.Add(TextArea);

			// BASIC CONFIG
			TextArea.Dock = DockStyle.Fill;
			TextArea.TextChanged += (this.OnTextChanged);

			// INITIAL VIEW CONFIG
			TextArea.WrapMode = WrapMode.Whitespace;
			TextArea.IndentationGuides = IndentView.LookBoth;

			// STYLING
			InitColors();
			InitSyntaxColoring();

			// NUMBER MARGIN
			InitNumberMargin();

			// BOOKMARK MARGIN
			InitBookmarkMargin();

			// CODE FOLDING MARGIN
			InitCodeFolding();

			EditorMenu.Visible = false;
			InitMenu();
		}

		public TextEditor(string path) : this()
        {
			_FilePath = path;
			Load();
        }

		public string CurrentLine
        {
			get
            {
				var line = TextArea.LineFromPosition(TextArea.CurrentPosition);
				return TextArea.Lines[line].Text;
            }
        }

		public string Content
        {
			get
            {
				return TextArea.Text;
            } 
			set
            {
				TextArea.Text = value;
            }
        }

		public string ContentAbove
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);
				var line = TextArea.LineFromPosition(TextArea.CurrentPosition);
				for(int i = 0; i <= line; i++)
                {
					sw.WriteLine(TextArea.Lines[i].Text);
                }
				return sb.ToString();
			}
			
		}

		public void InsertLineAfterCaret(string line)
        {
			var lidx = TextArea.LineFromPosition(TextArea.CurrentPosition);
			var nextPos = TextArea.Lines[lidx].EndPosition;
			TextArea.InsertText(nextPos, line + "\r\n");
			TextArea.CurrentPosition = nextPos + line.Length;
			SetStatus("Insertion: " + line);
		}

		protected virtual void Load()
        {
			TextArea.Text = File.ReadAllText(FilePath);
			UpdateTitle();
			TextArea.EmptyUndoBuffer();
			SetStatus("Opened " + FileName);
        }

		private bool _hasChanges = false;
		public bool HasChanges { 
			get
			{
				return _hasChanges;
			}
			private set
			{
				_hasChanges = value;
				UpdateTitle();
			}
			
		}

		private string _FilePath = "";
		/// <summary>
		/// Get or set the full file name on the disk for the editor
		/// </summary>
		public string FilePath { 
			get
			{
				return _FilePath;
			}
			set
			{
				_FilePath = value;
				UpdateTitle();
			}
		}

		/// <summary>
		/// Get the project-relative file name for the editor
		/// </summary>
		public string FileName
        {
			get
            {
				return GetRelativeFileName(_FilePath);
			}
        }

		protected virtual void UpdateTitle()
        {
			Text = Path.GetFileName(_FilePath) + (_hasChanges ? "*" : "");
        }

		public virtual void Save()
        {
			File.WriteAllText(FilePath, TextArea.Text);
			SetStatus("Saved " + FileName);
        }

		public void GoToLine(int line)
		{
			var l = TextArea.Lines[line];
			l.Goto();
			TextArea.SelectionStart = l.Position;
			TextArea.SelectionEnd = l.EndPosition;
		}

		protected virtual void InitMenu()
        {

        }

		protected void InitColors()
		{
			TextArea.SetSelectionBackColor(true, IntToColor(0x114D9C));
		}


		protected virtual void InitSyntaxColoring()
		{

			// Configure the default style
			TextArea.StyleResetDefault();
			TextArea.Styles[Style.Default].Font = "Courier New";
			TextArea.Styles[Style.Default].Size = 10;
			TextArea.Styles[Style.Default].BackColor = IntToColor(0xEEEEEE);
			TextArea.Styles[Style.Default].ForeColor = IntToColor(0x000000);
			TextArea.StyleClearAll();
		}

		private void OnTextChanged(object sender, EventArgs e)
		{

		}


		#region Numbers, Bookmarks, Code Folding

		/// <summary>
		/// the background color of the text area
		/// </summary>
		private const int BACK_COLOR = 0xCDCDCD;

		/// <summary>
		/// default text color of the text area
		/// </summary>
		private const int FORE_COLOR = 0x121212;

		/// <summary>
		/// change this to whatever margin you want the line numbers to show in
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		/// <summary>
		/// change this to whatever margin you want the bookmarks/breakpoints to show in
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;
		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		/// change this to whatever margin you want the code folding tree (+/-) to show in
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		/// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = true;

		private void InitNumberMargin()
		{

			TextArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
			TextArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
			TextArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			TextArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

			var nums = TextArea.Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;

			TextArea.MarginClick += TextArea_MarginClick;
		}

		private void InitBookmarkMargin()
		{

			//TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

			var margin = TextArea.Margins[BOOKMARK_MARGIN];
			margin.Width = 0;
			margin.Sensitive = false;
			margin.Type = MarginType.Symbol;
			margin.Mask = (1 << BOOKMARK_MARKER);
			//margin.Cursor = MarginCursor.Arrow;

			var marker = TextArea.Markers[BOOKMARK_MARKER];
			marker.Symbol = MarkerSymbol.Circle;
			marker.SetBackColor(IntToColor(0xFF003B));
			marker.SetForeColor(IntToColor(0x000000));
			marker.SetAlpha(100);

		}

		private void InitCodeFolding()
		{

			TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
			TextArea.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

			// Enable code folding
			TextArea.SetProperty("fold", "1");
			TextArea.SetProperty("fold.compact", "1");

			// Configure a margin to display folding symbols
			TextArea.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			TextArea.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			TextArea.Margins[FOLDING_MARGIN].Sensitive = true;
			TextArea.Margins[FOLDING_MARGIN].Width = 20;

			// Set colors for all folding markers
			for (int i = 25; i <= 31; i++)
			{
				TextArea.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
				TextArea.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
			}

			// Configure folding markers with respective symbols
			TextArea.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			TextArea.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			TextArea.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			TextArea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			TextArea.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			TextArea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			TextArea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic folding
			TextArea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

		}

		private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (e.Margin == BOOKMARK_MARGIN)
			{
				// Do we have a marker for this line?
				const uint mask = (1 << BOOKMARK_MARKER);
				var line = TextArea.Lines[TextArea.LineFromPosition(e.Position)];
				if ((line.MarkerGet() & mask) > 0)
				{
					// Remove existing bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add bookmark
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}

        #endregion

        public static Color IntToColor(int rgb)
		{
			return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		}

		public void InvokeIfNeeded(Action action)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(action);
			}
			else
			{
				action.Invoke();
			}
		}
	}
}
