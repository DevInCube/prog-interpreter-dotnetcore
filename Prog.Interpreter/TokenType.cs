namespace Prog
{
    public enum TokenType
    {
        None,  // default
        Keyword,
        Operator,
        Separator,
        Literal,
        Identifier,
        Comment,  // extra
        Whitespace,  // extra
    }
}