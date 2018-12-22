using CSHTMLTokenizer.Tokens;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSHTMLTokenizer
{
    internal class CSTokenizer
    {
        private enum State
        {
            Start, Data, Quote, EOF, CSBlock, CSLine, BeforeCS
        }

        private enum Trigger
        {
            GotChar, EOF, Data, Quote, CSBlock, CSLine, BeforeCS
        }

        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Start);
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<char> _gotCharTrigger;
        private List<IToken> Tokens { get; set; } = new List<IToken>();
        private StringBuilder _buffer = new StringBuilder();
        private int _braces = 0;

        public CSTokenizer()
        {
            _gotCharTrigger = _machine.SetTriggerParameters<char>(Trigger.GotChar);

            _machine.Configure(State.Start)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.Data)
                .OnEntryFrom(_gotCharTrigger, OnGotCharData)
                .OnEntryFrom(Trigger.Data, OnDataEntry)
                .PermitReentry(Trigger.Data)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Quote, State.Quote)
                .Permit(Trigger.BeforeCS, State.BeforeCS)
                .Permit(Trigger.EOF, State.EOF);

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

        private void OnGotCharBeforeCS(char ch)
        {
            _buffer.Append(ch);
            if(IsOpenCurlyBraces(ch))
            {
                if(_buffer.ToString().StartsWith("functions"))
                {
                    var token = new CSBlockStart
                    {
                        IsFunctions = true
                    };
                    Tokens.Add(token);
                    _machine.Fire(Trigger.Data);
                    return;
                }
                else
                {
                    var token = new CSBlockStart
                    {
                        IsFunctions = false
                    };
                    Tokens.Add(token);
                    _machine.Fire(Trigger.Data);
                    foreach(var tempCh in _buffer.ToString())
                    {
                        _machine.Fire(_gotCharTrigger, tempCh);
                    }
                    return;
                }
            }
            if (_buffer.ToString().Trim() == "implements" )
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Implements
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inherits")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Inherit
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "inject")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Inject
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "layout")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Layout
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "page")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Page
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "using")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.Using
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
            if (_buffer.ToString().Trim() == "addTagHelper")
            {
                var token = new CSLine
                {
                    LineType = CSLineType.AddTagHelper
                };
                Tokens.Add(token);
                _machine.Fire(Trigger.CSLine);
                return;
            }
        }

        private void OnDataEntry()
        {
            Tokens.Add(new Text());
        }

        private void OnGotCharQuote(char ch)
        {
            var quotedString = (QuotedString)GetCurrentToken();
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
            else quotedString.Append(ch);
        }

        private void OnGotCharCsLine(char ch)
        {
            if (IsOpenCurlyBraces(ch))
            {
                _machine.Fire(Trigger.CSBlock);
            }
            else if(IsCr(ch) || IsLf(ch))
            {
                _machine.Fire(Trigger.Data);
            }
            else
            {
                GetCurrentToken().Append(ch);
            }
        }

        private void OnGotCharData(char ch)
        {
            if (IsQuotationMark(ch))
            {
                var quotedString = new QuotedString
                {
                    QuoteMark = QuoteMarkType.DoubleQuote
                };
                Tokens.Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
            }
            else if (IsApostrophe(ch))
            {
                var quotedString = new QuotedString
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
            }
            else if (IsOpenCurlyBraces(ch))
            {
                _braces++;
                GetCurrentToken().Append(ch);
            }
            else if (IsCloseCurlyBraces(ch))
            {
                if (_braces == 0)
                {
                    Tokens.Add(new CSBlockEnd());
                    _machine.Fire(Trigger.Data);
                    return;
                }
                _braces--;
                GetCurrentToken().Append(ch);
            }
            else GetCurrentToken().Append(ch);
        }

        public static List<IToken> Tokenize(String str)
        {
            var tokenizer = new CSTokenizer();
            return tokenizer.GetTokens(str);
        }

        protected List<IToken> GetTokens(string str)
        {
            _machine.Fire(Trigger.Data);
            foreach (var ch in str) _machine.Fire(_gotCharTrigger, ch);
            _machine.Fire(Trigger.EOF);
            return RemoveEmpty(Tokens);
        }

        private List<IToken> RemoveEmpty(List<IToken> tokens)
        {
            return tokens.Where(t => !t.IsEmpty).ToList();
        }

        private IToken GetCurrentToken() => Tokens[Tokens.Count - 1];
        private bool IsQuotationMark(char ch) => ch == '"';
        private bool IsApostrophe(char ch) => ch == '\'';
        private bool IsAtSign(char ch) => ch == '@';
        private bool IsOpenCurlyBraces(char ch) => ch == '{';
        private bool IsCloseCurlyBraces(char ch) => ch == '}';
        private bool IsCr(char ch) => ch == '\r';
        private bool IsLf(char ch) => ch == '\f';
    }
}
