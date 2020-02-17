using System;

namespace Prog
{
    public abstract class ProgValue { }

    public class NoneValue : ProgValue
    {
        private NoneValue() { }

        public static NoneValue Value { get; } = new NoneValue();

        public override string ToString() => "none";

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj == Value;
        }
    }

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

    public class BooleanValue : ProgValue
    {
        public bool Value { get; }

        public BooleanValue(bool value)
        {
            this.Value = value;
        }

        public static implicit operator bool(BooleanValue d) => d.Value;
        public static implicit operator BooleanValue(bool b) => new BooleanValue(b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BooleanValue value &&
                Value == value.Value;
        }
    }

    public class StringValue : ProgValue
    {
        public string Value { get; }

        public StringValue(string value)
        {
            this.Value = value;
        }

        public static implicit operator string(StringValue d) => d.Value;
        public static implicit operator StringValue(string b) => new StringValue(b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public override bool Equals(object obj)
        {
            return obj is StringValue value &&
                Value == value.Value;
        }
    }
}