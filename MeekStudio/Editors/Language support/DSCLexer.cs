using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MikuASM;
using static MikuASM.Compiler;

namespace MeekStudio.Language
{
    class DSCLexer
    {
        private Compiler checker = new Compiler();

        public const int StyleDefault = 0;
        public const int StyleKeyword = 1;
        public const int StyleIdentifier = 2;
        public const int StyleNumber = 3;
        public const int StylePreprocessor = 4;
        public const int StyleComment = 5;
        public const int StylePath = 6;
        public const int StyleConst = 7;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_IDENTIFIER = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_PREPROCESSOR = 3;
        private const int STATE_COMMENT = 4;
        private const int STATE_PATH = 5;
        private const int STATE_CONST = 6;

        private HashSet<string> keywords;

        public void Style(Scintilla scintilla, int startPos, int endPos)
        {
            // Back up to the line start
            var line = scintilla.LineFromPosition(startPos);
            var curLine = scintilla.Lines[line];
            startPos = curLine.Position;

            var length = 0;
            var state = STATE_UNKNOWN;
            string directive = "";

            // Start styling
            while (startPos < endPos)
            {
                var c = (char)scintilla.GetCharAt(startPos);

            REPROCESS:
                switch (state)
                {
                    case STATE_UNKNOWN:
                        if (c == '#')
                        {
                            scintilla.StartStyling(startPos);
                            state = STATE_PREPROCESSOR;
                            goto REPROCESS;
                        }
                        else if (c == scintilla.GetCharAt(startPos + 1) && (c == '/' || c == '-'))
                        {
                            scintilla.StartStyling(startPos);
                            state = STATE_COMMENT;
                            goto REPROCESS;
                        }
                        else if (c == '@' || c == '$' || c == '.')
                        {
                            scintilla.StartStyling(startPos);
                            scintilla.SetStyling(1, StyleKeyword);
                        }
                        else if (Char.IsDigit(c))
                        {
                            scintilla.StartStyling(startPos);
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        }
                        else if (Char.IsLetter(c))
                        {
                            scintilla.StartStyling(startPos);
                            state = STATE_IDENTIFIER;
                            goto REPROCESS;
                        }
                        else
                        {
                            // Everything else
                            scintilla.StartStyling(startPos);
                            scintilla.SetStyling(1, StyleDefault);
                        }
                        break;

                    case STATE_COMMENT:
                        if(c == '\n' || startPos >= curLine.EndPosition)
                        {
                            scintilla.SetStyling(length, StyleComment);
                            length = 0;
                            state = STATE_UNKNOWN;
                        } else
                        {
                            length++;
                        }
                        break;


                    case STATE_PREPROCESSOR:
                        if (!Char.IsLetter(c) && !Char.IsPunctuation(c))
                        {
                            scintilla.SetStyling(length, StylePreprocessor);
                            length = 0;

                            switch(directive.ToUpper())
                            {
                                /*case PreprocessorDirectives.INCLUDE:
                                case PreprocessorDirectives.WRITE_FILE:
                                case PreprocessorDirectives.INCLUDE_BINARY:
                                    state = STATE_PATH;
                                    break;

                                case PreprocessorDirectives.CONST_DEF:
                                    state = STATE_CONST;
                                    break;

                                case PreprocessorDirectives.CONST_UNDEF:
                                    state = STATE_CONST;
                                    break;

                                case PreprocessorDirectives.SET_BINARY_FILTER:
                                    state = STATE_IDENTIFIER;
                                    break;


                                case PreprocessorDirectives.PUSH_CONTEXT:
                                case PreprocessorDirectives.POP_CONTEXT:
                                case PreprocessorDirectives.FORCE_STOP:
                                case PreprocessorDirectives.SORT_FORCE:
                                case PreprocessorDirectives.SET_VERBOSE:
                                case PreprocessorDirectives.CLEAR_BUFFER:*/
                                default:
                                    state = STATE_UNKNOWN;
                                    break;
                            }
                            
                        }
                        else
                        {
                            length++;
                            if(c != '#')
                            {
                                directive += c;
                            }
                        }
                        break;

                    case STATE_NUMBER:
                        if (Char.IsDigit(c))
                        {
                            length++;
                        }
                        else
                        {
                            scintilla.SetStyling(length, StyleNumber);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        break;

                    case STATE_CONST:
                    case STATE_IDENTIFIER:
                        if (Char.IsLetterOrDigit(c) || c == '_')
                        {
                            length++;
                        }
                        else
                        {
                            var identifier = scintilla.GetTextRange(startPos - length, length);
                            if (keywords.Contains(identifier.ToUpper()))
                            {
                                scintilla.SetStyling(length, StyleKeyword);
                            } 

                                                            
                            length = 0;
                            if(state == STATE_CONST && c == '=')
                            {
                                state = STATE_UNKNOWN;
                            } 
                            else
                            {
                                state = STATE_UNKNOWN;
                            }
                        }
                        break;

                    case STATE_PATH:
                        if(Char.IsLetterOrDigit(c) || Char.IsPunctuation(c))
                        {
                            length++;
                        } 
                        else
                        {
                            scintilla.SetStyling(length, StylePath);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        break;
                }

                startPos++;
            }
        }

        public DSCLexer()
        {
            // Put keywords in a HashSet
            var list = Translator.GetKeywords();
            this.keywords = new HashSet<string>(list);
        }
    }
}
