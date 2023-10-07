namespace Prog
{
    public enum OperatorArity
    {
        Unary,
        Binary,
    }

    public enum OperatorAssociativity
    {
        LeftToRight,
        RightToLeft,
    }
    public class OperatorInfo
    {
        public string Lexeme { get; }
        public OperatorArity Arity { get; }
        public int Priority { get; }
        public OperatorAssociativity Associativity { get; }

        public OperatorInfo(string lexeme, OperatorArity arity, int priority, OperatorAssociativity associativity = OperatorAssociativity.LeftToRight)
        {
            this.Lexeme = lexeme;
            this.Arity = arity;
            this.Priority = priority;
            this.Associativity = associativity;
        }
    }
}