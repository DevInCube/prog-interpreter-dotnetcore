using System.Linq;
using System;

namespace Prog
{
    public class ExecutionVisitor : SyntaxVisitor<ProgValue>
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();
        private int _indentationLevel = 0;
        private bool _enableLog = false;

        private void Log(string message)
        {
            if (!_enableLog) return;
            for (int i = 0; i < _indentationLevel; i++)
                Console.Write("  ");
            Console.WriteLine(message);
        }

        public override ProgValue Visit(ProgramSyntax syntax)
        {
            Log("PROGRAM BEGIN");
            _symbolTable.EnterScope();
            _indentationLevel += 1;
            foreach (var statement in syntax.Statements)
                statement.Accept(this);
            _indentationLevel -= 1;
            _symbolTable.LeaveScope();
            Log("PROGRAM END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(VariableDeclarationStatementSyntax syntax)
        {
            var value = syntax.Value?.Accept(this);
            Log($"VAR: {syntax.Identifier.Name} = {value}");
            _symbolTable.AddSymbol(syntax.Identifier.Name);
            if (value != null)
                _symbolTable.FindSymbol(syntax.Identifier.Name).Value = value;
            return value ?? NoneValue.Value;
        }

        public override ProgValue Visit(BlockSyntax syntax)
        {
            Log("BLOCK BEGIN");
            _symbolTable.EnterScope();
            _indentationLevel += 1;
            foreach (var statement in syntax.Statements)
                statement.Accept(this);
            _indentationLevel -= 1;
            _symbolTable.LeaveScope();
            Log("BLOCK END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(IfStatementSyntax syntax)
        {
            Log("IF BEGIN");
            var value = syntax.Condition.Accept(this);
            if (value is BooleanValue b && b.Value)
                syntax.ThenStatement.Accept(this);
            else
                syntax.ElseStatement?.Accept(this);
            Log("IF END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(WhileStatementSyntax syntax)
        {
            Log("WHILE BEGIN");
            while (true)
            {
                var value = syntax.Condition.Accept(this);
                if (value is BooleanValue bv && bv.Value)
                    syntax.Statement.Accept(this);
                else
                    break;
            }
            Log("WHILE END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(BinaryExpressionSyntax syntax)
        {
            Log($"BINARY: {syntax.OperatorToken}");
            _indentationLevel += 1;
            var leftOperand = syntax.Left.Accept(this);
            var rightOperand = syntax.Right.Accept(this);
            _indentationLevel -= 1;
            return syntax.OperatorToken.Value switch {
                "+" => new NumberValue((leftOperand as NumberValue) + (rightOperand as NumberValue)),
                "-" => new NumberValue((leftOperand as NumberValue) - (rightOperand as NumberValue)),
                "*" => new NumberValue((leftOperand as NumberValue) * (rightOperand as NumberValue)),
                "/" => new NumberValue((leftOperand as NumberValue) / (rightOperand as NumberValue)),
                "%" => new NumberValue((leftOperand as NumberValue) % (rightOperand as NumberValue)),
                "<" => new BooleanValue((leftOperand as NumberValue) < (rightOperand as NumberValue)),
                "<=" => new BooleanValue((leftOperand as NumberValue) <= (rightOperand as NumberValue)),
                ">" => new BooleanValue((leftOperand as NumberValue) > (rightOperand as NumberValue)),
                ">=" => new BooleanValue((leftOperand as NumberValue) >= (rightOperand as NumberValue)),
                "==" => new BooleanValue(leftOperand.Equals(rightOperand)),
                "!=" => new BooleanValue(!leftOperand.Equals(rightOperand)),
                "&&" => new BooleanValue((leftOperand as BooleanValue) && (rightOperand as BooleanValue)),
                "||" => new BooleanValue((leftOperand as BooleanValue) || (rightOperand as BooleanValue)),
                "=" => Assignment(),
                _ => throw new Exception($"Unsupported binary operator `{syntax.OperatorToken.Value}`."),
            };

            ProgValue Assignment()
            {
                var varName = (syntax.Left as IdentifierNameSyntax).Name;
                _symbolTable.AddSymbol(varName);
                var value = syntax.Right.Accept(this);
                _symbolTable.FindSymbol(varName).Value = value;
                return value;
            }
        }

        public override ProgValue Visit(UnaryExpressionSyntax syntax)
        {
            Log($"UNARY: {syntax.OperatorToken}");
            _indentationLevel += 1;
            var operandValue = syntax.Operand.Accept(this);
            _indentationLevel -= 1;
            return syntax.OperatorToken.Value switch {
                "+" => operandValue,
                "-" => new NumberValue(-(operandValue as NumberValue).Value),
                "!" => new BooleanValue(!(operandValue as BooleanValue).Value),
                _ => throw new Exception("Unsupported unary operator."),
            };
        }

        public override ProgValue Visit(ExpressionStatementSyntax syntax)
        {
            return syntax.Expression.Accept(this);
        }

        public override ProgValue Visit(LiteralExpressionSyntax syntax)
        {
            Log($"LITERAL: {syntax.Token.Value}");
            return syntax.Token.Value switch {
                "none" => NoneValue.Value,
                "true" => new BooleanValue(true),
                "false" => new BooleanValue(false),
                var s when s.StartsWith("\"") => new StringValue(s.Substring(1, s.Length - 2)),
                _ => new NumberValue(double.Parse(syntax.Token.Value)),
            };
        }

         public override ProgValue Visit(IdentifierNameSyntax syntax)
        {
            var value = _symbolTable.FindSymbol(syntax.Name)?.Value;  // @todo
            Log($"ID: {syntax.Name} = {value}");
            return value;
        }

        public override ProgValue Visit(InvocationExpressionSyntax syntax)
        {
            var arguments = syntax.ArgumentList.Arguments.Select(a => a.Accept(this)).ToArray();
            Log($"INVOCATION: {syntax.IdentifierName.Name}, {arguments.Length}");
            if (!Lang.Functions.TryGetValue(syntax.IdentifierName.Name, out var function))
                throw new Exception($"Undefined function `{syntax.IdentifierName.Name}`.");
            return function.Call(arguments);
        }


    }
}
