namespace Prog
{
    public class OperatorInfo
    {
        public string Lexeme { get; }

        public OperatorArity Arity { get; }

        public int Priority { get; }

        public OperatorAssociativity Associativity { get; }

        public OperatorInfo(string lexeme, OperatorArity arity, int priority, OperatorAssociativity associativity = OperatorAssociativity.LeftToRight)
        {
            Lexeme = lexeme;
            Arity = arity;
            Priority = priority;
            Associativity = associativity;
        }
    }
}