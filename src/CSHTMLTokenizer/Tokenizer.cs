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
            AfterAttributeName, BeforeAttributeValue, AttributeValue, AfterAttributeValue, Quote, CSBlock, CSLine, BeforeCS
        }

        private enum Trigger
        {
            GotChar, EOF, OpenTag, TagName, Data, SelfClosingStartTag, EndTagOpen, BeforeAttributeName, AttributeName,
            AfterAttributeName, BeforeAttributeValue, AttributeValue, AfterAttributeValue, Quote, CSBlock, CSLine, BeforeCS
        }

        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Start);
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<char> _gotCharTrigger;

        private readonly StringBuilder _buffer = new StringBuilder();
        private int _parens = 0;
        private int _braces = 0;
        private bool _inFunctions = false;

        public List<IToken> Tokens { get; set; } = new List<IToken>();

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
                .Permit(Trigger.AfterAttributeName, State.AfterAttributeName);

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
               .Permit(Trigger.EOF, State.EOF);

            _machine.Configure(State.Quote)
                .OnEntryFrom(_gotCharTrigger, OnGotCharQuote)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.CSLine)
              .OnEntryFrom(_gotCharTrigger, OnGotCharCsLine)
              .PermitReentry(Trigger.GotChar)
              .Permit(Trigger.Data, State.Data)
              .Permit(Trigger.EOF, State.EOF);

        }

        public static List<IToken> Parse(string str)
        {
            Tokenizer tokenizer = new Tokenizer();
            return tokenizer.ParseHtml(str);
        }

        protected List<IToken> ParseHtml(string str)
        {
            Tokens = new List<IToken>();
            _machine.Fire(Trigger.Data);
            foreach (char ch in str)
            {
                _machine.Fire(_gotCharTrigger, ch);
            }

            _machine.Fire(Trigger.EOF);
            Tokens = RemoveEmpty(Tokens);
            FixEmptyAttributes(Tokens);
            FixSelfClosingTags(Tokens);
            return Tokens;
        }

        private void FixEmptyAttributes(List<IToken> tokens)
        {
            IEnumerable<IToken> startTokens = Tokens.Where(t => t.TokenType == TokenType.StartTag);
            foreach (IToken startToken in startTokens)
            {
                StartTag startTag = (StartTag)startToken;
                startTag.Attributes = RemoveEmpty(startTag.Attributes);
            }
        }

        private List<IToken> ReplaceTokenWithListOfTokens(List<IToken> tokens, IToken key, List<IToken> value)
        {
            List<IToken> ret = new List<IToken>();
            foreach (IToken token in Tokens)
            {
                if (token.Id != key.Id)
                {
                    ret.Add(token);
                }
                else
                {
                    ret.AddRange(value);
                }

            }
            return ret;
        }

        //This is needed because Chrome removes the / from self closing tags
        //so <br /> becomes <br> in the browser DOM 
        private void FixSelfClosingTags(List<IToken> tokens)
        {
            List<IToken> tags = tokens.Where(t => t.TokenType == TokenType.StartTag || t.TokenType == TokenType.EndTag).ToList();
            for (int i = 0; i < tags.Count; i++)
            {
                IToken tag = tags[i];
                if (tag.TokenType == TokenType.StartTag)
                {
                    if ((i == tags.Count - 1 || tags[i + 1].TokenType == TokenType.StartTag) && !((StartTag)tag).IsGeneric)
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
            Tokens.Add(new Text());
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
                Tokens.Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
            }
            else if (IsApostrophe(ch))
            {
                QuotedString quotedString = new QuotedString
                {
                    QuoteMark = QuoteMarkType.SingleQuote
                };
                Tokens.Add(quotedString);
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
                    Tokens.Add(new CSBlockEnd());
                    _inFunctions = false;
                    _machine.Fire(Trigger.Data);
                    return;
                }
                GetCurrentToken().Append(ch);
                return;
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
                if (_buffer.ToString().StartsWith("functions"))
                {
                    CSBlockStart token = new CSBlockStart
                    {
                        IsFunctions = true,
                        IsOpenBrace = true
                    };
                    Tokens.Add(token);
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
                    Tokens.Add(token);
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
                Tokens.Add(token);
                _machine.Fire(Trigger.Data);
                foreach (char tempCh in _buffer.ToString())
                {
                    _machine.Fire(_gotCharTrigger, tempCh);
                }
                _machine.Fire(_gotCharTrigger, ch);
                return;
            }
            if (_buffer.ToString().Trim() == "implements")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Implements
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inherits")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Inherit
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inject")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Inject
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "layout")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Layout
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "page")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Page
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "using")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Using
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "addTagHelper")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.AddTagHelper
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "typeparam")
            {
                CSLine token = new CSLine
                {
                    LineType = CSLineType.Typeparam
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
        }

        private void OnGotCharQuote(char ch)
        {
            QuotedString quotedString = (QuotedString)GetCurrentToken();
            if (IsQuotationMark(ch) && quotedString.QuoteMark == QuoteMarkType.DoubleQuote)
            {
                _machine.Fire(Trigger.Data);
                return;
            }
            else if (IsApostrophe(ch) && quotedString.QuoteMark == QuoteMarkType.SingleQuote)
            {
                _machine.Fire(Trigger.Data);
                return;
            }
            else
            {
                quotedString.Append(ch);
            }
        }

        private void OnGotCharCsLine(char ch)
        {
            if (IsOpenCurlyBraces(ch))
            {
                _machine.Fire(Trigger.CSBlock);
            }
            else if (IsCr(ch) || IsLf(ch))
            {
                _machine.Fire(Trigger.Data);
            }
            else
            {
                GetCurrentToken().Append(ch);
            }
        }

        private void OnTagOpen()
        {
            Tokens.Add(new StartTag());
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
                _machine.Fire(Trigger.Data);
            }
        }

        private void OnEndTagOpen()
        {
            Tokens.Add(new EndTag());
        }

        private void OnGotCharEndTagOpen(char ch)
        {
            _machine.Fire(Trigger.TagName);
            _machine.Fire(_gotCharTrigger, ch);
        }

        private void OnBeforeAttributeName()
        {
            ((StartTag)GetCurrentToken()).Attributes.Add(new AttributeToken());
        }

        private void OnGotCharBeforeAttributeName(char ch)
        {
            if (IsWhiteSpace(ch))
            {
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
            return Tokens[Tokens.Count - 1];
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IToken token in Tokens)
            {
                sb.Append(token.ToHtml());
            }
            return sb.ToString();
        }
    }
}
