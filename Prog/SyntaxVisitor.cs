namespace Prog
{
    public abstract class SyntaxVisitor<TResult>
    {
        public virtual TResult Visit(ProgramSyntax syntax) => default(TResult);
        public virtual TResult Visit(VariableDeclarationStatementSyntax syntax) => default(TResult);
        public virtual TResult Visit(ExpressionStatementSyntax syntax) => default(TResult);
        public virtual TResult Visit(IfStatementSyntax syntax) => default(TResult);
        public virtual TResult Visit(WhileStatementSyntax syntax) => default(TResult);
        public virtual TResult Visit(BlockSyntax syntax) => default(TResult);
        public virtual TResult Visit(ExpressionSyntax syntax) => default(TResult);
        public virtual TResult Visit(LiteralExpressionSyntax syntax) => default(TResult);
        public virtual TResult Visit(UnaryExpressionSyntax syntax) => default(TResult);
        public virtual TResult Visit(BinaryExpressionSyntax syntax) => default(TResult);
        public virtual TResult Visit(InvocationExpressionSyntax syntax) => default(TResult);
        public virtual TResult Visit(ArgumentListSyntax syntax) => default(TResult);
        public virtual TResult Visit(IdentifierNameSyntax syntax) => default(TResult);
    }
}