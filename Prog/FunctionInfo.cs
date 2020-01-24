using System;
using System.Collections.Generic;

namespace Prog
{
    public class FunctionInfo
    {
        public Type ResultType { get; }
        public Type[] ArgumentTypes { get; }
        public Func<ProgValue[], ProgValue> Function { get; }

        public FunctionInfo(Type[] argTypes, Type resultType, Func<ProgValue[], ProgValue> func)
        {
            if (argTypes != null)
                foreach (var type in argTypes)
                    if (!typeof(ProgValue).IsAssignableFrom(type))
                        throw new ArgumentOutOfRangeException("Parameter type should be ProgValue type");
            if (!typeof(ProgValue).IsAssignableFrom(resultType))
                throw new ArgumentOutOfRangeException("Result type should be ProgValue type");
            this.ArgumentTypes = argTypes;
            this.ResultType = resultType;
            this.Function = func;
        }

        public ProgValue Call(ProgValue[] arguments)
        {
            if (ArgumentTypes != null)
            {
                if (arguments.Length != ArgumentTypes.Length)
                    throw new Exception($"Invalid number of arguments. " +
                        $"Expected {ArgumentTypes.Length} got {arguments.Length}");
                for (var i = 0; i < ArgumentTypes.Length; i++)
                    if (!ArgumentTypes[i].IsAssignableFrom(arguments[i].GetType()))
                        throw new Exception($"Expected argument" +
                            $" of type {ArgumentTypes[i]} got {arguments[0].GetType()}");
            }
            var result = Function(arguments);
            if (!ResultType.IsAssignableFrom(result.GetType()))
                throw new Exception($"Expected return type {ResultType} got {result.GetType()}");
            return result;
        }
    }
}