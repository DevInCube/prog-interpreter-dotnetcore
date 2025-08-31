using System.Collections.Generic;
using System.Linq;

namespace Prog
{
    public class SymbolTable
{
    private readonly Stack<HashSet<Variable>> scopes = new Stack<HashSet<Variable>>();

    public void EnterScope() => scopes.Push(new HashSet<Variable>());
    public void LeaveScope() => scopes.Pop();
    public Variable FindSymbol(string symbol) => scopes.FirstOrDefault(x => x.Any(t => t.Name == symbol))?.FirstOrDefault(t => t.Name == symbol);
    public void AddSymbol(string symbol, ProgValue value) => scopes.Peek().Add(new Variable(symbol, value));
    public bool CheckScope(string symbol) => scopes.Peek().Any(x => x.Name == symbol);
}
}