using System;
using System.Diagnostics;

namespace Prog
{
    public enum TokenType
    {
        None,  // default
        Keyword,
        Operator,
        Separator,
        Literal,
        Identifier,
        Comment,  // extra
        Whitespace,  // extra
    }

    [DebuggerDisplay("{Type}:`{Value}`")]
    public class Token
    {
        public TokenType Type { get; }
        // public int Line { get; set; }
        // public int Position { get; set; }
        public string Value { get; }  // lexeme

        public Token(TokenType type, string value = null)
        {
            this.Type = type;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"Type={Type}, Value={Value}";
        }

        public override bool Equals(object obj)
        {
            return obj is Token token &&
                   Type == token.Type &&
                   Value == token.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}