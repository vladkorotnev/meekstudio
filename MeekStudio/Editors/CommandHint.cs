using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeekStudio.Editors
{
    public partial class CommandHint : UserControl
    {
        public CommandHint()
        {
            InitializeComponent();
        }

        void SetLayout()
        {
            this.Invalidate();
        }

        private Font syntaxFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Bold);
        private Font hintFont = new Font(FontFamily.GenericSansSerif, 8);

        private string _syntax = "";
        public string Syntax
        {
            get
            {
                return _syntax;
            }
            set
            {
                _syntax = value;
                SetLayout();
            }
        }

        private string _hint = "";
        public string Hint
        {
            get
            {
                return _hint;
            }
            set
            {
                _hint = value;
                SetLayout();
            }
        }

        private void CommandHint_Paint(object sender, PaintEventArgs e)
        {
            int desiredWidth = Math.Min(325, this.Parent.Width) - 2;

            SizeF syntaxSz = e.Graphics.MeasureString(_syntax, syntaxFont);
            if (syntaxSz.Width > desiredWidth)
            {
                int iLines = (int)(Math.Round((syntaxSz.Width / desiredWidth) + .5));
                syntaxSz.Width = desiredWidth;
                syntaxSz.Height = iLines * syntaxSz.Height * 1.1f;
            } 
            else
            {
                desiredWidth = (int)Math.Round(syntaxSz.Width)+1;
            }
            e.Graphics.DrawString(_syntax, syntaxFont, Brushes.Black, new RectangleF(new PointF(1,1), syntaxSz));

            SizeF hintSz = e.Graphics.MeasureString(_hint, hintFont);
            if(hintSz.Width > desiredWidth)
            {
                int iLines = (int)(Math.Round((hintSz.Width / desiredWidth) + .5));
                hintSz.Width = desiredWidth;
                hintSz.Height = iLines * hintSz.Height * 1.1f;
            }
            e.Graphics.DrawString(_hint, hintFont, Brushes.Black, new RectangleF(new PointF(1, syntaxSz.Height+2), hintSz));

            this.Width = desiredWidth;
            this.Height = (int)Math.Round(2 + syntaxSz.Height + 2 + hintSz.Height);

            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
        }
    }
}
