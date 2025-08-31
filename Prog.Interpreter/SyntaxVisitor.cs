namespace Prog
{
    public abstract class SyntaxVisitor<TResult>
    {
        public virtual TResult Visit(ProgramSyntax syntax) => default;
        public virtual TResult Visit(EmptyStatement syntax) => default;
        public virtual TResult Visit(BlockedStatement syntax) => default;
        public virtual TResult Visit(VariableDeclarationStatementSyntax syntax) => default;
        public virtual TResult Visit(IfStatementSyntax syntax) => default;
        public virtual TResult Visit(WhileStatementSyntax syntax) => default;
        public virtual TResult Visit(BlockSyntax syntax) => default;
        public virtual TResult Visit(ExpressionSyntax syntax) => default;
        public virtual TResult Visit(LiteralExpressionSyntax syntax) => default;
        public virtual TResult Visit(UnaryExpressionSyntax syntax) => default;
        public virtual TResult Visit(BinaryExpressionSyntax syntax) => default;
        public virtual TResult Visit(InvocationExpressionSyntax syntax) => default;
        public virtual TResult Visit(ArgumentListSyntax syntax) => default;
        public virtual TResult Visit(IdentifierNameSyntax syntax) => default;
    }
}