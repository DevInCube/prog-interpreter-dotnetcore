// using System.Linq;
// using System.Linq.Expressions;
// using System.Collections.Generic;

// namespace Prog
// {
//     public class Runtime
//     {
//         public SymbolTable SymbolTable { get; } = new SymbolTable();
//     }

//     public interface IExecutable
//     {
//         void Execute(Runtime runtime);
//     }

//     public interface IExpression
//     {
//         ProgValue Calculate(Runtime runtime);
//     }

//     public interface INumericalExpression : IExpression
//     {
//         new NumberValue Calculate(Runtime runtime);
//     }

//     public interface ILogicalExpression : IExpression
//     {
//         new BooleanValue Calculate(Runtime runtime);
//     }

//     public interface IStringExpression : IExpression
//     {
//         new StringValue Calculate(Runtime runtime);
//     }

//     public interface INoneExpression : IExpression
//     {
//         new NoneValue Calculate(Runtime runtime);
//     }

//     public abstract class Ast : IExecutable
//     {
//         public abstract void Execute(Runtime runtime);
//     }

//     public class ProgramAst : Ast
//     {
//         public List<Ast> Items { get; } = new List<Ast>();

//         public override void Execute(Runtime runtime)
//         {
//             runtime.SymbolTable.EnterScope();
//             foreach (var item in Items)
//                 item.Execute(runtime);
//             runtime.SymbolTable.LeaveScope();
//         }
//     }

//     public class VariableDeclarationAst : Ast
//     {
//         public string VariableName { get; }
//         public IExpression InitExpression { get; }

//         public VariableDeclarationAst(string varName, IExpression initExpr = null)
//         {
//             VariableName = varName;
//             InitExpression = initExpr;
//         }

//         public override void Execute(Runtime runtime)
//         {
//             runtime.SymbolTable.AddSymbol(this.VariableName);
//             var variable = runtime.SymbolTable.FindSymbol(this.VariableName);
//             variable.Value = InitExpression.Calculate(runtime);
//         }
//     }

//     public abstract class StatementAst : Ast
//     {

//     }

//     public class IfStatementAst : StatementAst
//     {
//         public IExpression Expression { get; }
//         public StatementAst ThenStatement { get; }
//         public StatementAst ElseStatement { get; }

//         public IfStatementAst(IExpression expression, StatementAst thenStatement, StatementAst elseStatement = null)
//         {
//             Expression = expression;
//             ThenStatement = thenStatement;
//             ElseStatement = elseStatement;
//         }

//         public override void Execute(Runtime runtime)
//         {
//             var result = this.Expression.Calculate(runtime);
//             if (!(result is BooleanValue booleanValue))
//                 throw new System.Exception("Expected boolean result in test expression");
//             if (booleanValue.Value)
//                 ThenStatement.Execute(runtime);
//             else if (ElseStatement != null)
//                 ElseStatement.Execute(runtime);
//         }
//     }

//     public class WhileStatementAst : StatementAst
//     {
//         public IExpression Expression { get; }
//         public StatementAst Statement { get; }

//         public WhileStatementAst(IExpression expression, StatementAst statement, StatementAst elseStatement = null)
//         {
//             Expression = expression;
//             Statement = statement;
//         }

//         public override void Execute(Runtime runtime)
//         {
//             while (true)
//             {
//                 var result = this.Expression.Calculate(runtime);
//                 if (!(result is BooleanValue booleanValue))
//                     throw new System.Exception("Expected boolean result in test expression");
//                 if (!booleanValue.Value)
//                     break;
//                 Statement.Execute(runtime);
//             }
//         }
//     }

//     public class CompoundStatementAst : StatementAst
//     {
//         public List<Ast> Items { get; } = new List<Ast>();

//         public override void Execute(Runtime runtime)
//         {
//             runtime.SymbolTable.EnterScope();
//             foreach (var item in Items)
//                 item.Execute(runtime);
//             runtime.SymbolTable.LeaveScope();
//         }
//     }

//     public class ExpressionStatementAst : StatementAst
//     {
//         public IExpression Expression { get; }

//         public ExpressionStatementAst(IExpression expression)
//         {
//             this.Expression = expression;
//         }

//         public override void Execute(Runtime runtime)
//         {
//             var _ = Expression.Calculate(runtime);
//         }
//     }

//     public abstract class UnaryExpression : IExpression
//     {
//         public IExpression Expression { get; }
//         public UnaryExpression(IExpression expression)
//         {
//             this.Expression = expression;
//         }
//         public abstract ProgValue Calculate(Runtime runtime);
//     }

//     public abstract class BinaryExpression : IExpression
//     {
//         public IExpression LeftExpression { get; }
//         public IExpression RightExpression { get; }
//         public BinaryExpression(IExpression leftExpression, IExpression rightExpression)
//         {
//             this.LeftExpression = leftExpression;
//             this.RightExpression = rightExpression;
//         }
//         public abstract ProgValue Calculate(Runtime runtime);
//     }

//     public class AssignmentExpression : BinaryExpression
//     {
//         public AssignmentExpression(IExpression variableId, IExpression expression)
//             : base(variableId, expression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             if (!(this.LeftExpression is Identifier id))
//                 throw new System.Exception("Expected an identifier");
//             var value = this.RightExpression.Calculate(runtime);
//             id.Assign(runtime, value);
//             return value;
//         }
//     }

