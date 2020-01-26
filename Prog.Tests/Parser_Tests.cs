// using NUnit.Framework;
// using System;
// using System.Linq;

// namespace Prog.Tests
// {
//     [TestFixture]
//     public class ParserTests
//     {
//         [Test]
//         public void Parse_InputIncompleteUnary_Exception()
//         {
//             const string text = "+";
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [Test]
//         public void Parse_InputIncompleteBinary_Exception()
//         {
//             const string text = "2+";
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [Test]
//         public void Parse_InputTwoBinary_Exception()
//         {
//             const string text = "1*/2";
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [TestCase("let")]
//         [TestCase("let 1")]
//         [TestCase("let {")]
//         [TestCase("let (")]
//         [TestCase("let =")]
//         [TestCase("let a = ")]
//         [TestCase("let let")]
//         public void Parse_InputIncompleteVarDecl_Exception(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [TestCase("if")]
//         [TestCase("if (")]
//         [TestCase("if (true")]
//         [TestCase("if ()")]
//         [TestCase("if (true)")]
//         [TestCase("if (true) {} else")]
//         public void Parse_InputIncompleteIf_Exception(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [TestCase("while")]
//         [TestCase("while (")]
//         [TestCase("while (1")]
//         [TestCase("while ()")]
//         [TestCase("while (1)")]
//         public void Parse_InputIncompleteWhile_Exception(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [TestCase("{")]
//         public void Parse_InputIncompleteBlock_Exception(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }

//         [TestCase("2 - 2 - 2")]
//         [TestCase("2 / 2 / 2")]
//         public void Parse_InputSameOperatorsExpression_AssociativityLeftToRight(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             var tree = Parser.AnalyzeSyntax(tokens);
//             Assert.AreEqual(AstNodeType.Program, tree.Value.Type);
//             Assert.AreEqual(1, tree.Children.Count);
//             Assert.AreEqual(AstNodeType.Operation, tree.Children[0].Children[0].Value.Type);
//         }

//         [TestCase("a = b = c")]
//         public void Parse_InputSameOperatorsExpression_AssociativityRightToLeft(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             var tree = Parser.AnalyzeSyntax(tokens);
//             Assert.AreEqual(AstNodeType.Program, tree.Value.Type);
//             Assert.AreEqual(1, tree.Children.Count);
//             Assert.AreEqual(AstNodeType.Operation, tree.Children[0].Children[1].Value.Type);
//         }

//         [TestCase("*3")]
//         [TestCase("/3")]
//         [TestCase("%3")]
//         [TestCase("<3")]
//         [TestCase(">3")]
//         [TestCase("<=3")]
//         [TestCase(">=3")]
//         [TestCase("==3")]
//         [TestCase("!=3")]
//         [TestCase("&&3")]
//         [TestCase("||3")]
//         public void Parse_InputInvalidUnaryOperator_Exception(string text)
//         {
//             var lexer = new Lexer(text);
//             var tokens = lexer.Analyze().ToList();
//             Assert.Catch<Exception>(() => Parser.AnalyzeSyntax(tokens));
//         }
//     }
// }