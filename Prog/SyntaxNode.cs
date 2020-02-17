using System;
using System.Collections.Generic;
using System.Linq;

namespace Prog
{
    public abstract class SyntaxNode
    {
        // public SyntaxNode Parent { get; }
        public List<SyntaxNode> Children { get; } = new List<SyntaxNode>();

        public override string ToString() => this.GetType().ToString();

        public abstract TResult Accept<TResult>(SyntaxVisitor<TResult> visitor);
    }

    public sealed class ProgramSyntax : SyntaxNode
    {
        public List<StatementSyntax> Statements => Children.Cast<StatementSyntax>().ToList();

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class StatementSyntax : SyntaxNode
    {
    }

    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(IdentifierNameSyntax declarator)
        {
            Children.Add(declarator);
        }

        public IdentifierNameSyntax Identifier => (IdentifierNameSyntax)Children.First();

        public ExpressionSyntax Value => Children.Count > 1 ? (ExpressionSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class IfStatementSyntax : StatementSyntax
    {
        public ExpressionSyntax Condition => (ExpressionSyntax)Children[0];
        public StatementSyntax ThenStatement => (StatementSyntax)Children[1];
        public StatementSyntax ElseStatement => Children.Count > 2 ? (StatementSyntax)Children[2] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public ExpressionSyntax Condition => (ExpressionSyntax)Children[0];
        public StatementSyntax Statement => (StatementSyntax)Children[1];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class BlockSyntax : StatementSyntax
    {
        public List<StatementSyntax> Statements => Children.Cast<StatementSyntax>().ToList();

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(ExpressionSyntax expression)
        {
            Children.Add(expression);
        }

        public ExpressionSyntax Expression => (ExpressionSyntax)Children[0];

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

    public abstract class ExpressionSyntax : SyntaxNode
    {
    }

    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public Token Token { get; }
        public LiteralExpressionSyntax(Token token)
        {
            this.Token = token;
        }

        public override string ToString() => this.Token.Value;

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
        public ArgumentListSyntax ArgumentList => Children.Count > 1 ? (ArgumentListSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class ArgumentListSyntax : SyntaxNode
    {
        public List<ExpressionSyntax> Arguments => Children.Cast<ExpressionSyntax>().ToList();

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

        public override string ToString() => this.OperatorToken.Value;

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

        public override string ToString() => this.OperatorToken.Value;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}