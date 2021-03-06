using System.Collections.Generic;
using System.Linq;
using System;

namespace Prog
{
    public class Parser
    {
        private readonly IList<Token> _tokens;
        private int _index;

        private bool HasNext => _index != _tokens.Count;
        private Token Current => _tokens[_index];
        private void Advance() => _index += 1;

        private Parser(IList<Token> tokens)
        {
            this._tokens = tokens;
            this._index = 0;
        }

        public static ProgramSyntax Parse(IList<Token> tokens)
        {
            return new Parser(tokens).Program();
        }

        private bool SkipTokens()
        {
            while (HasNext)
            {
                if (Current.Type != TokenType.Whitespace
                    && Current.Type != TokenType.Comment)
                    return true;
                Advance();
            }
            return HasNext;
        }

        private Token Accept(TokenType type)
        {
            if (!SkipTokens()) return null;
            var current = Current;
            if (current.Type != type) return null;
            Advance();
            return current;
        }

        private Token Accept(string lexeme)
        {
            if (!SkipTokens()) return null;
            var current = Current;
            if (current.Value != lexeme) return null;
            Advance();
            return current;
        }

        private Token Expect(string lexeme)
        {
            if (Accept(lexeme) is var tree && tree != null) return tree;
            if (!HasNext)
                throw new Exception($"Expected `{lexeme}`, reached end of file");
            throw new Exception($"Expected `{lexeme}`, got `{Current.Type}:{Current.Value}`");
        }

        private Token Expect(TokenType type)
        {
            if (Accept(type) is var tree && tree != null) return tree;
            if (!HasNext)
                throw new Exception($"Expected `{type}`, reached end of file");
            throw new Exception($"Expected `{type}`, got `{Current.Type}:{Current.Value}`");
        }

        VariableDeclarationStatementSyntax VarDeclaration()
        {
            if (Accept("let") == null) return null;
            var idToken = Expect(TokenType.Identifier);
            var varDecl = new VariableDeclarationStatementSyntax(new IdentifierNameSyntax(idToken.Value));
            if (VarPrimeDeclaration() is var initExpr && initExpr != null)
                varDecl.Children.Add(initExpr);
            return varDecl;

            ExpressionSyntax VarPrimeDeclaration()
            {
                if (Accept("=") == null) return null;
                if (Expression() is var initExpression && initExpression == null)
                    throw new Exception("Expected an expression");
                return initExpression;
            }
        }

        StatementSyntax Statement()
        {
            return VarDeclaration()
                ?? IfStatement()
                ?? WhileStatement()
                ?? (StatementSyntax)BlockStatement()
                ?? ExpressionStatement();
        }
        BlockSyntax BlockStatement()
        {
            if (Accept("{") == null) return null;
            var block = new BlockSyntax();
            while (Statement() is var statement && statement != null)
                block.Children.Add(statement);
            Expect("}");
            return block;
        }

        IfStatementSyntax IfStatement()
        {
            if (Accept("if") == null) return null;
            Expect("(");
            if (Expression() is var testExpression && testExpression == null)
                throw new Exception("Expected test expression");
            Expect(")");
            if (Statement() is var thenStatement && thenStatement == null)
                throw new Exception("Expected then statement after test expression");
            var ifStatement = new IfStatementSyntax
            {
                Children = { testExpression, thenStatement }
            };
            if (SelectPrimeStatement() is var elseStatement && elseStatement != null)
                ifStatement.Children.Add(elseStatement);
            return ifStatement;

            StatementSyntax SelectPrimeStatement()
            {
                if (Accept("else") == null) return null;
                if (Statement() is var elseStatement && elseStatement == null)
                    throw new Exception("Expected then statement after else keyword");
                return elseStatement;
            }
        }

        WhileStatementSyntax WhileStatement()
        {
            if (Accept("while") == null) return null;
            Expect("(");
            if (Expression() is var testExpression && testExpression == null)
                throw new Exception("Expected test expression");
            Expect(")");
            if (Statement() is var bodyStatement && bodyStatement == null)
                throw new Exception("Expected body statement after test expression");
            return new WhileStatementSyntax
            {
                Children = { testExpression, bodyStatement }
            };
        }
        ExpressionStatementSyntax ExpressionStatement()
        {
            return (Expression() is var expr && expr != null)
                ? new ExpressionStatementSyntax(expr)
                : (ExpressionStatementSyntax)null;
        }
        //
        ProgramSyntax Program()
        {
            var program = new ProgramSyntax();
            while (Statement() is var statement && statement != null)
                program.Children.Add(statement);
            return program;
        }
        ExpressionSyntax Expression() => Assignment();
        ExpressionSyntax Assignment() => BinaryOperations();

