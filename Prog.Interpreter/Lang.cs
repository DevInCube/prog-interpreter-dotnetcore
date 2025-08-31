namespace Prog
{
    public static class Lang
    {
        public static readonly string[] Keywords =
        {
            "let",
            "if",
            "else",
            "while",
            "none",
            "true",
            "false",
        };

        public static readonly string[] Separators =
        {
            "(",
            ")",
            "{",
            "}",
            ",",
            ";",
        };

        public static readonly OperatorInfo[] OperatorsTable =
        {
            new("+", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new("-", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new("!", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new("*", OperatorArity.Binary, 2),
            new("/", OperatorArity.Binary, 2),
            new("%", OperatorArity.Binary, 2),
            new("+", OperatorArity.Binary, 3),
            new("-", OperatorArity.Binary, 3),
            new("<", OperatorArity.Binary, 5),
            new("<=", OperatorArity.Binary, 5),
            new(">", OperatorArity.Binary, 5),
            new(">=", OperatorArity.Binary, 5),
            new("==", OperatorArity.Binary, 6),
            new("!=", OperatorArity.Binary, 6),
            new("&&", OperatorArity.Binary, 10),
            new("||", OperatorArity.Binary, 11),
            new("=", OperatorArity.Binary, 13, OperatorAssociativity.RightToLeft),
        };

        public static Dictionary<string, FunctionInfo> Functions { get; } = new Dictionary<string, FunctionInfo>
        {
            {
                "sin",
                new FunctionInfo(
                    [typeof(NumberValue)],
                    typeof(NumberValue),
                    (ProgValue[] args) => (NumberValue)Math.Sin(args[0] as NumberValue))
            },
            {
                "floor",
                new FunctionInfo(
                    [typeof(NumberValue)],
                    typeof(NumberValue),
                    (ProgValue[] args) => (NumberValue)Math.Floor(args[0] as NumberValue))
            },
            {
                "print",
                new FunctionInfo(
                    null,
                    typeof(NoneValue),
                    (ProgValue[] args) =>
                    {
                        foreach (var arg in args)
                        {
                            Console.Write($"{arg} ");
                        }

                        Console.WriteLine();
                        return NoneValue.Value;
                    })
            },
        };
    }
}