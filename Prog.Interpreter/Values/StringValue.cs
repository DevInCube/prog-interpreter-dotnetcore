namespace Prog
{
    public class StringValue : ProgValue
    {
        public string Value { get; }

        public StringValue(string value)
        {
            Value = value;
        }

        public static implicit operator string(StringValue d) => d.Value;

        public static implicit operator StringValue(string b) => new StringValue(b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is StringValue value &&
                Value == value.Value;
        }
    }
}