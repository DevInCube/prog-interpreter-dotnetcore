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
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze();
            var filteredTokens = tokens
                .Where(x => x.Type != TokenType.Comment && x.Type != TokenType.Whitespace)
                .ToList();
            foreach (var token in filteredTokens)
                Console.Write($"({token.Type}:`{token.Value}`)");
            // parse
            var parser = new Parser(lexer);
            var syntaxTree = parser.Parse();
            Console.WriteLine();
            SyntaxTreePrinter.PrintParseTree(syntaxTree);
            // execution
            var executionVisitor = new ExecutionVisitor();
            var _ = syntaxTree.Accept(executionVisitor);
        }
    }
}
