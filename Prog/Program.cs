using System;
using System.IO;
using System.Linq;

namespace Prog
{
    internal partial class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Prog: fatal error: No input files");
                Environment.Exit(1);
            }

            var text = File.ReadAllText(args[0]);

            // lexical analysis
            var tokens = Lexer.Analyze(text).ToList();
            var filteredTokens = tokens
                .Where(x => x.Type != TokenType.Comment && x.Type != TokenType.Whitespace)
                .ToList();
            foreach (var token in filteredTokens)
            {
                Console.Write($"({token.Type}:`{token.Value}`)");
            }

            // parse
            var syntaxTree = Parser.Parse(tokens);
            Console.WriteLine();
            SyntaxTreePrinter.PrintParseTree(syntaxTree);

            // execution
            var executionVisitor = new ExecutionVisitor();
            _ = syntaxTree.Accept(executionVisitor);
        }
    }
}