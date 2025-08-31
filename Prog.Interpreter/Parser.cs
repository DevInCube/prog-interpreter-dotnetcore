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

        private Token? Accept(TokenType type)
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

        private Token? Accept(string lexeme)
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
            if (Accept(lexeme) is Token tree)
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
            if (Accept(type) is Token tree)
            {
                return tree;
            }

            if (!HasNext)
            {
                throw new Exception($"Expected `{type}`, reached end of file");
            }

            throw new Exception($"Expected `{type}`, got `{Current.Type}:{Current.Value}`");
        }

        private VariableDeclarationExpressionSyntax? VarDeclarationExpression()
        {
            if (Accept("let") == null)
            {
                return null;
            }

            var idToken = Expect(TokenType.Identifier);
            var varDecl = new VariableDeclarationExpressionSyntax(new IdentifierNameSyntax(idToken.Value));
            if (VarPrimeDeclaration() is ExpressionSyntax initExpr)
            {
                varDecl.Children.Add(initExpr);
            }

            return varDecl;

            ExpressionSyntax? VarPrimeDeclaration()
            {
                if (Accept("=") == null)
                {
                    return null;
                }

                if (Expression() is not ExpressionSyntax initExpression)
                {
                    throw new Exception("Expected an expression");
                }

                return initExpression;
            }
        }

        private ExpressionSyntax? StatementExpression()
        {
            return VarDeclarationExpression()
                ?? IfExpression()
                ?? WhileExpression()
                ?? (ExpressionSyntax?)BlockExpression();
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

        private BlockExpression? BlockExpression()
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

        private IfExpressionSyntax? IfExpression()
        {
            if (Accept("if") == null)
            {
                return null;
            }

            Expect("(");
            if (Expression() is not ExpressionSyntax testExpression)
            {
                throw new Exception("Expected test expression");
            }

            Expect(")");
            if (Statement() is not Statement thenStatement)
            {
                throw new Exception("Expected then statement after test expression");
            }

            var ifStatement = new IfExpressionSyntax
            {
                Children = { testExpression, thenStatement },
            };
            if (SelectPrimeStatement() is StatementSyntax elseStatement)
            {
                ifStatement.Children.Add(elseStatement);
            }

            return ifStatement;

            StatementSyntax? SelectPrimeStatement()
            {
                if (Accept("else") == null)
                {
                    return null;
                }

                if (Statement() is not Statement elseStatement)
                {
                    throw new Exception("Expected then statement after else keyword");
                }

                return elseStatement;
            }
        }

        private WhileExpressionSyntax? WhileExpression()
        {
            if (Accept("while") == null)
            {
                return null;
            }

            Expect("(");
            if (Expression() is not ExpressionSyntax testExpression)
            {
                throw new Exception("Expected test expression");
            }

            Expect(")");
            if (Statement() is not Statement bodyStatement)
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

        private ExpressionSyntax? Expression() => Assignment();

        private ExpressionSyntax? Assignment() => BinaryOperations();

        private static readonly OperatorInfo[][] s_prioritizedBinaryOperators
            = Lang.OperatorsTable
                .Where(x => x.Arity == OperatorArity.Binary)
                .Reverse()
                .ToArray()
                .GroupBy(x => x.Priority)
                .Select(x => x.ToArray())
                .ToArray();

        private ExpressionSyntax? BinaryOperations()
        {
            return BinaryRecur();

            ExpressionSyntax? BinaryRecur(int i = 0)
            {
                Func<ExpressionSyntax?> nextOperation = () => BinaryRecur(i + 1);
                if (i == s_prioritizedBinaryOperators.Length - 1)
                {
                    nextOperation = UnaryOperation;
                }

                return BinaryOperation(s_prioritizedBinaryOperators[i], nextOperation);
            }
        }

        private ExpressionSyntax? BinaryOperation(OperatorInfo[] operators, Func<ExpressionSyntax?> nextOperation)
        {
            if (nextOperation() is not ExpressionSyntax nextRuleTree)
            {
                return null;
            }

            if (BinaryOperationPrime(operators, nextOperation) is not ExpressionSyntax primeRuleTree)
            {
                return nextRuleTree;
            }

            SyntaxNode leftMostNode = primeRuleTree;
            while (leftMostNode.Children[0] is not EmptyExpressionSyntax)
            {
                leftMostNode = leftMostNode.Children[0];
            }

            leftMostNode.Children[0] = nextRuleTree;  // left sub-tree
            return primeRuleTree;
        }

        private ExpressionSyntax? BinaryOperationPrime(OperatorInfo[] operators, Func<ExpressionSyntax?> nextOperation)
        {
            if (AcceptOperator() is not Token operatorToken)
            {
                return null;
            }

            if (nextOperation() is not ExpressionSyntax nextRuleTree) // left
            {
                throw new Exception("Expected second operand");
            }

            if (BinaryOperationPrime(operators, nextOperation) is ExpressionSyntax thisRuleTree)
            {
                // right to left
                var operatorInfo = operators
                    .Where(x => x.Arity == OperatorArity.Binary)
                    .FirstOrDefault(x => x.Lexeme == operatorToken.Value)
                    ?? throw new Exception($"Binary operator not found: {operatorToken.Value}");
                if (operatorInfo.Associativity == OperatorAssociativity.RightToLeft)
                {
                    thisRuleTree.Children[0] = nextRuleTree;
                    return new BinaryExpressionSyntax(operatorToken, ExpressionSyntax.Empty, thisRuleTree);
                }
                else // left to right
                {
                    thisRuleTree.Children[0] = new BinaryExpressionSyntax(operatorToken, ExpressionSyntax.Empty, nextRuleTree);
                    return thisRuleTree;
                }
            }

            return new BinaryExpressionSyntax(operatorToken, ExpressionSyntax.Empty, nextRuleTree);

            Token? AcceptOperator() => operators.Select(op => Accept(op.Lexeme)).FirstOrDefault(token => token != null);
        }

        private ExpressionSyntax? UnaryOperation()
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

        private ExpressionSyntax? PrimaryOperation()
        {
            if (Accept(TokenType.Identifier) is Token identifierToken)
            {
                var idName = new IdentifierNameSyntax(identifierToken.Value);
                return (ArgumentList() is ArgumentListSyntax argumentList)
                    ? new InvocationExpressionSyntax(idName, argumentList)
                    : idName;
            }

            if (Accept("(") != null)
            {
                if (Expression() is not ExpressionSyntax exprTree)
                {
                    throw new Exception("Expected an expression");
                }

                Expect(")");
                return exprTree;
            }

            if (Accept(TokenType.Literal) is Token literalToken)
            {
                return new LiteralExpressionSyntax(literalToken);
            }

            return StatementExpression();
        }

        private ArgumentListSyntax? ArgumentList()
        {
            if (Accept("(") == null)
            {
                return null;
            }

            var argsTree = new ArgumentListSyntax();
            bool closed = false;
            while (!closed && Expression() is ExpressionSyntax arg)
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