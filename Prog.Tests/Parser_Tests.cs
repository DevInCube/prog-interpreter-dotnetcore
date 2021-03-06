using NUnit.Framework;
using System;
using System.Linq;

namespace Prog.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Parse_InputIncompleteUnary_Exception()
        {
            const string text = "+";
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [Test]
        public void Parse_InputIncompleteBinary_Exception()
        {
            const string text = "2+";
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [Test]
        public void Parse_InputTwoBinary_Exception()
        {
            const string text = "1*/2";
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [TestCase("let")]
        [TestCase("let 1")]
        [TestCase("let {")]
        [TestCase("let (")]
        [TestCase("let =")]
        [TestCase("let a = ")]
        [TestCase("let let")]
        public void Parse_InputIncompleteVarDecl_Exception(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [TestCase("if")]
        [TestCase("if (")]
        [TestCase("if (true")]
        [TestCase("if ()")]
        [TestCase("if (true)")]
        [TestCase("if (true) {} else")]
        public void Parse_InputIncompleteIf_Exception(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [TestCase("while")]
        [TestCase("while (")]
        [TestCase("while (1")]
        [TestCase("while ()")]
        [TestCase("while (1)")]
        public void Parse_InputIncompleteWhile_Exception(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [TestCase("{")]
        public void Parse_InputIncompleteBlock_Exception(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }

        [TestCase("2 - 2 - 2")]
        [TestCase("2 / 2 / 2")]
        public void Parse_InputSameOperatorsExpression_AssociativityLeftToRight(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            var tree = Parser.Parse(tokens);
            Assert.IsTrue(tree is ProgramSyntax);
            Assert.AreEqual(1, tree.Children.Count);
            Assert.AreEqual(1, tree.Children[0].Children.Count);
            Assert.IsTrue(tree.Children[0].Children[0].Children[0] is BinaryExpressionSyntax);
        }

        [TestCase("a = b = c")]
        public void Parse_InputSameOperatorsExpression_AssociativityRightToLeft(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            var tree = Parser.Parse(tokens);
            Assert.IsTrue(tree is ProgramSyntax);
            Assert.AreEqual(1, tree.Children.Count);
            Assert.AreEqual(1, tree.Children[0].Children.Count);
            Assert.IsTrue(tree.Children[0].Children[0].Children[1] is BinaryExpressionSyntax);
        }

        [TestCase("*3")]
        [TestCase("/3")]
        [TestCase("%3")]
        [TestCase("<3")]
        [TestCase(">3")]
        [TestCase("<=3")]
        [TestCase(">=3")]
        [TestCase("==3")]
        [TestCase("!=3")]
        [TestCase("&&3")]
        [TestCase("||3")]
        public void Parse_InputInvalidUnaryOperator_Exception(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.Catch<Exception>(() => Parser.Parse(tokens));
        }
    }
}