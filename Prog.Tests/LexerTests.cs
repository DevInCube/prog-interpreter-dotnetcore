using NUnit.Framework;
using System;
using System.Linq;

namespace Prog.Tests
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void Analyze_InputEmpty_ReturnEmpty()
        {
            const string text = "";
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(0, tokens.Count);
        }

        [Test]
        public void Analyze_InputInvalidChar_Exception()
        {
            const string text = "♥";
            Assert.Catch<Exception>(() => Lexer.Analyze(text).ToList());
        }

        [TestCase(" ")]
        [TestCase("\n\n")]
        [TestCase("34")]
        [TestCase("3.14")]
        [TestCase("one")]
        [TestCase("{")]
        [TestCase("==")]
        [TestCase("<")]
        [TestCase("<=")]
        [TestCase("&&")]
        [TestCase("!")]
        [TestCase("!=")]
        [TestCase("if")]
        [TestCase("// comment")]
        [TestCase("\"one\"")]
        public void Analyze_InputLexeme_ReturnOne(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(1, tokens.Count);
        }

        [TestCase("&")]
        [TestCase("|")]
        public void Analyze_InputIncompleteOperator_Exception(string text)
        {
            Assert.Catch<Exception>(() => Lexer.Analyze(text).ToList());
        }

        [TestCase("\"")]
        [TestCase("\"no end")]
        public void Analyze_InputUnclosedString_Exception(string text)
        {
            Assert.Catch<Exception>(() => Lexer.Analyze(text).ToList());
        }

        [TestCase("let")]
        [TestCase("if")]
        [TestCase("else")]
        [TestCase("while")]
        public void Analyze_InputKeyword_ReturnKeyword(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(TokenType.Keyword, tokens[0].Type);
        }

        [TestCase("none")]
        [TestCase("true")]
        [TestCase("false")]
        public void Analyze_InputWord_ReturnLiteral(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(TokenType.Literal, tokens[0].Type);
        }

        [TestCase("_")]
        [TestCase("a")]
        [TestCase("_A")]
        [TestCase("a1")]
        [TestCase("_if")]
        public void Analyze_InputWord_ReturnIdentifier(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
        }

        [TestCase("1234")]
        [TestCase("12.34")]
        public void Analyze_InputNumberFormats_ReturnOneNumber(string text)
        {
            var tokens = Lexer.Analyze(text).ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Literal, tokens[0].Type);
        }

        [TestCase("34.")]
        public void Analyze_InputNumberNoFractionalPart_Exception(string text)
        {
            Assert.Catch<Exception>(() => Lexer.Analyze(text).ToList());
        }
    }
}