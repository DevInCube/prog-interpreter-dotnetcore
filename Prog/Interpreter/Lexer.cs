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
        private readonly StringBuilder _lexeme = new StringBuilder();

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
            this._index = 0;
            while (HasCurrent)
                yield return ReadToken();
        }

        private Token ReadToken()
        {
            _lexeme.Clear();
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
            _lexeme.Clear();
            while (HasCurrent && char.IsWhiteSpace(Current))
            {
                _lexeme.Append(Current);
                Advance();
            }
            return new Token(TokenType.Whitespace, _lexeme.ToString());
        }
        private Token ReadWord()
        {
            _lexeme.Clear();
            while (HasCurrent
                && (Current == '_' || char.IsLetterOrDigit(Current)))
            {
                _lexeme.Append(Current);
                Advance();
            }
            var lexemeStr = _lexeme.ToString();
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
            _lexeme.Clear();
            bool hadDecimalPoint = false;
            while (HasCurrent
                && (char.IsDigit(Current) || (Current == '.' && !hadDecimalPoint)))
            {
                _lexeme.Append(Current);
                if (Current == '.')
                    hadDecimalPoint = true;
                Advance();
            }
            if (hadDecimalPoint && _lexeme[_lexeme.Length - 1] == '.')
                throw new Exception("Fractional part expected");
            return new Token(TokenType.Literal, _lexeme.ToString());
        }
        private Token ReadString()
        {
            // with no escape-sequences
            _lexeme.Clear().Append("\"");
            Advance();  // skip start quotes
            while (HasCurrent && Current != '"')
            {
                _lexeme.Append(Current);
                Advance();
            }
            if (!HasCurrent)
                throw new Exception("Expected end of string");
            Advance();  // skip end quotes
            return new Token(TokenType.Literal, _lexeme.Append("\"").ToString());
        }
        private Token ReadLineComment()
        {
            _lexeme.Clear().Append("//");
            Advance(2);  // skip "//"
            while (HasCurrent && (Current != '\r' && Current != '\n'))
            {
                _lexeme.Append(Current);
                Advance();
            }
            return new Token(TokenType.Comment, _lexeme.ToString());
        }
        private Token ReadSeparator()
        {
            var _lexeme = Current.ToString();
            Advance();
            return new Token(TokenType.Separator, _lexeme);
        }

        private static string[] _operators = Lang.OperatorsTable.Select(x => x.Lexeme).Distinct().ToArray();

        private Token ReadOperator()
        {
            _lexeme.Clear();
            string lastMatch = null;
            while (HasCurrent)
            {
                _lexeme.Append(Current);
                var candidateStr = _lexeme.ToString();
                if (!_operators.Any(x => x.StartsWith(candidateStr)))
                    break;
                lastMatch = candidateStr;
                Advance();
            }
            if (lastMatch == null) return null;
            if (!_operators.Contains(lastMatch))
                throw new Exception($"Unknown operator: {lastMatch}");
            return new Token(TokenType.Operator, lastMatch);
        }
    }
}