using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prog
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Prog: fatal error: No input files");
                Environment.Exit(1);
            }
            var text = File.ReadAllText(args[0]);
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze();
            var filteredTokens = tokens
                .Where(x => x.Type != TokenType.Comment && x.Type != TokenType.Whitespace)
                .ToList();
            foreach (var token in filteredTokens)
                Console.Write($"({token.Type}:`{token.Value}`)");
            var tree = Parser.AnalyzeSyntax(filteredTokens);
            Console.WriteLine();
            PrintParseTree(tree);
            // //
            // var walker = new TestWalker();
            // tree.Accept(walker);
            // foreach (var item in walker.Items) Console.WriteLine($"var: {((IdentifierNameSyntax)item).Name}");

            var executionVisitor = new ExecutionVisitor();
            tree.Accept(executionVisitor);
            // var tree = Parser.AnalyzeSyntax(tokens);
            // var json = JsonSerializer.Serialize(tree, new JsonSerializerOptions
            // {
            //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //     WriteIndented = true
            // });
            // File.WriteAllText(Path.Join(Path.GetDirectoryName(args[0]), "ast.json"), json);
            // var semAnalyzer = new SemanticAnalyzer(tree);
            // semAnalyzer.Analyze();
            // //Console.WriteLine();
            // //PrintParseTree(tree);
            // Runtime.Execute(tree);
        }

        static void PrintParseTree(ProgramSyntax syntaxTree)
        {
            if (syntaxTree != null)
                PrintPretty(syntaxTree, "", true, true);
            else
                Console.WriteLine("(empty)");
        }

        // adapted from: https://stackoverflow.com/a/1649223
        static void PrintPretty(SyntaxNode node, string indent, bool root, bool last)
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
