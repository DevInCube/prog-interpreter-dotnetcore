namespace Prog
{
    public class NoneValue : ProgValue
    {
        private NoneValue() { }

        public static NoneValue Value { get; } = new NoneValue();

        public override string ToString() => "none";

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj == Value;
        }
    }
}