namespace Prog
{
    public abstract class SyntaxVisitor<TResult>
    {
        public virtual TResult Visit(ProgramSyntax syntax) => default;

        public virtual TResult Visit(EmptyExpression syntax) => default;

        public virtual TResult Visit(Statement syntax) => default;

        public virtual TResult Visit(VariableDeclarationExpressionSyntax syntax) => default;

        public virtual TResult Visit(IfExpressionSyntax syntax) => default;

        public virtual TResult Visit(WhileExpressionSyntax syntax) => default;

        public virtual TResult Visit(BlockExpression syntax) => default;

        public virtual TResult Visit(ExpressionSyntax syntax) => default;

        public virtual TResult Visit(LiteralExpressionSyntax syntax) => default;

        public virtual TResult Visit(UnaryExpressionSyntax syntax) => default;

        public virtual TResult Visit(BinaryExpressionSyntax syntax) => default;

        public virtual TResult Visit(InvocationExpressionSyntax syntax) => default;

        public virtual TResult Visit(ArgumentListSyntax syntax) => default;

        public virtual TResult Visit(IdentifierNameSyntax syntax) => default;
    }
}