using System;

namespace Prog
{
    internal static class SyntaxTreePrinter
    {
        public static void PrintParseTree(ProgramSyntax syntaxTree)
        {
            if (syntaxTree != null)
                PrintPretty(syntaxTree, "", true, true);
            else
                Console.WriteLine("(empty)");
        }

        // adapted from: https://stackoverflow.com/a/1649223
        private static void PrintPretty(SyntaxNode node, string indent, bool root, bool last)
        {
            Console.Write(indent);
            string newIndent;
            if (last)
            {
                if (!root)
                {
                    Console.Write("└─");
                    newIndent = indent + "◦◦";
                }
                else
                {
                    newIndent = indent + "";
                }
            }
            else
            {
                Console.Write("├─");
                newIndent = indent + "│◦";
            }
            Console.Write($"{ GetValue()}\n");
            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                PrintPretty(child, newIndent, false, i == node.Children.Count - 1);
            }

            string GetValue()
            {
                return node switch
                {
                    var _ when node is IdentifierNameSyntax idName => idName.Name,
                    var _ when node is UnaryExpressionSyntax uexpr => uexpr.OperatorToken.Value,
                    var _ when node is BinaryExpressionSyntax bexpr => bexpr.OperatorToken.Value,
                    var _ when node is LiteralExpressionSyntax literal => literal.Token.Value,
                    _ => node.GetType().ToString(),
                };
            }
        }
    }
}
