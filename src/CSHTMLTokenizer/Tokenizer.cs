using CSHTMLTokenizer.Tokens;
using Stateless;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSHTMLTokenizer
{
    public class Tokenizer
    {
        private enum State
        {
            Start, Data, TagOpen, EOF, TagName, SelfClosingStartTag, EndTagOpen, BeforeAttributeName, AttributeName,
            AfterAttributeName, BeforeAttributeValue, AttributeValue, AfterAttributeValue, Quote, CSBlock, CSLine, BeforeCS,
            EndOfLine, BeforeAttributeNameEndOfLine, BeforeAttributeNameWhiteSpace, BeforeQuotedStringEndOfLine
        }

        private enum Trigger
        {
            GotChar, EOF, OpenTag, TagName, Data, SelfClosingStartTag, EndTagOpen, BeforeAttributeName, AttributeName,
            AfterAttributeName, BeforeAttributeValue, AttributeValue, AfterAttributeValue, Quote, CSBlock, CSLine, BeforeCS,
            EndOfLine, BeforeAttributeNameWhiteSpace, BeforeQuotedStringEndOfLine, MultiLineQuote
        }

        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Start);
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<char> _gotCharTrigger;

        private readonly StringBuilder _buffer = new StringBuilder();
        private int _parens = 0;
        private int _braces = 0;
        private bool _inFunctions = false;

        public List<Line> Lines { get; set; } = new List<Line>();

        public Tokenizer()
        {
            _gotCharTrigger = _machine.SetTriggerParameters<char>(Trigger.GotChar);

            _machine.Configure(State.Start)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.Data)
                .OnEntryFrom(_gotCharTrigger, OnGotCharData)
                .OnEntryFrom(Trigger.Data, OnDataEntry)
                .PermitReentry(Trigger.GotChar)
                .PermitReentry(Trigger.Data)
                .Permit(Trigger.Quote, State.Quote)
                .Permit(Trigger.BeforeCS, State.BeforeCS)
                .Permit(Trigger.OpenTag, State.TagOpen)
                .Permit(Trigger.EndOfLine, State.EndOfLine)
                .Permit(Trigger.EOF, State.EOF);

            _machine.Configure(State.TagOpen)
                .OnEntryFrom(_gotCharTrigger, OnGotCharTagOpen)
                .OnEntryFrom(Trigger.OpenTag, OnTagOpen)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.TagName, State.TagName)
                .Permit(Trigger.SelfClosingStartTag, State.SelfClosingStartTag)
                .Permit(Trigger.EndTagOpen, State.EndTagOpen)
                .PermitReentry(Trigger.OpenTag);

            _machine.Configure(State.TagName)
                 .OnEntryFrom(_gotCharTrigger, OnGotCharTagName)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data)
                .Permit(Trigger.BeforeAttributeName, State.BeforeAttributeName);

            _machine.Configure(State.SelfClosingStartTag)
                .OnEntryFrom(_gotCharTrigger, OnSelfClosingStartTag)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.EndTagOpen)
                .OnEntryFrom(Trigger.EndTagOpen, OnEndTagOpen)
                .OnEntryFrom(_gotCharTrigger, OnGotCharEndTagOpen)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.TagName, State.TagName);

            _machine.Configure(State.BeforeAttributeName)
                .OnEntryFrom(Trigger.BeforeAttributeName, OnBeforeAttributeName)
                .OnEntryFrom(_gotCharTrigger, OnGotCharBeforeAttributeName)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.AttributeName, State.AttributeName)
                .Permit(Trigger.AfterAttributeName, State.AfterAttributeName)
                .Permit(Trigger.EndOfLine, State.BeforeAttributeNameEndOfLine)
                .Permit(Trigger.BeforeAttributeNameWhiteSpace, State.BeforeAttributeNameWhiteSpace);

            _machine.Configure(State.BeforeAttributeNameWhiteSpace)
                .OnEntryFrom(Trigger.BeforeAttributeNameWhiteSpace, OnBeforeAttributeNameWhiteSpace)
                .OnEntryFrom(_gotCharTrigger, OnGotCharBeforeAttributeNameWhiteSpace)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.BeforeAttributeName, State.BeforeAttributeName);

            _machine.Configure(State.BeforeAttributeNameEndOfLine)
                .OnEntryFrom(Trigger.EndOfLine, OnBeforeAttributeNameEndOfLine)
                .Permit(Trigger.BeforeAttributeName, State.BeforeAttributeName);

            _machine.Configure(State.AttributeName)
                .OnEntryFrom(_gotCharTrigger, OnGotCharAttributeName)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.AfterAttributeName, State.AfterAttributeName)
                .Permit(Trigger.BeforeAttributeValue, State.BeforeAttributeValue);

            _machine.Configure(State.AfterAttributeName)
                .OnEntryFrom(_gotCharTrigger, OnGotCharAfterAttributeName)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.SelfClosingStartTag, State.SelfClosingStartTag)
                .Permit(Trigger.Data, State.Data)
                .Permit(Trigger.BeforeAttributeName, State.BeforeAttributeName);

            _machine.Configure(State.BeforeAttributeValue)
                .OnEntryFrom(Trigger.BeforeAttributeValue, OnBeforeAttributeValue)
                .OnEntryFrom(_gotCharTrigger, OnGotCharBeforeAttributeValue)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.AttributeValue, State.AttributeValue);

            _machine.Configure(State.AttributeValue)
                .OnEntryFrom(_gotCharTrigger, OnGotCharAttributeValue)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.AfterAttributeValue, State.AfterAttributeValue)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.AfterAttributeValue)
                .OnEntryFrom(_gotCharTrigger, OnGotCharAfterAttributeValue)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.SelfClosingStartTag, State.SelfClosingStartTag)
                .Permit(Trigger.Data, State.Data)
                .Permit(Trigger.BeforeAttributeName, State.BeforeAttributeName);

            _machine.Configure(State.BeforeCS)
               .OnEntryFrom(_gotCharTrigger, OnGotCharBeforeCS)
               .OnEntryFrom(Trigger.Data, OnDataEntry)
               .PermitReentry(Trigger.GotChar)
               .Permit(Trigger.Data, State.Data)
               .Permit(Trigger.CSLine, State.CSLine)
               .Permit(Trigger.EOF, State.EOF)
               .Permit(Trigger.MultiLineQuote, State.Quote);

            _machine.Configure(State.Quote)
                .OnEntryFrom(_gotCharTrigger, OnGotCharQuote)
                .OnEntryFrom(Trigger.MultiLineQuote, OnEntryMultiLineQuote)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data)
                .Permit(Trigger.EndOfLine, State.BeforeQuotedStringEndOfLine);

            _machine.Configure(State.BeforeQuotedStringEndOfLine)
                .OnEntryFrom(Trigger.EndOfLine, OnBeforeQuotedStringEndOfLine)
                .Permit(Trigger.Quote, State.Quote);

            _machine.Configure(State.CSLine)
              .OnEntryFrom(_gotCharTrigger, OnGotCharCsLine)
              .PermitReentry(Trigger.GotChar)
              .Permit(Trigger.Data, State.Data)
              .Permit(Trigger.EndOfLine, State.EndOfLine)
              .Permit(Trigger.EOF, State.EOF);

            _machine.Configure(State.EndOfLine)
                .OnEntry(OnEndOfLine)
                .Permit(Trigger.Data, State.Data);

        }

        private void OnGotCharBeforeAttributeNameWhiteSpace(char ch)
        {
            if (IsWhiteSpace(ch))
            {
                StartTag tag = ((StartTag)GetCurrentToken());
                tag.Attributes[tag.Attributes.Count - 1].Append(ch);
            }
            else
            {
                _machine.Fire(Trigger.BeforeAttributeName);
                _machine.Fire(_gotCharTrigger, ch);
            }
        }

        private void OnBeforeAttributeNameWhiteSpace()
        {
            ((StartTag)GetCurrentToken()).Attributes.Add(new Text());
        }

        public static List<Line> Parse(string str)
        {
            Tokenizer tokenizer = new Tokenizer();
            return tokenizer.ParseHtml(str);
        }

        protected List<Line> ParseHtml(string str)
        {
            Lines = new List<Line>() { new Line() };
            _machine.Fire(Trigger.Data);
            foreach (char ch in str)
            {
                _machine.Fire(_gotCharTrigger, ch);
            }

            _machine.Fire(Trigger.EOF);
            RemoveAllEmpty();
            FixEmptyAttributes();
            FixSelfClosingTags();
            FixLastLine();
            return Lines;
        }

        private void RemoveAllEmpty()
        {
            foreach (Line line in Lines.Where(token => token.TokenType == TokenType.Line))
            {
                line.Tokens = RemoveEmpty(line.Tokens);
            }
        }

        private void FixLastLine()
        {
            GetCurrentLine().LastLine = true;
        }

        private void OnBeforeAttributeNameEndOfLine()
        {
            StartTag currentStartTag = (StartTag)GetCurrentToken();
            currentStartTag.LineType = currentStartTag.LineType == LineType.SingleLine ? LineType.MultiLineStart : LineType.MultiLine;
            Line newLine = new Line();
            StartTag newStartTag = new StartTag
            {
                LineType = currentStartTag.LineType == LineType.SingleLine ? LineType.MultiLineStart : LineType.MultiLine
            };
            foreach (char ch in currentStartTag.Name)
            {
                newStartTag.Append(ch);
            }
            newLine.Add(newStartTag);
            Lines.Add(newLine);
            _machine.Fire(Trigger.BeforeAttributeName);
        }

        private void OnBeforeQuotedStringEndOfLine()
        {
            QuotedString currentQuotedString = (QuotedString)GetCurrentToken();
            currentQuotedString.LineType = currentQuotedString.LineType == LineType.SingleLine ? LineType.MultiLineStart : LineType.MultiLine;
            Line newLine = new Line();
            QuotedString newQuotedString = new QuotedString
            {
                LineType = currentQuotedString.LineType == LineType.SingleLine ? LineType.MultiLineStart : LineType.MultiLine,
                QuoteMark = currentQuotedString.QuoteMark,
                HasParentheses = currentQuotedString.HasParentheses,
                IsCSStatement = currentQuotedString.IsCSStatement,
                IsMultiLineStatement = currentQuotedString.IsMultiLineStatement
            };
            newLine.Add(newQuotedString);
            Lines.Add(newLine);
            _machine.Fire(Trigger.Quote);
        }

        private void OnEndOfLine()
        {
            Lines.Add(new Line());
            _machine.Fire(Trigger.Data);
        }

        private void FixEmptyAttributes()
        {
            foreach (Line line in Lines.Where(token => token.TokenType == TokenType.Line))
            {
                IEnumerable<IToken> startTokens = line.Tokens.Where(t => t.TokenType == TokenType.StartTag);
                foreach (IToken startToken in startTokens)
                {
                    StartTag startTag = (StartTag)startToken;
                    startTag.Attributes = RemoveEmpty(startTag.Attributes);
                }
            }
        }

        //This is needed because Chrome removes the / from self closing tags
        //so <br /> becomes <br> in the browser DOM 
        private void FixSelfClosingTags()
        {
            List<IToken> tokens = new List<IToken>();
            foreach (Line line in Lines.Where(token => token.TokenType == TokenType.Line))
            {
                foreach (IToken token in line.Tokens)
                {
                    tokens.Add(token);
                }
            }
            List<IToken> tags = tokens.Where(t => t.TokenType == TokenType.StartTag || t.TokenType == TokenType.EndTag).ToList();
            for (int i = 0; i < tags.Count; i++)
            {
                IToken tag = tags[i];
                if (tag.TokenType == TokenType.StartTag &&
                        (
                            ((StartTag)tag).LineType == LineType.SingleLine ||
                            ((StartTag)tag).LineType == LineType.MultiLineEnd
                        )
                    )
                {
                    if (
                        (i == tags.Count - 1 || tags[i + 1].TokenType == TokenType.StartTag) &&
                            !((StartTag)tag).IsGeneric
                        )
                    {
                        ((StartTag)tag).IsSelfClosingTag = true;
                    }
                }
            }
        }

        private List<IToken> RemoveEmpty(List<IToken> tokens)
        {
            return tokens.Where(t => !t.IsEmpty).ToList();
        }

        private void OnDataEntry()
        {
            if (GetCurrentLine().Tokens.Count > 0 && GetCurrentToken().TokenType == TokenType.StartTag)
            {
                StartTag startTag = (StartTag)GetCurrentToken();
                if (startTag.LineType == LineType.MultiLine)
                {
                    startTag.LineType = LineType.MultiLineEnd;
                }
            }
            GetCurrentLine().Add(new Text());
        }

        private void OnGotCharData(char ch)
        {
            if (IsLessThanSign(ch))
            {
                _machine.Fire(Trigger.OpenTag);
                return;
            }
            else if (IsQuotationMark(ch))
            {
                QuotedString quotedString = new QuotedString
                {
                    QuoteMark = QuoteMarkType.DoubleQuote
                };
                GetCurrentLine().Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
            }
            else if (IsApostrophe(ch))
            {
                QuotedString quotedString = new QuotedString
                {
                    QuoteMark = QuoteMarkType.SingleQuote
                };
                GetCurrentLine().Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
            }
            else if (IsAtSign(ch))
            {
                _buffer.Clear();
                _machine.Fire(Trigger.BeforeCS);
                return;
            }
            else if (IsOpenCurlyBraces(ch))
            {
                _braces++;
                GetCurrentToken().Append(ch);
                return;
            }
            else if (IsCloseCurlyBraces(ch))
            {
                if (_braces != 0)
                {
                    _braces--;
                }

                if (_braces == 0)
                {
                    GetCurrentLine().Add(new CSBlockEnd());
                    _inFunctions = false;
                    _machine.Fire(Trigger.Data);
                    return;
                }
                GetCurrentToken().Append(ch);
                return;
            }
            else if (IsN(ch))
            {
                _machine.Fire(Trigger.EndOfLine);
            }
            else if (IsLf(ch) || IsCr(ch))
            {
                //Ignore
            }
            else
            {
                GetCurrentToken().Append(ch);
            }
        }

        private void OnGotCharBeforeCS(char ch)
        {
            _buffer.Append(ch);
            if (IsAtSign(ch))
            {
                if (_buffer.ToString() == "@")
                {
                    _machine.Fire(Trigger.Data);
                    _machine.Fire(_gotCharTrigger, ch);
                }
            }
            if (IsOpenCurlyBraces(ch))
            {
                if (_buffer.ToString().StartsWith("functions") || _buffer.ToString().StartsWith("code"))
                {
                    CSBlockStart token = new CSBlockStart
                    {
                        IsFunctions = true,
                        IsOpenBrace = true,
                        IsCode = _buffer.ToString().StartsWith("code")
                    };
                    GetCurrentLine().Add(token);
                    _braces++;
                    _inFunctions = true;
                    _machine.Fire(Trigger.Data);
                    return;
                }
                else
                {
                    CSBlockStart token = new CSBlockStart
                    {
                        IsFunctions = false,
                        IsOpenBrace = _buffer.ToString().StartsWith("@ {")
                    };
                    GetCurrentLine().Add(token);
                    _machine.Fire(Trigger.Data);
                    foreach (char tempCh in _buffer.ToString())
                    {
                        _machine.Fire(_gotCharTrigger, tempCh);
                    }
                    return;
                }
            }
            if (IsLessThanSign(ch))
            {
                CSBlockStart token = new CSBlockStart
                {
                    IsFunctions = false,
                    IsOpenBrace = false
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.Data);
                foreach (char tempCh in _buffer.ToString())
                {
                    _machine.Fire(_gotCharTrigger, tempCh);
                }
                _machine.Fire(_gotCharTrigger, ch);
                return;
            }
            if (IsQuotationMark(ch))
            {
                if (_buffer.ToString().Trim() == "\"")
                {
                    _machine.Fire(Trigger.MultiLineQuote);
                }
            }
            if (_buffer.ToString().Trim() == "implements")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Implements
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inherits")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Inherit
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inject")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Inject
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "layout")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Layout
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "page")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Page
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "using")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Using
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "addTagHelper")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.AddTagHelper
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "typeparam")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Typeparam
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "attribute")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Attribute
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "namespace")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Namespace
                };
                GetCurrentLine().Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
        }

        private void OnEntryMultiLineQuote()
        {
            QuotedString multiLineQuote = new QuotedString
            {
                LineType = LineType.SingleLine,
                IsMultiLineStatement = true,
                QuoteMark = QuoteMarkType.DoubleQuote
            };
            GetCurrentLine().Tokens.Add(multiLineQuote);
        }

        private void OnGotCharQuote(char ch)
        {
            QuotedString quotedString = (QuotedString)GetCurrentToken();
            if (IsN(ch))
            {
                _machine.Fire(Trigger.EndOfLine);
                return;
            }
            if (IsCr(ch))
            {
                return;
            }
            if (IsQuotationMark(ch) && quotedString.QuoteMark == QuoteMarkType.DoubleQuote)
            {
                if (quotedString.LineType == LineType.MultiLine)
                {
                    quotedString.LineType = LineType.MultiLineEnd;
                }

                _machine.Fire(Trigger.Data);
                return;
            }
            if (IsApostrophe(ch) && quotedString.QuoteMark == QuoteMarkType.SingleQuote)
            {
                if (quotedString.LineType == LineType.MultiLine)
                {
                    quotedString.LineType = LineType.MultiLineEnd;
                }

                _machine.Fire(Trigger.Data);
                return;
            }
            quotedString.Append(ch);
        }

        private void OnGotCharCsLine(char ch)
        {
            if (IsOpenCurlyBraces(ch))
            {
                _machine.Fire(Trigger.CSBlock);
            }
            else if (IsN(ch))
            {
                _machine.Fire(Trigger.EndOfLine);
            }
            else if (IsCr(ch) || IsLf(ch))
            {
                //ignore
            }
            else
            {
                GetCurrentToken().Append(ch);
            }
        }

        private void OnTagOpen()
        {
            GetCurrentLine().Add(new StartTag());
        }

        private void OnGotCharTagOpen(char ch)
        {
            if (IsSolidus(ch))
            {
                _machine.Fire(Trigger.EndTagOpen);
            }
            else
            {
                _machine.Fire(Trigger.TagName);
                _machine.Fire(_gotCharTrigger, ch);
            }
        }

        private void OnGotCharTagName(char ch)
        {
            if (IsGreaterThanSign(ch))
            {
                if (_inFunctions)
                {
                    ((StartTag)GetCurrentToken()).IsGeneric = true;
                }
                _machine.Fire(Trigger.Data);
            }
            else if (IsWhiteSpace(ch))
            {
                _machine.Fire(Trigger.BeforeAttributeName);
            }
            else
            {
                GetCurrentToken().Append(ch);
            }
        }

        private void OnSelfClosingStartTag(char ch)
        {
            if (IsGreaterThanSign(ch))
            {
                StartTag startTag = (StartTag)GetCurrentToken();
                if (startTag.LineType == LineType.MultiLine)
                {
                    startTag.LineType = LineType.MultiLineEnd;
                }

                _machine.Fire(Trigger.Data);
            }
        }

        private void OnEndTagOpen()
        {
            GetCurrentLine().Add(new EndTag());
        }

        private void OnGotCharEndTagOpen(char ch)
        {
            _machine.Fire(Trigger.TagName);
            _machine.Fire(_gotCharTrigger, ch);
        }

        private void OnBeforeAttributeName()
        {
            StartTag startTag = (StartTag)GetCurrentToken();
            AttributeToken atrrib = new AttributeToken();

            if (
                (startTag.LineType == LineType.MultiLine || startTag.LineType == LineType.MultiLineEnd) &&
                startTag.Attributes.Count == 0)
            {
                atrrib.IsFirstInLine = true;
            }

            startTag.Attributes.Add(atrrib);
        }

        private void OnGotCharBeforeAttributeName(char ch)
        {
            if (IsN(ch))
            {
                _machine.Fire(Trigger.EndOfLine);
                return;
            }
            if (IsCr(ch) || IsLf(ch))
            {
                return;
            }
            if (IsWhiteSpace(ch))
            {
                _machine.Fire(Trigger.BeforeAttributeNameWhiteSpace);
                return;
            }
            if (IsSolidus(ch) || IsGreaterThanSign(ch))
            {
                _machine.Fire(Trigger.AfterAttributeName);
                _machine.Fire(_gotCharTrigger, ch);
                return;
            }
            _machine.Fire(Trigger.AttributeName);
            _machine.Fire(_gotCharTrigger, ch);
        }

        private void OnGotCharAttributeName(char ch)
        {
            if (IsWhiteSpace(ch) || IsSolidus(ch) || IsGreaterThanSign(ch))
            {
                _machine.Fire(Trigger.AfterAttributeName);
                _machine.Fire(_gotCharTrigger, ch);
            }
            else if (IsEqualsSign(ch))
            {
                _machine.Fire(Trigger.BeforeAttributeValue);
            }
            else
            {
                StartTag tag = ((StartTag)GetCurrentToken());
                tag.Attributes[tag.Attributes.Count - 1].Append(ch);
            }
        }

        private void OnGotCharAfterAttributeName(char ch)
        {
            if (IsSolidus(ch))
            {
                _machine.Fire(Trigger.SelfClosingStartTag);
            }
            else if (IsEqualsSign(ch))
            {
                _machine.Fire(Trigger.BeforeAttributeValue);
            }
            else if (IsGreaterThanSign(ch))
            {
                _machine.Fire(Trigger.Data);
            }
            else if (IsWhiteSpace(ch))
            {
                return;
            }
            else
            {
                _machine.Fire(Trigger.BeforeAttributeName);
                _machine.Fire(_gotCharTrigger, ch);
            }
        }

        private void OnBeforeAttributeValue()
        {
            StartTag tag = ((StartTag)GetCurrentToken());
            ((AttributeToken)tag.Attributes[tag.Attributes.Count - 1]).NameOnly = false;
        }

        private void OnGotCharBeforeAttributeValue(char ch)
        {
            StartTag tag = ((StartTag)GetCurrentToken());
            AttributeToken attribute = (AttributeToken)tag.Attributes[tag.Attributes.Count - 1];
            if (IsWhiteSpace(ch))
            {
                return;
            }
            else if (IsQuotationMark(ch))
            {
                attribute.Value.QuoteMark = QuoteMarkType.DoubleQuote;
                _machine.Fire(Trigger.AttributeValue);
                return;
            }
            else if (IsApostrophe(ch))
            {
                attribute.Value.QuoteMark = QuoteMarkType.SingleQuote;
                _machine.Fire(Trigger.AttributeValue);
                return;
            }
            attribute.Value.QuoteMark = QuoteMarkType.Unquoted;
            _machine.Fire(Trigger.AttributeValue);
            _machine.Fire(_gotCharTrigger, ch);
        }

        private void OnGotCharAttributeValue(char ch)
        {
            StartTag tag = ((StartTag)GetCurrentToken());
            AttributeToken attribute = (AttributeToken)tag.Attributes[tag.Attributes.Count - 1];
            if (IsQuotationMark(ch) && (attribute.Value.QuoteMark == QuoteMarkType.DoubleQuote))
            {
                _machine.Fire(Trigger.AfterAttributeValue);
                return;
            }
            else if (IsApostrophe(ch) && (attribute.Value.QuoteMark == QuoteMarkType.SingleQuote))
            {
                _machine.Fire(Trigger.AfterAttributeValue);
                return;
            }
            else if (IsGreaterThanSign(ch) && (attribute.Value.QuoteMark == QuoteMarkType.Unquoted))
            {
                _machine.Fire(Trigger.Data);
                return;
            }
            else if (IsAtSign(ch) && attribute.Value.IsEmpty)
            {
                attribute.Value.IsCSStatement = true;
                return;
            }
            else if (IsOpenParenthesis(ch) && attribute.Value.IsEmpty && attribute.Value.IsCSStatement && !attribute.Value.HasParentheses)
            {
                attribute.Value.HasParentheses = true;
                return;
            }
            else if (IsOpenParenthesis(ch))
            {
                _parens++;
                attribute.Value.Append(ch);
                return;
            }
            else if (IsCloseParenthesis(ch) && attribute.Value.HasParentheses && _parens == 0)
            {
                return;
            }
            else if (IsCloseParenthesis(ch) && attribute.Value.HasParentheses && _parens != 0)
            {
                _parens--;
                attribute.Value.Append(ch);
                return;
            }
            attribute.Value.Append(ch);
        }

        private void OnGotCharAfterAttributeValue(char ch)
        {
            if (IsWhiteSpace(ch))
            {
                _machine.Fire(Trigger.BeforeAttributeName);
            }
            else if (IsSolidus(ch))
            {
                _machine.Fire(Trigger.SelfClosingStartTag);
            }
            else if (IsGreaterThanSign(ch))
            {
                _machine.Fire(Trigger.Data);
            }
            else
            {
                _machine.Fire(Trigger.BeforeAttributeName);
                _machine.Fire(_gotCharTrigger, ch);
            }
        }

        private IToken GetCurrentToken()
        {
            Line line = GetCurrentLine();
            return line.Tokens[line.Tokens.Count - 1];
        }

        private Line GetCurrentLine()
        {
            return Lines.Last(token => token.TokenType == TokenType.Line);
        }

        private bool IsLessThanSign(char ch)
        {
            return ch == '<';
        }

        private bool IsGreaterThanSign(char ch)
        {
            return ch == '>';
        }

        private bool IsSolidus(char ch)
        {
            return ch == '/';
        }

        private bool IsWhiteSpace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }

        private bool IsEqualsSign(char ch)
        {
            return ch == '=';
        }

        private bool IsQuotationMark(char ch)
        {
            return ch == '"';
        }

        private bool IsApostrophe(char ch)
        {
            return ch == '\'';
        }

        private bool IsAtSign(char ch)
        {
            return ch == '@';
        }

        private bool IsOpenParenthesis(char ch)
        {
            return ch == '(';
        }

        private bool IsCloseParenthesis(char ch)
        {
            return ch == ')';
        }

        private bool IsOpenCurlyBraces(char ch)
        {
            return ch == '{';
        }

        private bool IsCloseCurlyBraces(char ch)
        {
            return ch == '}';
        }

        private bool IsCr(char ch)
        {
            return ch == '\r';
        }

        private bool IsLf(char ch)
        {
            return ch == '\f';
        }

        private bool IsN(char ch)
        {
            return ch == '\n';
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Line line in Lines)
            {
                sb.Append(line.ToHtml());
            }
            return sb.ToString();
        }
    }
}