//     public class DivisionExpression : BinaryExpression, INumericalExpression
//     {
//         public DivisionExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 / (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class ModulusDivisionExpression : BinaryExpression, INumericalExpression
//     {
//         public ModulusDivisionExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 % (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class SumExpression : BinaryExpression, INumericalExpression
//     {
//         public SumExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 + (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class DifferenceExpression : BinaryExpression, INumericalExpression
//     {
//         public DifferenceExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 - (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class PositiveExpression : UnaryExpression, INumericalExpression
//     {
//         public PositiveExpression(INumericalExpression expression) : base(expression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue((this.Expression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class NegativeExpression : UnaryExpression, INumericalExpression
//     {
//         public NegativeExpression(INumericalExpression expression) : base(expression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         NumberValue INumericalExpression.Calculate(Runtime runtime)
//         {
//             return new NumberValue(-(this.Expression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class ComplementExpression : UnaryExpression, ILogicalExpression
//     {
//         public ComplementExpression(ILogicalExpression expression) : base(expression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue(!(this.Expression as ILogicalExpression).Calculate(runtime));
//         }
//     }

//     public class LogicalAndExpression : BinaryExpression, ILogicalExpression
//     {
//         public LogicalAndExpression(ILogicalExpression leftExpression, ILogicalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as ILogicalExpression).Calculate(runtime)
//                 && (this.RightExpression as ILogicalExpression).Calculate(runtime));
//         }
//     }

//     public class LogicalOrExpression : BinaryExpression, ILogicalExpression
//     {
//         public LogicalOrExpression(ILogicalExpression leftExpression, ILogicalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as ILogicalExpression).Calculate(runtime)
//                 || (this.RightExpression as ILogicalExpression).Calculate(runtime));
//         }
//     }

//     public class EqualityExpression : BinaryExpression
//     {
//         public EqualityExpression(IExpression leftExpression, IExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return new BooleanValue(this.LeftExpression.Calculate(runtime).Equals(this.RightExpression.Calculate(runtime)));
//         }
//     }

//     public class NonEqualityExpression : BinaryExpression
//     {
//         public NonEqualityExpression(IExpression leftExpression, IExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return new BooleanValue(!this.LeftExpression.Calculate(runtime).Equals(this.RightExpression.Calculate(runtime)));
//         }
//     }

//     public class LessThanExpression : BinaryExpression, ILogicalExpression
//     {
//         public LessThanExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 < (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class LessThanEqualsExpression : BinaryExpression, ILogicalExpression
//     {
//         public LessThanEqualsExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 <= (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class GreaterThanExpression : BinaryExpression, ILogicalExpression
//     {
//         public GreaterThanExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 > (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class GreaterThanEqualsExpression : BinaryExpression, ILogicalExpression
//     {
//         public GreaterThanEqualsExpression(INumericalExpression leftExpression, INumericalExpression rightExpression) : base(leftExpression, rightExpression)
//         {
//         }

//         public override ProgValue Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }

//         BooleanValue ILogicalExpression.Calculate(Runtime runtime)
//         {
//             return new BooleanValue((this.LeftExpression as INumericalExpression).Calculate(runtime)
//                 >= (this.RightExpression as INumericalExpression).Calculate(runtime));
//         }
//     }

//     public class BooleanLiteral : ILogicalExpression
//     {
//         private readonly BooleanValue _value;
//         public BooleanLiteral(BooleanValue value)
//         {
//             this._value = value;
//         }

//         public BooleanValue Calculate(Runtime runtime)
//         {
//             return _value;
//         }

//         ProgValue IExpression.Calculate(Runtime runtime)
//         {
//             return ((ILogicalExpression)this).Calculate(runtime);
//         }
//     }

//     public class NumberLiteral : INumericalExpression
//     {
//         private readonly NumberValue _value;
//         public NumberLiteral(NumberValue value)
//         {
//             this._value = value;
//         }

//         public NumberValue Calculate(Runtime runtime)
//         {
//             return _value;
//         }

//         ProgValue IExpression.Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }
//     }

//     public class StringLiteral : IStringExpression
//     {
//         private readonly StringValue _value;
//         public StringLiteral(StringValue value)
//         {
//             this._value = value;
//         }

//         public StringValue Calculate(Runtime runtime)
//         {
//             return _value;
//         }

//         ProgValue IExpression.Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }
//     }

//     public class NoneLiteral : INoneExpression
//     {
//         public NoneLiteral()
//         {
//         }

//         public NoneValue Calculate(Runtime runtime)
//         {
//             return NoneValue.Value;
//         }

//         ProgValue IExpression.Calculate(Runtime runtime)
//         {
//             return ((INumericalExpression)this).Calculate(runtime);
//         }
//     }

//     public interface IAssignable
//     {
//         void Assign(Runtime runtime, ProgValue value);
//     }

//     public class Identifier : IExpression, IAssignable
//     {
//         private readonly string _name;

//         public Identifier(string name)
//         {
//             this._name = name;
//         }

//         public void Assign(Runtime runtime, ProgValue value)
//         {
//             if (!runtime.SymbolTable.CheckScope(this._name))
//                 throw new System.Exception($"Variable `{this._name}` not found");
//             var variable = runtime.SymbolTable.FindSymbol(this._name);
//             variable.Value = value;
//         }

//         public ProgValue Calculate(Runtime runtime)
//         {
//             var variable = runtime.SymbolTable.FindSymbol(this._name);
//             return variable.Value;
//         }
//     }

//     public class FunctionCallExpression : IExpression
//     {
//         private readonly string _name;
//         private readonly List<IExpression> _arguments;
//         public FunctionCallExpression(string name, List<IExpression> arguments)
//         {
//             this._name = name;
//             this._arguments = arguments;
//         }

//         public ProgValue Calculate(Runtime runtime)
//         {
//             var variable = runtime.SymbolTable.FindSymbol(this._name);
//             // @todo implement function call
//             var argumentValues = this._arguments.Select(x => x.Calculate(runtime));
//             return NoneValue.Value;
//         }
//     }
// }