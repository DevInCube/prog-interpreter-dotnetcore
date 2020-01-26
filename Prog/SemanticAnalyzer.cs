    // public class SemanticAnalyzer
    // {
    //     private Tree<AstNode> _tree;
    //     private SymbolTable _symbolTable = new SymbolTable();

    //     public SemanticAnalyzer(Tree<AstNode> tree)
    //     {
    //         this._tree = tree ?? throw new ArgumentNullException(nameof(tree));
    //     }

    //     public void Analyze()
    //     {
    //         if (_tree.Value.Type != AstNodeType.Program)
    //             throw new Exception("Root should be a program");
    //         Check(_tree);
    //     }

    //     private void Check(Tree<AstNode> tree, Tree<AstNode> parent = null)
    //     {
    //         switch (tree.Value.Type)
    //         {
    //             case AstNodeType.Operation when (string)tree.Value.Value == "=":
    //                 CheckAssignment(tree);
    //                 break;
    //             case AstNodeType.Identifier when tree.Children.Count == 1:
    //                 CheckFunctionCall(tree);
    //                 break;
    //             case AstNodeType.Identifier when tree.Children.Count == 0:
    //                 CheckIdentifierUse(tree, parent);
    //                 break;
    //             case AstNodeType.IfStatement:
    //             case AstNodeType.WhileStatement:
    //                 CheckTestExpression(tree);
    //                 break;
    //             case AstNodeType.Program:
    //             case AstNodeType.Block:
    //                 _symbolTable.EnterScope();
    //                 break;
    //         }
    //         foreach (var child in tree.Children)
    //             Check(child, tree);
    //         switch (tree.Value.Type)
    //         {
    //             case AstNodeType.Program:
    //             case AstNodeType.Block:
    //                 _symbolTable.LeaveScope();
    //                 break;
    //         }
    //     }

    //     private void CheckAssignment(Tree<AstNode> tree)
    //     {
    //         if (tree.Children[0].Value.Type != AstNodeType.Identifier)
    //             throw new Exception("Expected a variable name to the left of an assignment");
    //     }

    //     private void CheckFunctionCall(Tree<AstNode> tree)
    //     {
    //         var funcName = (string)tree.Value.Value;
    //         if (!Prog.Lang.Functions.ContainsKey(funcName))
    //             throw new Exception($"Undefined reference to function {funcName}");
    //     }

    //     private void CheckIdentifierUse(Tree<AstNode> tree, Tree<AstNode> parent)
    //     {
    //         var varName = (string)tree.Value.Value;
    //         if (parent.Value.Type == AstNodeType.VarDeclaration)
    //             if (_symbolTable.CheckScope(varName))
    //                 throw new Exception($"Variable {varName} has already been declared");
    //             else
    //                 _symbolTable.AddSymbol(varName);
    //         else
    //             if (_symbolTable.FindSymbol(varName) == null)
    //             throw new Exception($"Undefined variable {varName}");
    //     }

    //     private void CheckTestExpression(Tree<AstNode> tree)
    //     {
    //         bool isNotBooleanLiteral =
    //             tree.Children[0].Value.Type == AstNodeType.Number
    //             || tree.Children[0].Value.Type == AstNodeType.String
    //             || tree.Children[0].Value.Type == AstNodeType.None;
    //         var boolResultOperators = new[] { "<", "<=", ">", ">=", "==", "!=", "!", "&&", "||" };
    //         bool isNotBooleanOperator =
    //             tree.Children[0].Value.Type == AstNodeType.Operation
    //             && !boolResultOperators.Contains(tree.Children[0].Value.Value);
    //         if (isNotBooleanLiteral || isNotBooleanOperator)
    //             throw new Exception("Expected boolean in test expression result");
    //     }
    // }
