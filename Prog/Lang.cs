using System;
using System.Collections.Generic;

namespace Prog
{
    public static class Lang
    {
        public static readonly string[] Keywords = {
            "let",
            "if",
            "else",
            "while",
            "none",
            "true",
            "false"
        };

        public static readonly List<string> Separators = new List<string>{
            "(",
            ")",
            "{",
            "}",
            ","
        };

        public static readonly OperatorInfo[] OperatorsTable = {
            new OperatorInfo ("+", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new OperatorInfo ("-", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new OperatorInfo ("!", OperatorArity.Unary, 1, OperatorAssociativity.RightToLeft),
            new OperatorInfo ("*", OperatorArity.Binary, 2),
            new OperatorInfo ("/", OperatorArity.Binary, 2),
            new OperatorInfo ("%", OperatorArity.Binary, 2),
            new OperatorInfo ("+", OperatorArity.Binary, 3),
            new OperatorInfo ("-", OperatorArity.Binary, 3),
            new OperatorInfo ("<", OperatorArity.Binary, 5),
            new OperatorInfo ("<=", OperatorArity.Binary, 5),
            new OperatorInfo (">", OperatorArity.Binary, 5),
            new OperatorInfo (">=", OperatorArity.Binary, 5),
            new OperatorInfo ("==", OperatorArity.Binary, 6),
            new OperatorInfo ("!=", OperatorArity.Binary, 6),
            new OperatorInfo ("&&", OperatorArity.Binary, 10),
            new OperatorInfo ("||", OperatorArity.Binary, 11),
            new OperatorInfo ("=", OperatorArity.Binary, 13, OperatorAssociativity.RightToLeft)
        };

        public static Dictionary<string, FunctionInfo> Functions = new Dictionary<string, FunctionInfo> {
            {
                "sin",
                new FunctionInfo (
                    new Type[] { typeof (NumberValue) },
                    typeof (NumberValue),
                    (ProgValue[] args) => (NumberValue)Math.Sin(args[0] as NumberValue))
            },
            {
                "floor",
                new FunctionInfo (
                    new Type[] { typeof (NumberValue) },
                    typeof (NumberValue),
                    (ProgValue[] args) => (NumberValue)Math.Floor (args[0] as NumberValue))
            },
            {
                "print",
                new FunctionInfo (null, typeof (NoneValue),
                    (ProgValue[] args) => {
                        foreach (var arg in args)
                            Console.Write ($"{arg} ");
                        Console.WriteLine ();
                        return NoneValue.Value;
                    })
            },
        };
    }

    public enum AstNodeType
    {
        Program,
        VarDeclaration,
        Block,
        IfStatement,
        WhileStatement,
        Operation,
        ArgsList,
        Identifier,
        Number,
        Boolean,
        String,
        None,
    }

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