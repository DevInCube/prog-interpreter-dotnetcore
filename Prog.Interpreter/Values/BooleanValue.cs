namespace Prog
{
    public class BooleanValue : ProgValue
    {
        public bool Value { get; }

        public BooleanValue(bool value)
        {
            Value = value;
        }

        public static implicit operator bool(BooleanValue d) => d.Value;

        public static implicit operator BooleanValue(bool b) => new BooleanValue(b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is BooleanValue value &&
                Value == value.Value;
        }
    }
}