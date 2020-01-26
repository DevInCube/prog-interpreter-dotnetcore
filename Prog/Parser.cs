using System.Collections.Generic;
using System.Linq;
using System;

namespace Prog
{
    public class Variable
    {
        public string Name { get; }
        public ProgValue Value { get; set; }

        public Variable(string name, ProgValue value = null)
        {
            this.Name = name;
            this.Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is Variable variable &&
                   Name == variable.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }

    public class SymbolTable
    {
        private Stack<HashSet<Variable>> scopes = new Stack<HashSet<Variable>>();

        public void EnterScope() => scopes.Push(new HashSet<Variable>());
        public void LeaveScope() => scopes.Pop();
        public Variable FindSymbol(string symbol) => scopes.LastOrDefault(x => x.Any(t => t.Name == symbol))?.FirstOrDefault(t => t.Name == symbol);
        public void AddSymbol(string symbol, ProgValue value) => scopes.Peek().Add(new Variable(symbol, value));
        public bool CheckScope(string symbol) => scopes.Peek().Any(x => x.Name == symbol);
    }

    public static class Parser
    {
        public static ProgramSyntax AnalyzeSyntax(List<Token> tokens)
        {
            int index = 0;
            return Program();

            // @todo split into 2 methods
            Token Accept(string lexeme, TokenType type = TokenType.None)
            {
                Token currToken = null;
                while (index < tokens.Count)
                {
                    currToken = tokens[index];
                    if (currToken.Type != TokenType.Whitespace
                        && currToken.Type != TokenType.Comment)
                        break;
                    index += 1;
                }
                if (index == tokens.Count) return null;
                if (type != TokenType.None)
                {
                    if (currToken.Type != type) return null;
                }
                else
                {
                    if (currToken.Value != lexeme) return null;
                }
                index += 1;
                return currToken;
            }

            // @todo split
            Token Expect(string lexeme, TokenType type = TokenType.None)
            {
                if (Accept(lexeme, type) is var tree && tree != null) return tree;
                if (index == tokens.Count)
                    throw new Exception($"Expected `{lexeme ?? type.ToString()}`, reached end of file");
                var token = tokens[index];
                throw new Exception($"Expected `{lexeme ?? type.ToString()}`, got `{token.Type}:{token.Value}`");
            }

            VariableDeclarationStatementSyntax VarDeclaration()
            {
                if (Accept("let") == null) return null;
                var idToken = Expect(null, TokenType.Identifier);
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
                var parent = new BlockSyntax();
                while (true)
                {
                    if (Statement() is var statement && statement != null)
                        parent.Children.Add(statement);
                    else
                        break;
                }
                Expect("}");
                return parent;
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
                while (true)
                {
                    if (Statement() is var statement && statement != null)
                        program.Children.Add(statement);
                    else
                        break;
                }
                return program;
            }
            ExpressionSyntax Expression() => Assignment();
            ExpressionSyntax Assignment()
                => BinaryOperations(Lang.OperatorsTable.Where(x => x.Arity == OperatorArity.Binary).Reverse().ToArray().GroupBy(x => x.Priority).Select(x => x.ToArray()).ToArray());
            ExpressionSyntax BinaryOperations(OperatorInfo[][] operatorsTable)
            {
                return BinaryRecur();
                //
                ExpressionSyntax BinaryRecur(int i = 0)
                {
                    Func<ExpressionSyntax> nextOperation = () => BinaryRecur(i + 1);
                    if (i == operatorsTable.Length - 1)
                        nextOperation = UnaryOperation;
                    return BinaryOperation(operatorsTable[i], nextOperation);
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
                var operatorToken = Accept(null, TokenType.Operator);
                var primitiveTree = Primitive();
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
            ExpressionSyntax Primitive()
            {
                if (Accept(null, TokenType.Identifier) is var token && token != null)
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
                if ((token = Accept(null, TokenType.Literal)) != null)
                    return new LiteralExpressionSyntax(token);
                return null;  // this was not a primitive
            }
            ArgumentListSyntax ArgumentList()
            {
                if (Accept("(") == null) return null;
                var argsTree = new ArgumentListSyntax();
                bool closed = false;
                while (Expression() is var arg && arg != null)
                {
                    argsTree.Children.Add(arg);
                    if (Accept(")") != null)
                    {
                        closed = true;
                        break;
                    }
                    else
                        Expect(",");
                }
                if (!closed)
                    Expect(")");
                return argsTree;
            }
        }
    }
}