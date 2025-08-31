namespace Prog
{
    public abstract class SyntaxNode
    {
        public List<SyntaxNode> Children { get; } = [];

        public override string ToString() => GetType().ToString();

        public abstract TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
            where TResult : notnull;
    }

    public sealed class ProgramSyntax : SyntaxNode
    {
        public List<Statement> Statements => [.. Children.Cast<Statement>()];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class StatementSyntax : SyntaxNode
    {
    }

    public abstract class ExpressionSyntax : SyntaxNode
    {
        public static ExpressionSyntax Empty { get; } = new EmptyExpressionSyntax();
    }

    public sealed class EmptyExpressionSyntax : ExpressionSyntax
    {
        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            throw new Exception("Can not visit empty expression syntax.");
        }
    }

    public sealed class EmptyExpression : ExpressionSyntax
    {
        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class Statement : StatementSyntax
    {
        public ExpressionSyntax Expression => (ExpressionSyntax)Children.First();

        public Statement(ExpressionSyntax expression)
        {
            Children.Add(expression);
        }

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class VariableDeclarationExpressionSyntax : ExpressionSyntax
    {
        public VariableDeclarationExpressionSyntax(IdentifierNameSyntax declarator)
        {
            Children.Add(declarator);
        }

        public IdentifierNameSyntax Identifier => (IdentifierNameSyntax)Children.First();

        public ExpressionSyntax? Value => Children.Count > 1 ? (ExpressionSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class IfExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Condition => (ExpressionSyntax)Children[0];

        public Statement ThenStatement => (Statement)Children[1];

        public Statement? ElseStatement => Children.Count > 2 ? (Statement)Children[2] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class WhileExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Condition => (ExpressionSyntax)Children[0];

        public Statement Statement => (Statement)Children[1];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class BlockExpression : ExpressionSyntax
    {
        public List<Statement> Statements => [.. Children.Cast<Statement>()];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        public override string ToString() => Token.Value;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(Token token, ExpressionSyntax expression)
        {
            OperatorToken = token;
            Children.Add(expression);
        }

        public Token OperatorToken { get; }

        public ExpressionSyntax Operand => (ExpressionSyntax)Children.First();

        public override string ToString() => OperatorToken.Value;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(
            Token token,
            ExpressionSyntax leftExpression,
            ExpressionSyntax rightExpression)
        {
            OperatorToken = token;
            Children.Add(leftExpression);
            Children.Add(rightExpression);
        }

        public Token OperatorToken { get; }

        public ExpressionSyntax Left => (ExpressionSyntax)Children[0];

        public ExpressionSyntax Right => (ExpressionSyntax)Children[1];

        public override string ToString() => OperatorToken.Value;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class IdentifierNameSyntax : ExpressionSyntax
    {
        public IdentifierNameSyntax(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public override string ToString() => Name;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class InvocationExpressionSyntax : ExpressionSyntax
    {
        public InvocationExpressionSyntax(
            IdentifierNameSyntax identifierName,
            ArgumentListSyntax argumentList)
        {
            Children.Add(identifierName);
            Children.Add(argumentList);
        }

        public IdentifierNameSyntax IdentifierName => (IdentifierNameSyntax)Children[0];

        public ArgumentListSyntax? ArgumentList => Children.Count > 1 ? (ArgumentListSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class ArgumentListSyntax : SyntaxNode
    {
        public List<ExpressionSyntax> Arguments => [.. Children.Cast<ExpressionSyntax>()];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}