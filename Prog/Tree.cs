using System.Collections.Generic;

namespace Prog
{
    public class Tree<T>
    {
        public T Value { get; }
        public List<Tree<T>> Children { get; } = new List<Tree<T>>();
        public Tree(T value, params Tree<T>[] children)
        {
            this.Value = value;
            this.Children.AddRange(children);
        }
    }

    public static class Tree
    {
        public static Tree<T> Create<T>(T value, params Tree<T>[] children)
                    => new Tree<T>(value, children);
    }
}