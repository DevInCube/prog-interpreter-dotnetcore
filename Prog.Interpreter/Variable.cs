using System;

namespace Prog
{
    public class Variable
    {
        public string Name { get; }
        public ProgValue Value { get; set; }

        public Variable(string name, ProgValue value = null)
        {
            this.Name = name;
            this.Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Variable variable &&
                    Name == variable.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}