        private static OperatorInfo[][] PrioritizedBinaryOperators
            = Lang.OperatorsTable
                .Where(x => x.Arity == OperatorArity.Binary)
                .Reverse()
                .ToArray()
                .GroupBy(x => x.Priority)
                .Select(x => x.ToArray())
                .ToArray();
        ExpressionSyntax BinaryOperations()
        {
            return BinaryRecur();
            //
            ExpressionSyntax BinaryRecur(int i = 0)
            {
                Func<ExpressionSyntax> nextOperation = () => BinaryRecur(i + 1);
                if (i == PrioritizedBinaryOperators.Length - 1)
                    nextOperation = UnaryOperation;
                return BinaryOperation(PrioritizedBinaryOperators[i], nextOperation);
            }
        }
        ExpressionSyntax BinaryOperation(OperatorInfo[] operators, Func<ExpressionSyntax> nextOperation)
        {
            if (nextOperation() is var nextRuleTree && nextRuleTree == null) return null;
            if (BinaryOperationPrime(operators, nextOperation) is var primeRuleTree && primeRuleTree != null)
            {
                SyntaxNode leftMostNode = primeRuleTree;
                while (leftMostNode.Children[0] != null)
                    leftMostNode = leftMostNode.Children[0];
                leftMostNode.Children[0] = nextRuleTree;  // left sub-tree
                return primeRuleTree;
            }
            return nextRuleTree;
        }

        ExpressionSyntax BinaryOperationPrime(OperatorInfo[] operators, Func<ExpressionSyntax> nextOperation)
        {
            if (AcceptOperator() is var operatorToken && operatorToken == null) return null;
            if (nextOperation() is var nextRuleTree && nextRuleTree == null) // left
                throw new Exception("Expected second operand");
            if (BinaryOperationPrime(operators, nextOperation) is var thisRuleTree && thisRuleTree != null)
            {
                // right to left
                var operatorInfo = operators.Where(x => x.Arity == OperatorArity.Binary).FirstOrDefault(x => x.Lexeme == operatorToken.Value);
                if (operatorInfo == null)
                    throw new Exception($"Binary operator not found: {operatorToken.Value}");
                if (operatorInfo.Associativity == OperatorAssociativity.RightToLeft)
                {
                    thisRuleTree.Children[0] = nextRuleTree;
                    return new BinaryExpressionSyntax(operatorToken, null, thisRuleTree);
                }
                else  // left to right
                {
                    thisRuleTree.Children[0] = new BinaryExpressionSyntax(operatorToken, null, nextRuleTree);
                    return thisRuleTree;
                }
            }
            return new BinaryExpressionSyntax(operatorToken, null, nextRuleTree);

            Token AcceptOperator() => operators.Select(op => Accept(op.Lexeme)).FirstOrDefault(token => token != null);
        }
        ExpressionSyntax UnaryOperation()
        {
            var operatorToken = Accept(TokenType.Operator);
            var primitiveTree = PrimaryOperation();
            if (operatorToken == null && primitiveTree == null) return null;
            if (operatorToken != null)
            {
                if (Lang.OperatorsTable.Where(x => x.Arity == OperatorArity.Unary).FirstOrDefault(x => x.Lexeme == operatorToken.Value) == null)
                    throw new Exception($"Invalid unary operator {operatorToken.Value}");
                if (primitiveTree == null)
                    throw new Exception("Expected an operand");
                return new UnaryExpressionSyntax(operatorToken, primitiveTree);
            }
            return primitiveTree;
        }
        ExpressionSyntax PrimaryOperation()
        {
            if (Accept(TokenType.Identifier) is var token && token != null)
            {
                var idName = new IdentifierNameSyntax(token.Value);
                return (ArgumentList() is var argumentList && argumentList != null)
                    ? new InvocationExpressionSyntax(idName, argumentList)
                    : (ExpressionSyntax)idName;
            }
            if (Accept("(") != null)
            {
                if (Expression() is var exprTree && exprTree == null)
                    throw new Exception("Expected an expression");
                Expect(")");
                return exprTree;
            }
            if ((token = Accept(TokenType.Literal)) != null)
                return new LiteralExpressionSyntax(token);
            return null;  // this was not a primary operation
        }
        ArgumentListSyntax ArgumentList()
        {
            if (Accept("(") == null) return null;
            var argsTree = new ArgumentListSyntax();
            bool closed = false;
            while (Expression() is var arg && arg != null && !closed)
            {
                argsTree.Children.Add(arg);
                if (Accept(")") != null)
                    closed = true;
                else
                    Expect(",");
            }
            if (!closed)
                Expect(")");
            return argsTree;
        }
    }
}