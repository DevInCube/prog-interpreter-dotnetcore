using NUnit.Framework;
using System;
using System.Linq;

namespace Prog.Tests
{
    // https://help.infragistics.com/help/doc/wpf/2015.1/clr4.0/html/IG_SPE_Semantic_Errors.html#_Ref349656373
    [TestFixture]
    public class SemanticTests
    {
        // semantic tests
        [TestCase("5 = 3")]
        public void Parse_InputAssignLeftIsNotId_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }

        [TestCase("a = 5")]
        [TestCase("a")]
        public void Parse_InputAssignUndeclaredVar_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }
        [TestCase("und_(1)")]
        public void Parse_InputCallUndefinedFunc_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }
        [TestCase("let a\nlet a")]
        public void Parse_InputDeclareExistingVar_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }

        // type checking
        [TestCase("while (5) {}")]
        [TestCase("if (5) {}")]
        public void Parse_InputNumberInTestExpr_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }

        [TestCase("while (2 + 5) {}")]
        [TestCase("if (2 + 5) {}")]
        public void Parse_InputNumberExprInTestExpr_Exception(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Analyze().ToList();
            var tree = Parser.AnalyzeSyntax(tokens);
            var semAnalyzer = new SemanticAnalyzer(tree);
            Assert.Catch<Exception>(() => semAnalyzer.Analyze());
        }
    }
}