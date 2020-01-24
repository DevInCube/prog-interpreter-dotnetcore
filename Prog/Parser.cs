using System.Collections.Generic;
using System.Linq;
using System;

namespace Prog
{
    public class AstNode
    {
        public AstNodeType Type { get; }
        public object Value { get; }

        public AstNode(AstNodeType type, object value = null)
        {
            this.Type = type;
            this.Value = value;
        }
    }

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
        public Variable FindSymbol(string symbol) => scopes.LastOrDefault(x => x.Any(t => t.Name == symbol)).FirstOrDefault(t => t.Name == symbol);
        public void AddSymbol(string symbol) => scopes.Peek().Add(new Variable(symbol, NoneValue.Value));
        public bool CheckScope(string symbol) => scopes.Peek().Any(x => x.Name == symbol);
    }

    public class SemanticAnalyzer
    {
        private Tree<AstNode> _tree;
        private SymbolTable _symbolTable = new SymbolTable();

        public SemanticAnalyzer(Tree<AstNode> tree)
        {
            this._tree = tree ?? throw new ArgumentNullException(nameof(tree));
        }

        public void Analyze()
        {
            if (_tree.Value.Type != AstNodeType.Program)
                throw new Exception("Root should be a program");
            Check(_tree);
        }

        private void Check(Tree<AstNode> tree, Tree<AstNode> parent = null)
        {
            switch (tree.Value.Type)
            {
                case AstNodeType.Operation when (string)tree.Value.Value == "=":
                    CheckAssignment(tree);
                    break;
                case AstNodeType.Identifier when tree.Children.Count == 1:
                    CheckFunctionCall(tree);
                    break;
                case AstNodeType.Identifier when tree.Children.Count == 0:
                    CheckIdentifierUse(tree, parent);
                    break;
                case AstNodeType.IfStatement:
                case AstNodeType.WhileStatement:
                    CheckTestExpression(tree);
                    break;
                case AstNodeType.Program:
                case AstNodeType.Block:
                    _symbolTable.EnterScope();
                    break;
            }
            foreach (var child in tree.Children)
                Check(child, tree);
            switch (tree.Value.Type)
            {
                case AstNodeType.Program:
                case AstNodeType.Block:
                    _symbolTable.LeaveScope();
                    break;
            }
        }

        private void CheckAssignment(Tree<AstNode> tree)
        {
            if (tree.Children[0].Value.Type != AstNodeType.Identifier)
                throw new Exception("Expected a variable name to the left of an assignment");
        }

        private void CheckFunctionCall(Tree<AstNode> tree)
        {
            var funcName = (string)tree.Value.Value;
            if (!Prog.Lang.Functions.ContainsKey(funcName))
                throw new Exception($"Undefined reference to function {funcName}");
        }

        private void CheckIdentifierUse(Tree<AstNode> tree, Tree<AstNode> parent)
        {
            var varName = (string)tree.Value.Value;
            if (parent.Value.Type == AstNodeType.VarDeclaration)
                if (_symbolTable.CheckScope(varName))
                    throw new Exception($"Variable {varName} has already been declared");
                else
                    _symbolTable.AddSymbol(varName);
            else
                if (_symbolTable.FindSymbol(varName) == null)
                throw new Exception($"Undefined variable {varName}");
        }

        private void CheckTestExpression(Tree<AstNode> tree)
        {
            bool isNotBooleanLiteral =
                tree.Children[0].Value.Type == AstNodeType.Number
                || tree.Children[0].Value.Type == AstNodeType.String
                || tree.Children[0].Value.Type == AstNodeType.None;
            var boolResultOperators = new[] { "<", "<=", ">", ">=", "==", "!=", "!", "&&", "||" };
            bool isNotBooleanOperator =
                tree.Children[0].Value.Type == AstNodeType.Operation
                && !boolResultOperators.Contains(tree.Children[0].Value.Value);
            if (isNotBooleanLiteral || isNotBooleanOperator)
                throw new Exception("Expected boolean in test expression result");
        }
    }

    public static class Parser
    {
        public static Tree<AstNode> AnalyzeSyntax(List<Token> tokens)
        {
            int index = 0;
            return Program();

            AstNode ToAstNode(Token token)
            {
                return token.Type switch
                {
                    TokenType.Identifier => new AstNode(AstNodeType.Identifier, token.Value),
                    TokenType.Literal => token.Value switch
                    {
                        "none" => new AstNode(AstNodeType.None, null),
                        var b when (b == "true" || b == "false") => new AstNode(AstNodeType.Boolean, bool.Parse(b)),
                        var x when x.StartsWith('\"') => new AstNode(AstNodeType.String, token.Value.Substring(1, token.Value.Length - 2)),
                        _ => new AstNode(AstNodeType.Number, Double.Parse(token.Value))
                    },
                    TokenType.Operator => new AstNode(AstNodeType.Operation, token.Value),
                    _ => throw new ArgumentException("Invalid token to AstNode")
                };
            }

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

            Tree<AstNode> VarDeclaration()
            {
                if (Accept("let") == null) return null;
                var idToken = Expect(null, TokenType.Identifier);
                var tree = new Tree<AstNode>(new AstNode(AstNodeType.VarDeclaration),
                    new Tree<AstNode>(ToAstNode(idToken)));
                if (VarPrimeDeclaration() is var initExpr && initExpr != null)
                    tree.Children.Add(initExpr);
                return tree;

                Tree<AstNode> VarPrimeDeclaration()
                {
                    if (Accept("=") == null) return null;
                    if (Expression() is var initExpression && initExpression == null)
                        throw new Exception("Expected an expression");
                    return initExpression;
                }
            }

            Tree<AstNode> Statement()
            {
                Tree<AstNode> tree;
                if ((tree = SelectStatement()) != null) return tree;
                if ((tree = LoopStatement()) != null) return tree;
                if ((tree = CompoundStatement()) != null) return tree;
                return ExpressionStatement();
            }
            Tree<AstNode> CompoundStatement()
            {
                if (Accept("{") == null) return null;
                var parent = new Tree<AstNode>(new AstNode(AstNodeType.Block));
                Tree<AstNode> tree;
                do
                {
                    tree = VarDeclaration();
                    if (tree == null)
                        tree = Statement();
                    if (tree != null)
                        parent.Children.Add(tree);
                } while (tree != null);
                Expect("}");
                return parent;
            }
            Tree<AstNode> SelectStatement()
            {
                if (Accept("if") == null) return null;
                var selectTree = new Tree<AstNode>(new AstNode(AstNodeType.IfStatement));
                Expect("(");
                if (Expression() is var testExpression && testExpression == null)
                    throw new Exception("Expected test expression");
                selectTree.Children.Add(testExpression);
                Expect(")");
                if (Statement() is var thenStatement && thenStatement == null)
                    throw new Exception("Expected then statement after test expression");
                selectTree.Children.Add(thenStatement);
                if (SelectPrimeStatement() is var elseStatement && elseStatement != null)
                    selectTree.Children.Add(elseStatement);
                return selectTree;

                Tree<AstNode> SelectPrimeStatement()
                {
                    if (Accept("else") == null) return null;
                    if (Statement() is var elseStatement && elseStatement == null)
                        throw new Exception("Expected then statement after else keyword");
                    return elseStatement;
                }
            }

            Tree<AstNode> LoopStatement()
            {
                if (Accept("while") == null) return null;
                var loopTree = new Tree<AstNode>(new AstNode(AstNodeType.WhileStatement));
                Expect("(");
                if (Expression() is var testExpression && testExpression == null)
                    throw new Exception("Expected test expression");
                loopTree.Children.Add(testExpression);
                Expect(")");
                if (Statement() is var bodyStatement && bodyStatement == null)
                    throw new Exception("Expected body statement after test expression");
                loopTree.Children.Add(bodyStatement);
                return loopTree;
            }
            Tree<AstNode> ExpressionStatement() { return Expression(); }
            //
            Tree<AstNode> Program()
            {
                var program = new Tree<AstNode>(new AstNode(AstNodeType.Program));
                Tree<AstNode> tree;
                do
                {
                    tree = VarDeclaration();
                    if (tree == null)
                        tree = Statement();
                    if (tree != null)
                        program.Children.Add(tree);
                } while (tree != null);
                return program;
            }
            Tree<AstNode> Expression() => Assignment();
            Tree<AstNode> Assignment() => BinaryOperations(Lang.OperatorsTable.Where(x => x.Arity == OperatorArity.Binary).Reverse().ToArray().GroupBy(x => x.Priority).Select(x => x.ToArray()).ToArray());
            Tree<AstNode> BinaryOperations(OperatorInfo[][] operatorsTable)
            {
                return BinaryRecur();
                //
                Tree<AstNode> BinaryRecur(int i = 0)
                {
                    Func<Tree<AstNode>> nextOperation = () => BinaryRecur(i + 1);
                    if (i == operatorsTable.Length - 1)
                        nextOperation = UnaryOperation;
                    return BinaryOperation(operatorsTable[i], nextOperation);
                }
            }
            Tree<AstNode> BinaryOperation(OperatorInfo[] operators, Func<Tree<AstNode>> nextOperation)
            {
                if (nextOperation() is var nextRuleTree && nextRuleTree == null) return null;
                if (BinaryOperationPrime(operators, nextOperation) is var primeRuleTree && primeRuleTree != null)
                {
                    var leftMostNode = primeRuleTree;
                    while (leftMostNode.Children[0] != null)
                        leftMostNode = leftMostNode.Children[0];
                    leftMostNode.Children[0] = nextRuleTree;  // left sub-tree
                    return primeRuleTree;
                }
                return nextRuleTree;
            }

            Tree<AstNode> BinaryOperationPrime(OperatorInfo[] operators, Func<Tree<AstNode>> nextOperation)
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
                        return Tree.Create(ToAstNode(operatorToken), null, thisRuleTree);
                    }
                    else  // left to right
                    {
                        thisRuleTree.Children[0] = Tree.Create(ToAstNode(operatorToken), null, nextRuleTree);
                        return thisRuleTree;
                    }
                }
                return Tree.Create(ToAstNode(operatorToken), null, nextRuleTree);

                Token AcceptOperator() => operators.Select(op => Accept(op.Lexeme)).FirstOrDefault(token => token != null);
            }
            Tree<AstNode> UnaryOperation()
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
                    return new Tree<AstNode>(ToAstNode(operatorToken), primitiveTree);
                }
                return primitiveTree;
            }
            Tree<AstNode> Primitive()
            {
                if (Accept(null, TokenType.Identifier) is var token && token != null)
                {
                    var tree = new Tree<AstNode>(ToAstNode(token));
                    if (FuncCall() is var funcCall && funcCall != null)
                        tree.Children.Add(funcCall);
                    return tree;
                }
                if (Accept("(") != null)
                {
                    if (Expression() is var exprTree && exprTree == null)
                        throw new Exception("Expected an expression");
                    Expect(")");
                    return exprTree;
                }
                if ((token = Accept(null, TokenType.Literal)) != null)
                    return new Tree<AstNode>(ToAstNode(token));
                return null;  // this was not a primitive
            }
            Tree<AstNode> FuncCall()
            {
                if (Accept("(") == null) return null;
                var argsTree = new Tree<AstNode>(new AstNode(AstNodeType.ArgsList));
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