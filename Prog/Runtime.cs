using System;
using System.Linq;
using System.Collections.Generic;

namespace Prog
{
    public static class Runtime_old
    {
        public static void Execute(Tree<AstNode> tree)
        {
            var scopeStack = new List<Dictionary<string, ProgValue>>();


            void DeclareVariable(string name)
            {
                if (scopeStack.Last().ContainsKey(name))
                    throw new Exception($"Variable {name} is already declared");
                scopeStack.Last().Add(name, NoneValue.Value);
            }

            ProgValue GetVariableValue(string name)
            {
                for (var i = scopeStack.Count - 1; i >= 0; i--)
                    if (scopeStack[i].ContainsKey(name))
                        return scopeStack[i][name];
                throw new Exception($"Variable {name} is not declared");
            }

            void SetVariableValue(string name, ProgValue value)
            {
                for (var i = scopeStack.Count - 1; i >= 0; i--)
                    if (scopeStack[i].ContainsKey(name))
                    {
                        scopeStack[i][name] = value;
                        return;
                    }
                throw new Exception($"Variable {name} is not declared");
            }

            ProgValue CalculateExpression(Tree<AstNode> tree)
            {
                return tree.Value.Type switch
                {
                    AstNodeType.Identifier when (tree.Children.Count == 0) => GetVariableValue(tree.Value.Value as string),
                    AstNodeType.Identifier => CallFunction((string)tree.Value.Value, tree.Children[0]),
                    AstNodeType.Number => new NumberValue((double)tree.Value.Value),
                    AstNodeType.Boolean => new BooleanValue((bool)tree.Value.Value),
                    AstNodeType.String => new StringValue((string)tree.Value.Value),
                    AstNodeType.None => NoneValue.Value,
                    AstNodeType.Operation => CalculateOperation(tree),
                    _ => throw new Exception("This is not an expression")
                };
            }

            ProgValue CallFunction(string functionName, Tree<AstNode> args)
            {
                if (!Prog.Lang.Functions.TryGetValue(functionName, out var function))
                    throw new Exception($"Undefined function {functionName} call");
                var arguments = args.Children.Select(CalculateExpression).ToArray();
                return function.Call(arguments);
            }

            void VariableDeclaration(Tree<AstNode> tree)
            {
                var varName = (string)tree.Children[0].Value.Value;
                DeclareVariable(varName);
                if (tree.Children.Count > 1)
                {
                    var newValue = CalculateExpression(tree.Children[1]);
                    SetVariableValue(varName, newValue);
                }
            }

            void ExecuteStatement(Tree<AstNode> tree)
            {
                switch (tree.Value.Type)
                {
                    case AstNodeType.Block: BlockStatement(tree); break;
                    case AstNodeType.IfStatement: IfStatement(tree); break;
                    case AstNodeType.WhileStatement: WhileStatement(tree); break;
                    default: CalculateExpression(tree); break;
                }
            }

            void BlockStatement(Tree<AstNode> tree)
            {
                var blockScope = new Dictionary<string, ProgValue>();
                scopeStack.Add(blockScope);
                foreach (var node in tree.Children)
                    if (node.Value.Type == AstNodeType.VarDeclaration)
                        VariableDeclaration(node);
                    else
                        ExecuteStatement(node);
                scopeStack.Remove(blockScope);
            }

            void Program(Tree<AstNode> tree)
            {
                BlockStatement(tree);
            }

            void WhileStatement(Tree<AstNode> tree)
            {
                while (true)
                {
                    var testValue = CalculateExpression(tree.Children[0]);
                    if (!(testValue is BooleanValue boolValue))
                        throw new Exception("Expected boolean value in test expression");
                    if (!boolValue.Value) break;
                    ExecuteStatement(tree.Children[1]);
                }
            }

            void IfStatement(Tree<AstNode> tree)
            {
                var testValue = CalculateExpression(tree.Children[0]);
                if (!(testValue is BooleanValue boolValue))
                    throw new Exception("Expected boolean value in test expression");
                if (boolValue.Value)
                    ExecuteStatement(tree.Children[1]);
                else if (tree.Children.Count == 3)
                    ExecuteStatement(tree.Children[2]);
            }

            ProgValue CalculateOperation(Tree<AstNode> tree)
            {
                return tree switch
                {
                    var unary when (tree.Children.Count == 1) => CalculateUnaryOperation(unary),
                    var assign when ((string)tree.Value.Value == "=") => CalculateAssignment(assign),
                    var binary when (tree.Children.Count == 2) => CalculateBinaryOperation(binary),
                    _ => throw new Exception($"Invalid operation tree ({tree.Children.Count} children)")
                };
            }

            ProgValue CalculateAssignment(Tree<AstNode> tree)
            {
                if (tree.Children[0].Value.Type != AstNodeType.Identifier)
                    throw new Exception("Expected a variable name to the left of an assignment");
                var varName = (string)tree.Children[0].Value.Value;
                var newValue = CalculateExpression(tree.Children[1]);
                SetVariableValue(varName, newValue);
                return newValue;
            }

            var unaryOperations = new Dictionary<string, FunctionInfo> {
                {"+", new FunctionInfo(
                    new Type[]{typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => args[0] as NumberValue)
                },
                {"-", new FunctionInfo(
                    new Type[]{typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue(-(args[0] as NumberValue).Value))
                },
                {"!", new FunctionInfo(
                    new Type[]{typeof(BooleanValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue(!(args[0] as BooleanValue).Value))
                }
            };

            ProgValue CalculateUnaryOperation(Tree<AstNode> tree)
            {
                var op = (string)tree.Value.Value;
                if (!Lang.OperatorsTable.Where(x => x.Arity == OperatorArity.Unary).Select(x => x.Lexeme).Contains(op))
                    throw new Exception($"Invalid unary operator {op}");
                var operand = CalculateExpression(tree.Children[0]);
                return unaryOperations[op].Call(new ProgValue[] { operand });
            }

            var binaryOperations = new Dictionary<string, FunctionInfo> {
                {"+", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue((args[0] as NumberValue).Value + (args[1] as NumberValue).Value))
                },
                {"-", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue((args[0] as NumberValue).Value - (args[1] as NumberValue).Value))
                },
                {"*", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue((args[0] as NumberValue).Value * (args[1] as NumberValue).Value))
                },
                {"/", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue((args[0] as NumberValue).Value / (args[1] as NumberValue).Value))
                },
                {"%", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(NumberValue),
                    (ProgValue[] args) => new NumberValue((args[0] as NumberValue).Value % (args[1] as NumberValue).Value))
                },
                {"<", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as NumberValue).Value < (args[1] as NumberValue).Value))
                },
                {"<=", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as NumberValue).Value <= (args[1] as NumberValue).Value))
                },
                {">", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as NumberValue).Value > (args[1] as NumberValue).Value))
                },
                {">=", new FunctionInfo(
                    new Type[]{typeof(NumberValue),typeof(NumberValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as NumberValue).Value >= (args[1] as NumberValue).Value))
                },
                {"&&", new FunctionInfo(
                    new Type[]{typeof(BooleanValue),typeof(BooleanValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as BooleanValue).Value && (args[1] as BooleanValue).Value))
                },
                {"||", new FunctionInfo(
                    new Type[]{typeof(BooleanValue),typeof(BooleanValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue((args[0] as BooleanValue).Value || (args[1] as BooleanValue).Value))
                },
                {"==", new FunctionInfo(
                    new Type[]{typeof(ProgValue), typeof(ProgValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue(args[0].Equals(args[1])))
                },
                {"!=", new FunctionInfo(
                    new Type[]{typeof(ProgValue), typeof(ProgValue)},
                    typeof(BooleanValue),
                    (ProgValue[] args) => new BooleanValue(!args[0].Equals(args[1])))
                },
            };

            ProgValue CalculateBinaryOperation(Tree<AstNode> tree)
            {
                var op = (string)tree.Value.Value;
                if (!binaryOperations.ContainsKey(op))
                    throw new Exception($"Invalid binary operator {op}");
                var leftOperand = CalculateExpression(tree.Children[0]);
                var rightOperand = CalculateExpression(tree.Children[1]);
                return binaryOperations[op].Call(new ProgValue[] { leftOperand, rightOperand });
            }

            Program(tree);
        }
    }
}