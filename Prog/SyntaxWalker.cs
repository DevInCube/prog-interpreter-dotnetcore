namespace Prog
{
    public class SyntaxWalker<TResult> : SyntaxVisitor<TResult>
    {
        public override TResult Visit(ProgramSyntax syntax)
        {
            foreach (var s in syntax.Statements)
                s.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(VariableDeclarationStatementSyntax syntax)
        {
            syntax.Identifier.Accept(this);
            syntax.Value?.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(IfStatementSyntax syntax)
        {
            syntax.Condition.Accept(this);
            syntax.ThenStatement.Accept(this);
            syntax.ElseStatement?.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(WhileStatementSyntax syntax)
        {
            syntax.Condition.Accept(this);
            syntax.Statement.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(ExpressionStatementSyntax syntax)
        {
            syntax.Expression.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(BinaryExpressionSyntax syntax)
        {
            syntax.Left.Accept(this);
            syntax.Right.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(UnaryExpressionSyntax syntax)
        {
            syntax.Operand.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(InvocationExpressionSyntax syntax)
        {
            syntax.ArgumentList.Accept(this);
            return base.Visit(syntax);
        }

        public override TResult Visit(ArgumentListSyntax syntax)
        {
            foreach (var argument in syntax.Arguments)
                argument.Accept(this);
            return base.Visit(syntax);
        }
    }
}