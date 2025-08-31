using System;
using System.Diagnostics;

namespace Prog
{
    [DebuggerDisplay("{Type}:`{Value}`")]
    public class Token
    {
        public TokenType Type { get; }

        // public int Line { get; set; }
        // public int Position { get; set; }
        public string Value { get; } // lexeme

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"Type={Type}, Value={Value}";
        }

        public override bool Equals(object? obj)
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