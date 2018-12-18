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
            Start, Data, Quote, EOF
        }

        private enum Trigger
        {
            GotChar, EOF, Data, Quote
        }

        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Start);
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<char> _gotCharTrigger;
        private List<IToken> Tokens { get; set; } = new List<IToken>();

        public CSTokenizer()
        {
            _gotCharTrigger = _machine.SetTriggerParameters<char>(Trigger.GotChar);

            _machine.Configure(State.Start)
                .Permit(Trigger.Data, State.Data);

            _machine.Configure(State.Data)
                .OnEntryFrom(_gotCharTrigger, OnGotCharData)
                .OnEntryFrom(Trigger.Data, OnDataEntry)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Quote, State.Quote)
                .Permit(Trigger.EOF, State.EOF);

            _machine.Configure(State.Quote)
                .OnEntryFrom(_gotCharTrigger, OnGotCharQuote)
                .PermitReentry(Trigger.GotChar)
                .Permit(Trigger.Data, State.Data);
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
            if (IsQuotationMark(ch))
            {
                var quotedString = new QuotedString();
                quotedString.QuoteMark = QuoteMarkType.DoubleQuote;
                Tokens.Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
            }
            else if (IsApostrophe(ch))
            {
                var quotedString = new QuotedString();
                quotedString.QuoteMark = QuoteMarkType.SingleQuote;
                Tokens.Add(quotedString);
                _machine.Fire(Trigger.Quote);
                return;
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
    }
}
