namespace Prog
{
    public abstract class SyntaxVisitor<TResult>
    {
        public virtual TResult Visit(ProgramSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(VariableDeclarationStatementSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(ExpressionStatementSyntax syntax)
        {
            return default(TResult);
        }
        public virtual TResult Visit(IfStatementSyntax syntax)
        {
            return default(TResult);
        }
        public virtual TResult Visit(WhileStatementSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(BlockSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(ExpressionSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(LiteralExpressionSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(UnaryExpressionSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(BinaryExpressionSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(InvocationExpressionSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(ArgumentListSyntax syntax)
        {
            return default(TResult);
        }

        public virtual TResult Visit(IdentifierNameSyntax syntax)
        {
            return default(TResult);
        }
    }
}