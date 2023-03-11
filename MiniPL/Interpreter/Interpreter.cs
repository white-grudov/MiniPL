namespace MiniPL
{
    /* Interpreter is the main component of the MiniPL interpreter, which takes the
     * valid AST, checked by the semantic analyzer, and executes it statement by
     * statement.
     */
    public class Interpreter : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;
        public Interpreter(AST ast)
        {
            Context = Context.GetInstance();
            Ast = ast;
        }
        // Runs the program
        public void Interpret()
        {
            Ast.Root.Accept(this);
        }
        public void Visit(ProgNode node)
        {
            node.Stmts?.Accept(this);
        }
        public void Visit(StmtsNode node)
        {
            foreach (var child in node.GetAllChildren())
                child.Accept(this);
        }
        // If expr is null returns, otherwise assigns the value
        public void Visit(DeclNode node)
        {
            if (node.Expr == null) return;
            string name = node.Ident.Token.Value;
            object value = node.Expr.Accept(this);
            Context.Assign(name, value);
        }
        // Assigns the value
        public void Visit(AssignNode node)
        {
            string name = node.Ident.Token.Value;
            object value = node.Expr.Accept(this);
            Context.Assign(name, value);
        }
        // Gets the values of lower and upper bounds of range and runs the nested statements
        public void Visit(ForNode node)
        {
            string indexName = node.Ident.Token.Value;
            int lowerBound = ToInt(node.StartExpr.Accept(this));
            int upperBound = ToInt(node.EndExpr.Accept(this));

            if (upperBound < lowerBound) return;
            for (int i = lowerBound; i <= upperBound; ++i)
            {
                Context.Assign(indexName, i);
                node.Stmts.Accept(this);
            }
        }
        // Evaluates the condition and runs the nested statements
        public void Visit(IfNode node)
        {
            bool condition = (bool)node.Expr.Accept(this);
            if (condition)
            {
                node.IfStmts.Accept(this);
            }
            else
            {
                node.ElseStmts?.Accept(this);
            }
        }
        // Evaluates the expression and prints it
        public void Visit(PrintNode node)
        {
            object expr = node.Expr.Accept(this);
            Console.Write(expr);
        }
        /* Reads the input and assigns it to value. If input cannot be casted to int if needed,
         * the exception is thrown
         */
        public void Visit(ReadNode node)
        {
            string name = node.Ident.Token.Value;
            string? input = Console.ReadLine();
            string type = Context.GetVariableType(name);
            if (type == "int")
            {
                if (!int.TryParse(input, out _))
                {
                    throw new RuntimeError(ErrorMessage.RE_CAST_TO_INT, node.Ident.Token.Pos);
                }
            }
            string value = input ?? "";
            Context.Assign(name, value);
        }
        public object Visit(ExprNode node)
        {
            // expr has only one operand
            if (node.GetType() == typeof(LExprNode))
            {
                return ((LExprNode)node).LeftOpnd.Accept(this);
            }
            // expr has unary operator
            else if (node.GetType() == typeof(UExprNode))
            {
                UExprNode currentNode = (UExprNode)node;
                bool value = (bool)currentNode.LeftOpnd.Accept(this);
                if (currentNode.UnOp.Token.Type == TokenType.NOT)
                {
                    return !value;
                }
                else throw new Exception("Unexpected unOp type");
            }
            // expr has two operands
            else if (node.GetType() == typeof(LRExprNode))
            {
                LRExprNode currentNode = (LRExprNode)node;
                object leftValue = currentNode.LeftOpnd.Accept(this);
                object rightValue = currentNode.RightOpnd.Accept(this);

                switch (node.Type)
                {
                    case "int":
                        switch (currentNode.Op.Token.Value)
                        {
                            case "+":
                                return ToInt(leftValue) + ToInt(rightValue);
                            case "-":
                                return ToInt(leftValue) - ToInt(rightValue);
                            case "*":
                                return ToInt(leftValue) * ToInt(rightValue);
                            case "/":
                                if (ToInt(rightValue) == 0)
                                {
                                    throw new RuntimeError(ErrorMessage.RE_DIVISION_BY_ZERO, currentNode.Op.Token.Pos);
                                }
                                return ToInt(leftValue) / ToInt(rightValue);
                            case "=":
                                return ToInt(leftValue) == ToInt(rightValue);
                            case "<":
                                return ToInt(leftValue) < ToInt(rightValue);
                            case ">":
                                return ToInt(leftValue) > ToInt(rightValue);
                            default:
                                throw new Exception("Unexpected operator");
                        }
                    case "string":
                        switch (currentNode.Op.Token.Value)
                        {
                            case "+":
                                return (string)leftValue + (string)rightValue;
                            case "=":
                                return (string)leftValue == (string)rightValue;
                            default:
                                throw new Exception("Unexpected operator");
                        }
                    case "bool":
                        switch (currentNode.Op.Token.Value)
                        {
                            case "=":
                                return (bool)leftValue == (bool)rightValue;
                            case "&":
                                return (bool)leftValue && (bool)rightValue;
                            default:
                                throw new Exception("Unexpected operator");
                        }
                    default:
                        throw new Exception("Unexpected type");
                }
            }
            else throw new Exception("Unexpected ExprNode children");
        }
        public object Visit(OpndNode node)
        {
            return node.Child.Accept(this);
        }
        // Visits token node and gets its value
        public object Visit(TokenNode node)
        {
            if (node.GetType() == typeof(IdentNode))
            {
                object? value = Context.GetVariableValue(node.Token.Value);
                if (value != null)
                {
                    return value;
                }
                else throw new RuntimeError($"{ErrorMessage.RE_UNINITIALIZED_VAR} {node.Token.Value}", node.Token.Pos);
            }
            else if (node.GetType() == typeof(StrNode))
            {
                string result = node.Token.Value;
                return result[1..^1];
            }
            return node.Token.Value;
        }
        // Casts object (int/str) to integer
        private static int ToInt(object number)
        {
            if (number.GetType() == typeof(int)) return (int)number;
            return int.Parse((string)number);
        }
    }
}
