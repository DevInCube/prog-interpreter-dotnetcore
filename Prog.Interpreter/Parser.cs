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
            _tokens = tokens;
            _index = 0;
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
                {
                    return true;
                }

                Advance();
            }

            return HasNext;
        }

        private Token Accept(TokenType type)
        {
            if (!SkipTokens())
            {
                return null;
            }

            var current = Current;
            if (current.Type != type)
            {
                return null;
            }

            Advance();
            return current;
        }

        private Token Accept(string lexeme)
        {
            if (!SkipTokens())
            {
                return null;
            }

            var current = Current;
            if (current.Value != lexeme)
            {
                return null;
            }

            Advance();
            return current;
        }

        private Token Expect(string lexeme)
        {
            if (Accept(lexeme) is var tree && tree != null)
            {
                return tree;
            }

            if (!HasNext)
            {
                throw new Exception($"Expected `{lexeme}`, reached end of file");
            }

            throw new Exception($"Expected `{lexeme}`, got `{Current.Type}:{Current.Value}`");
        }

        private Token Expect(TokenType type)
        {
            if (Accept(type) is var tree && tree != null)
            {
                return tree;
            }

            if (!HasNext)
            {
                throw new Exception($"Expected `{type}`, reached end of file");
            }

            throw new Exception($"Expected `{type}`, got `{Current.Type}:{Current.Value}`");
        }

        private VariableDeclarationExpressionSyntax VarDeclarationExpression()
        {
            if (Accept("let") == null)
            {
                return null;
            }

            var idToken = Expect(TokenType.Identifier);
            var varDecl = new VariableDeclarationExpressionSyntax(new IdentifierNameSyntax(idToken.Value));
            if (VarPrimeDeclaration() is var initExpr && initExpr != null)
            {
                varDecl.Children.Add(initExpr);
            }

            return varDecl;

            ExpressionSyntax VarPrimeDeclaration()
            {
                if (Accept("=") == null)
                {
                    return null;
                }

                if (Expression() is var initExpression && initExpression == null)
                {
                    throw new Exception("Expected an expression");
                }

                return initExpression;
            }
        }

        private ExpressionSyntax StatementExpression()
        {
            return VarDeclarationExpression()
                ?? IfExpression()
                ?? WhileExpression()
                ?? (ExpressionSyntax)BlockExpression();
        }

        private EmptyExpression? EmptyExpression()
        {
            return Accept(";") != null
                ? new EmptyExpression()
                : null;
        }

        private Statement? Statement()
        {
            if (EmptyExpression() is EmptyExpression emptyExpression)
            {
                return new Statement(emptyExpression);
            }

            var statement = Expression();
            _ = EmptyExpression();
            if (statement == null)
            {
                return null;
            }

            return new Statement(statement);
        }

        private BlockExpression BlockExpression()
        {
            if (Accept("{") == null)
            {
                return null;
            }

            var block = new BlockExpression();
            block.Children.AddRange(Statements());
            Expect("}");
            return block;
        }

        private IfExpressionSyntax IfExpression()
        {
            if (Accept("if") == null)
            {
                return null;
            }

            Expect("(");
            if (Expression() is var testExpression && testExpression == null)
            {
                throw new Exception("Expected test expression");
            }

            Expect(")");
            if (Statement() is var thenStatement && thenStatement == null)
            {
                throw new Exception("Expected then statement after test expression");
            }

            var ifStatement = new IfExpressionSyntax
            {
                Children = { testExpression, thenStatement },
            };
            if (SelectPrimeStatement() is var elseStatement && elseStatement != null)
            {
                ifStatement.Children.Add(elseStatement);
            }

            return ifStatement;

            StatementSyntax SelectPrimeStatement()
            {
                if (Accept("else") == null)
                {
                    return null;
                }

                if (Statement() is var elseStatement && elseStatement == null)
                {
                    throw new Exception("Expected then statement after else keyword");
                }

                return elseStatement;
            }
        }

        private WhileExpressionSyntax WhileExpression()
        {
            if (Accept("while") == null)
            {
                return null;
            }

            Expect("(");
            if (Expression() is var testExpression && testExpression == null)
            {
                throw new Exception("Expected test expression");
            }

            Expect(")");
            if (Statement() is var bodyStatement && bodyStatement == null)
            {
                throw new Exception("Expected body statement after test expression");
            }

            return new WhileExpressionSyntax
            {
                Children = { testExpression, bodyStatement },
            };
        }

        private ProgramSyntax Program()
        {
            var program = new ProgramSyntax();
            program.Children.AddRange(Statements());
            return program;
        }

        private IEnumerable<Statement> Statements()
        {
            while (Statement() is Statement statement)
            {
                yield return statement;
            }
        }

        private ExpressionSyntax Expression() => Assignment();

        private ExpressionSyntax Assignment() => BinaryOperations();

        private static OperatorInfo[][] prioritizedBinaryOperators
            = Lang.OperatorsTable
                .Where(x => x.Arity == OperatorArity.Binary)
                .Reverse()
                .ToArray()
                .GroupBy(x => x.Priority)
                .Select(x => x.ToArray())
                .ToArray();

        private ExpressionSyntax BinaryOperations()
        {
            return BinaryRecur();

            ExpressionSyntax BinaryRecur(int i = 0)
            {
                Func<ExpressionSyntax> nextOperation = () => BinaryRecur(i + 1);
                if (i == prioritizedBinaryOperators.Length - 1)
                {
                    nextOperation = UnaryOperation;
                }

                return BinaryOperation(prioritizedBinaryOperators[i], nextOperation);
            }
        }

        private ExpressionSyntax BinaryOperation(OperatorInfo[] operators, Func<ExpressionSyntax> nextOperation)
        {
            if (nextOperation() is var nextRuleTree && nextRuleTree == null)
            {
                return null;
            }

            if (BinaryOperationPrime(operators, nextOperation) is var primeRuleTree && primeRuleTree != null)
            {
                SyntaxNode leftMostNode = primeRuleTree;
                while (leftMostNode.Children[0] != null)
                {
                    leftMostNode = leftMostNode.Children[0];
                }

                leftMostNode.Children[0] = nextRuleTree;  // left sub-tree
                return primeRuleTree;
            }

            return nextRuleTree;
        }

        private ExpressionSyntax BinaryOperationPrime(OperatorInfo[] operators, Func<ExpressionSyntax> nextOperation)
        {
            if (AcceptOperator() is var operatorToken && operatorToken == null)
            {
                return null;
            }

            if (nextOperation() is var nextRuleTree && nextRuleTree == null) // left
            {
                throw new Exception("Expected second operand");
            }

            if (BinaryOperationPrime(operators, nextOperation) is var thisRuleTree && thisRuleTree != null)
            {
                // right to left
                var operatorInfo = operators.Where(x => x.Arity == OperatorArity.Binary).FirstOrDefault(x => x.Lexeme == operatorToken.Value);
                if (operatorInfo == null)
                {
                    throw new Exception($"Binary operator not found: {operatorToken.Value}");
                }

                if (operatorInfo.Associativity == OperatorAssociativity.RightToLeft)
                {
                    thisRuleTree.Children[0] = nextRuleTree;
                    return new BinaryExpressionSyntax(operatorToken, null, thisRuleTree);
                }
                else // left to right
                {
                    thisRuleTree.Children[0] = new BinaryExpressionSyntax(operatorToken, null, nextRuleTree);
                    return thisRuleTree;
                }
            }

            return new BinaryExpressionSyntax(operatorToken, null, nextRuleTree);

            Token AcceptOperator() => operators.Select(op => Accept(op.Lexeme)).FirstOrDefault(token => token != null);
        }

        private ExpressionSyntax UnaryOperation()
        {
            var operatorToken = Accept(TokenType.Operator);
            var primitiveTree = PrimaryOperation();
            if (operatorToken == null && primitiveTree == null)
            {
                return null;
            }

            if (operatorToken != null)
            {
                if (Lang.OperatorsTable.Where(x => x.Arity == OperatorArity.Unary).FirstOrDefault(x => x.Lexeme == operatorToken.Value) == null)
                {
                    throw new Exception($"Invalid unary operator {operatorToken.Value}");
                }

                if (primitiveTree == null)
                {
                    throw new Exception("Expected an operand");
                }

                return new UnaryExpressionSyntax(operatorToken, primitiveTree);
            }

            return primitiveTree;
        }

        private ExpressionSyntax PrimaryOperation()
        {
            if (Accept(TokenType.Identifier) is var token && token != null)
            {
                var idName = new IdentifierNameSyntax(token.Value);
                return (ArgumentList() is var argumentList && argumentList != null)
                    ? new InvocationExpressionSyntax(idName, argumentList)
                    : idName;
            }

            if (Accept("(") != null)
            {
                if (Expression() is var exprTree && exprTree == null)
                {
                    throw new Exception("Expected an expression");
                }

                Expect(")");
                return exprTree;
            }

            if ((token = Accept(TokenType.Literal)) != null)
            {
                return new LiteralExpressionSyntax(token);
            }

            return StatementExpression();
        }

        private ArgumentListSyntax ArgumentList()
        {
            if (Accept("(") == null)
            {
                return null;
            }

            var argsTree = new ArgumentListSyntax();
            bool closed = false;
            while (!closed && Expression() is var arg && arg != null)
            {
                argsTree.Children.Add(arg);
                if (Accept(")") != null)
                {
                    closed = true;
                }
                else
                {
                    Expect(",");
                }
            }

            if (!closed)
            {
                Expect(")");
            }

            return argsTree;
        }
    }
}