using System;
using System.Collections.Generic;
using System.IO;

namespace Prog
{
class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Prog: fatal error: No input files");
            Environment.Exit(1);
        }
        var text = File.ReadAllText(args[0]);
        var lexer = new Lexer(text);
        var tokens = lexer.Analyze();
        foreach (var token in tokens)
            Console.Write($"({token.Type}:{token.Value})");
        
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

        static void PrintParseTree(Tree<AstNode> root)
        {
            if (root != null)
                PrintPretty(root, "", true, true);
            else
                Console.WriteLine("(empty)");
        }

        // adapted from: https://stackoverflow.com/a/1649223
        static void PrintPretty(Tree<AstNode> node, string indent, bool root, bool last)
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
                return node.Value.Type switch
                {
                    AstNodeType.Identifier => node.Value.Value as string,
                    AstNodeType.String => $"\"{node.Value.Value}\"",
                    AstNodeType.Number => $"{(double)node.Value.Value}",
                    AstNodeType.Operation => node.Value.Value as string,
                    _ => node.Value.Type.ToString(),
                };
            }
        }
    }
}
