using System.Linq;
using System;

namespace Prog
{
    public class ExecutionVisitor : SyntaxVisitor<ProgValue>
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();
        private readonly ExecutionLogger _logger = new ExecutionLogger();

        public override ProgValue Visit(ProgramSyntax syntax)
        {
            _logger.Log("PROGRAM BEGIN");
            _symbolTable.EnterScope();
            _logger.Indent();
            foreach (var statement in syntax.Statements)
                statement.Accept(this);
            _logger.Unindent();
            _symbolTable.LeaveScope();
            _logger.Log("PROGRAM END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(VariableDeclarationStatementSyntax syntax)
        {
            var value = syntax.Value?.Accept(this) ?? NoneValue.Value;
            _logger.Log($"VAR: {syntax.Identifier.Name} = {value}");
            _symbolTable.AddSymbol(syntax.Identifier.Name, value);
            return value;
        }

        public override ProgValue Visit(BlockSyntax syntax)
        {
            _logger.Log("BLOCK BEGIN");
            _symbolTable.EnterScope();
            _logger.Indent();
            foreach (var statement in syntax.Statements)
                statement.Accept(this);
            _logger.Unindent();
            _symbolTable.LeaveScope();
            _logger.Log("BLOCK END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(IfStatementSyntax syntax)
        {
            _logger.Log("IF BEGIN");
            var value = syntax.Condition.Accept(this);
            if (value is BooleanValue b && b.Value)
                syntax.ThenStatement.Accept(this);
            else
                syntax.ElseStatement?.Accept(this);
            _logger.Log("IF END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(WhileStatementSyntax syntax)
        {
            _logger.Log("WHILE BEGIN");
            while (syntax.Condition.Accept(this) is BooleanValue bv && bv.Value)
                 syntax.Statement.Accept(this);
            _logger.Log("WHILE END");
            return NoneValue.Value;
        }

        public override ProgValue Visit(ExpressionStatementSyntax syntax)
        {
            return syntax.Expression.Accept(this);
        }

        public override ProgValue Visit(BinaryExpressionSyntax syntax)
        {
            _logger.Log($"BINARY: {syntax.OperatorToken}");
            _logger.Indent();
            var leftOperand = syntax.Left.Accept(this);
            var rightOperand = syntax.Right.Accept(this);
            _logger.Unindent();
            return syntax.OperatorToken.Value switch
            {
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
                var value = syntax.Right.Accept(this);
                _symbolTable.FindSymbol(varName).Value = value;
                return value;
            }
        }

        public override ProgValue Visit(UnaryExpressionSyntax syntax)
        {
            _logger.Log($"UNARY: {syntax.OperatorToken}");
            _logger.Indent();
            var operandValue = syntax.Operand.Accept(this);
            _logger.Unindent();
            return syntax.OperatorToken.Value switch
            {
                "+" => operandValue,
                "-" => new NumberValue((operandValue is NumberValue number) ? -number : throw new InvalidOperationException("Expected a number.")),
                "!" => new BooleanValue((operandValue is BooleanValue boolean) ? !boolean : throw new InvalidOperationException("Expected a boolean.")),
                _ => throw new Exception("Unsupported unary operator."),
            };
        }

        public override ProgValue Visit(LiteralExpressionSyntax syntax)
        {
            _logger.Log($"LITERAL: {syntax.Token.Value}");
            return syntax.Token.Value switch
            {
                "none" => NoneValue.Value,
                "true" => new BooleanValue(true),
                "false" => new BooleanValue(false),
                var s when s.StartsWith("\"") => new StringValue(s.Substring(1, s.Length - 2)),
                _ => new NumberValue(double.Parse(syntax.Token.Value)),
            };
        }

        public override ProgValue Visit(IdentifierNameSyntax syntax)
        {
            var symbol = _symbolTable.FindSymbol(syntax.Name)
                ?? throw new Exception($"Undefined symbol `{syntax.Name}`.");
            var value = symbol.Value;
            _logger.Log($"ID: {syntax.Name} = {value}");
            return value;
        }

        public override ProgValue Visit(InvocationExpressionSyntax syntax)
        {
            var arguments = syntax.ArgumentList.Arguments.Select(a => a.Accept(this)).ToArray();
            _logger.Log($"INVOCATION: {syntax.IdentifierName.Name}, {arguments.Length}");
            if (!Lang.Functions.TryGetValue(syntax.IdentifierName.Name, out var function))
                throw new Exception($"Undefined function `{syntax.IdentifierName.Name}`.");
            return function.Call(arguments);
        }
    }
}
