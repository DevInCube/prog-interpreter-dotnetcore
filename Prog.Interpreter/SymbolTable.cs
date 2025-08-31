namespace Prog
{
    public class SymbolTable
    {
        private readonly Stack<HashSet<Variable>> _scopes = new Stack<HashSet<Variable>>();

        public void EnterScope() => _scopes.Push(new HashSet<Variable>());

        public void LeaveScope() => _scopes.Pop();

        public Variable? FindSymbol(string symbol) => _scopes.FirstOrDefault(x => x.Any(t => t.Name == symbol))?.FirstOrDefault(t => t.Name == symbol);

        public void AddSymbol(string symbol, ProgValue value) => _scopes.Peek().Add(new Variable(symbol, value));

        public bool CheckScope(string symbol) => _scopes.Peek().Any(x => x.Name == symbol);
    }
}