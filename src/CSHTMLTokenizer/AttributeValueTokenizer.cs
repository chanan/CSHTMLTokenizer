using CSHTMLTokenizer.Tokens;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSHTMLTokenizer
{
    internal class AttributeValueTokenizer
    {
        private enum State
        {
            Start, Data, CS, EOF, BeforeCS
        }

        private enum Trigger
        {
            GotChar, EOF, Data, CS, BeforeCS
        }

        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Start);
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<char> _gotCharTrigger;
        private List<IToken> Tokens { get; set; } = new List<IToken>();
        private int parens = 0;

        public AttributeValueTokenizer()
        {
            _gotCharTrigger = _machine.SetTriggerParameters<char>(Trigger.GotChar);

            _machine.Configure(State.Start)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.Data)
                .OnEntryFrom(_gotCharTrigger, OnGotCharData)
                .OnEntryFrom(Trigger.Data, OnDataEntry)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.BeforeCS, State.BeforeCS)
                .Permit(Trigger.EOF, State.EOF);

            _machine.Configure(State.BeforeCS)
                .OnEntryFrom(_gotCharTrigger, OnGotCharBeforeCS)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.CS, State.CS);

            _machine.Configure(State.CS)
                .OnEntryFrom(_gotCharTrigger, OnGotCharCS)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data)
                .Permit(Trigger.EOF, State.EOF);
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

        private void OnGotCharData(char ch)
        {
            if (IsAtSign(ch))
            {
                parens = 0;
                var attributeValueStatement = new AttributeValueStatement();
                Tokens.Add(attributeValueStatement);
                _machine.Fire(Trigger.BeforeCS);
                return;
            }
            else GetCurrentToken().Append(ch);
        }

        private void OnGotCharBeforeCS(char ch)
        {
            if (IsOpenParenthesis(ch))
            {
                var attibuteValueStatement = (AttributeValueStatement)GetCurrentToken();
                attibuteValueStatement.HasParentheses = true;
                _machine.Fire(Trigger.CS);
                return;
            }
            else
            {
                _machine.Fire(Trigger.CS);
                _machine.Fire(_gotCharTrigger, ch);
            }
        }

        private void OnGotCharCS(char ch)
        {
            var attibuteValueStatement = (AttributeValueStatement)GetCurrentToken();
            if(IsOpenParenthesis(ch) && attibuteValueStatement.HasParentheses)
            {
                parens++;
                GetCurrentToken().Append(ch);
            }
            else if (IsCloseParenthesis(ch) && attibuteValueStatement.HasParentheses)
            {
                if(parens == 0)
                {
                    _machine.Fire(Trigger.Data);
                    return;
                }
                else
                {
                    parens--;
                    GetCurrentToken().Append(ch);
                }
            }
            else GetCurrentToken().Append(ch);
        }

        public static List<IToken> Tokenize(String str)
        {
            var tokenizer = new AttributeValueTokenizer();
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
        private bool IsOpenParenthesis(char ch) => ch == '(';
        private bool IsCloseParenthesis(char ch) => ch == ')';
    }
}
