using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace Prog
{
    public class Lexer
    {
        private readonly string _input;
        private int _index = 0;

        public Lexer(string input)
        {
            this._input = input;
        }

        private bool HasCurrent => _index < _input.Length;
        private char Current => _input[_index];
        private bool HasNext => _index + 1 < _input.Length;
        private char Next => _input[_index + 1];
        private void Advance(int k = 1) => _index += k;

        public IEnumerable<Token> Analyze()
        {
            while (HasCurrent)
                yield return ReadToken();
        }

        private Token ReadToken()
        {
            return Current switch
            {
                var ws when char.IsWhiteSpace(ws) => ReadSpaces(),
                var d when char.IsDigit(d) => ReadNumber(),
                '"' => ReadString(),
                var w when (w == '_' || char.IsLetter(w)) => ReadWord(),
                var sep when (Lang.Separators.Contains(sep.ToString()))
                    => ReadSeparator(),
                var com when (com == '/' && HasNext && Next == '/')
                    => ReadLineComment(),
                _ => ReadOperator() ?? throw new Exception("Lexical error")
            };
        }
        private Token ReadSpaces()
        {
            var lexeme = new StringBuilder();
            while (HasCurrent && char.IsWhiteSpace(Current))
            {
                lexeme.Append(Current);
                Advance();
            }
            return new Token(TokenType.Whitespace, lexeme.ToString());
        }
        private Token ReadWord()
        {
            var lexeme = new StringBuilder();
            while (HasCurrent
                && (Current == '_' || char.IsLetterOrDigit(Current)))
            {
                lexeme.Append(Current);
                Advance();
            }
            var lexemeStr = lexeme.ToString();
            var type = lexemeStr switch
            {
                var x when (x == "true" || x == "false" || x == "none") => TokenType.Literal,
                var x when Lang.Keywords.Contains(x) => TokenType.Keyword,
                _ => TokenType.Identifier
            };
            return new Token(type, lexemeStr);
        }
        private Token ReadNumber()
        {
            var lexeme = new StringBuilder();
            bool hadDecimalPoint = false;
            while (HasCurrent
                && (char.IsDigit(Current) || (Current == '.' && !hadDecimalPoint)))
            {
                lexeme.Append(Current);
                if (Current == '.')
                    hadDecimalPoint = true;
                Advance();
            }
            if (hadDecimalPoint && lexeme[lexeme.Length - 1] == '.')
                throw new Exception("Fractional part expected");
            return new Token(TokenType.Literal, lexeme.ToString());
        }
        private Token ReadString()
        {
            // with no escape-sequences
            var lexeme = new StringBuilder("\"");
            Advance();  // skip start quotes
            while (HasCurrent && Current != '"')
            {
                lexeme.Append(Current);
                Advance();
            }
            if (!HasCurrent)
                throw new Exception("Expected end of string");
            Advance();  // skip end quotes
            lexeme.Append("\"");
            return new Token(TokenType.Literal, lexeme.ToString());
        }
        private Token ReadLineComment()
        {
            var lexeme = new StringBuilder("//");
            // skip "//"
            Advance(2);
            while (HasCurrent && (Current != '\r' && Current != '\n'))
            {
                lexeme.Append(Current);
                Advance();
            }
            return new Token(TokenType.Comment, lexeme.ToString());
        }
        private Token ReadSeparator()
        {
            var lexeme = Current.ToString();
            Advance();
            return new Token(TokenType.Separator, lexeme);
        }
        private Token ReadOperator()
        {
            var candidate = new StringBuilder();
            string lastMatch = null;
            var operators = Lang.OperatorsTable.Select(x => x.Lexeme).Distinct().ToArray();
            while (HasCurrent)
            {
                candidate.Append(Current);
                var candidateStr = candidate.ToString();
                if (!operators.Any(x => x.StartsWith(candidateStr)))
                    break;
                lastMatch = candidateStr;
                Advance();
            }
            if (lastMatch == null) return null;
            if (!operators.Contains(lastMatch))
                throw new Exception($"Unknown operator: {lastMatch}");
            return new Token(TokenType.Operator, lastMatch);
        }
    }
}