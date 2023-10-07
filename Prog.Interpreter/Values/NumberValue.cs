namespace Prog
{
    public class NumberValue : ProgValue
    {
        public double Value { get; }

        public NumberValue(double value)
        {
            this.Value = value;
        }

        public static implicit operator double(NumberValue d) => d.Value;
        public static implicit operator NumberValue(double b) => new NumberValue(b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public override bool Equals(object obj)
        {
            return obj is NumberValue value &&
                Value == value.Value;
        }
    }
}