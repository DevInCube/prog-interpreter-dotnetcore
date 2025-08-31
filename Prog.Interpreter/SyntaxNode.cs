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
        public List<BlockedStatement> Statements => Children.Cast<BlockedStatement>().ToList();

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class StatementSyntax : SyntaxNode
    {
    }

    public sealed class EmptyStatement : StatementSyntax
    {
        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class BlockedStatement : StatementSyntax
    {
        public StatementSyntax Statement => (StatementSyntax)Children.First();

        public BlockedStatement(StatementSyntax statement)
        {
            Children.Add(statement);
        }

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(IdentifierNameSyntax declarator)
        {
            Children.Add(declarator);
        }

        public IdentifierNameSyntax Identifier => (IdentifierNameSyntax)Children.First();

        public StatementSyntax Value => Children.Count > 1 ? (StatementSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class IfStatementSyntax : StatementSyntax
    {
        public StatementSyntax Condition => (StatementSyntax)Children[0];
        public BlockedStatement ThenStatement => (BlockedStatement)Children[1];
        public BlockedStatement ElseStatement => Children.Count > 2 ? (BlockedStatement)Children[2] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public StatementSyntax Condition => (StatementSyntax)Children[0];
        public BlockedStatement Statement => (BlockedStatement)Children[1];

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class BlockSyntax : StatementSyntax
    {
        public List<BlockedStatement> Statements => Children.Cast<BlockedStatement>().ToList();

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class ExpressionSyntax : StatementSyntax
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

    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(Token token, StatementSyntax expression)
        {
            OperatorToken = token;
            Children.Add(expression);
        }
        public Token OperatorToken { get; }
        public StatementSyntax Operand => (StatementSyntax)Children.First();

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
            StatementSyntax leftExpression,
            StatementSyntax rightExpression)
        {
            OperatorToken = token;
            Children.Add(leftExpression);
            Children.Add(rightExpression);
        }

        public Token OperatorToken { get; }
        public StatementSyntax Left => (StatementSyntax)Children[0];
        public StatementSyntax Right => (StatementSyntax)Children[1];

        public override string ToString() => this.OperatorToken.Value;

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
        public ArgumentListSyntax ArgumentList => Children.Count > 1 ? (ArgumentListSyntax)Children[1] : null;

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class ArgumentListSyntax : SyntaxNode
    {
        public List<StatementSyntax> Arguments => Children.Cast<StatementSyntax>().ToList();

        public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}