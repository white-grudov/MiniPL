namespace MiniPL
{
    internal class Interpreter : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;
        public Interpreter(AST ast)
        {
            Context = Context.GetInstance();
            Ast = ast;
        }
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

        public void Visit(DeclNode node)
        {
            if (node.Expr == null) return;
            string name = node.Ident.Token.Value;
            object value = node.Expr.Accept(this);
            Context.Assign(name, value);
        }

        public void Visit(AssignNode node)
        {
            string name = node.Ident.Token.Value;
            object value = node.Expr.Accept(this);
            Context.Assign(name, value);
        }

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

        public void Visit(PrintNode node)
        {
            object expr = node.Expr.Accept(this);
            Console.Write(expr);
        }

        public void Visit(ReadNode node)
        {
            string name = node.Ident.Token.Value;
            string? input = Console.ReadLine();
            string type = Context.GetVariableType(name);
            if (type == "int")
            {
                if (!int.TryParse(input, out _))
                {
                    throw new RuntimeError("Unable to cast input to int", node.Ident.Token.Pos);
                }
            }

            string value = input ?? "";
            Context.Assign(name, value);
        }

        public object Visit(ExprNode node)
        {
            // expr has only one operand
            if (node.UnOp == null && node.Op == null)
            {
                return node.LeftOpnd.Accept(this);
            }
            // expr has unary operator
            else if (node.UnOp != null && node.Op == null)
            {
                bool value = (bool)node.LeftOpnd.Accept(this);
                if (node.UnOp.Token.Type == TokenType.NOT)
                {
                    return !value;
                }
                else throw new Exception("Unexpected unOp type");
            }
            // expr has two operands
            else if (node.UnOp == null && node.Op != null && node.RightOpnd != null)
            {
                object leftValue = node.LeftOpnd.Accept(this);
                object rightValue = node.RightOpnd.Accept(this);

                return node.Type switch
                {
                    "int" => node.Op.Token.Value switch
                    {
                        "+" => ToInt(leftValue) + ToInt(rightValue),
                        "-" => ToInt(leftValue) - ToInt(rightValue),
                        "*" => ToInt(leftValue) * ToInt(rightValue),
                        "/" => ToInt(leftValue) / ToInt(rightValue),
                        "=" => ToInt(leftValue) == ToInt(rightValue),
                        "<" => ToInt(leftValue) < ToInt(rightValue),
                        ">" => ToInt(leftValue) > ToInt(rightValue),
                        _ => throw new Exception("Unexpected operator")
                    },
                    "string" => node.Op.Token.Value switch
                    {
                        "+" => (string)leftValue + (string)rightValue,
                        "=" => (string)leftValue == (string)rightValue,
                        _ => throw new Exception("Unexpected operator")
                    },
                    "bool" => node.Op.Token.Value switch
                    {
                        "=" => (bool)leftValue == (bool)rightValue,
                        "&" => (bool)leftValue && (bool)rightValue,
                        _ => throw new Exception("Unexpected operator")
                    },
                    _ => throw new Exception("Unexpected type")
                };
            }
            else throw new Exception("Unexpected ExprNode children");
        }

        public object Visit(OpndNode node)
        {
            return node.Child.Accept(this);
        }

        public object Visit(TokenNode node)
        {
            if (node.GetType() == typeof(IdentNode))
            {
                object? value = Context.GetVariableValue(node.Token.Value);
                if (value != null)
                {
                    return value;
                }
                else throw new RuntimeError($"Usage of uninitialized variable {node.Token.Value}", node.Token.Pos);
            }
            else if (node.GetType() == typeof(StrNode))
            {
                string result = node.Token.Value;
                return result[1..^1];
            }
            return node.Token.Value;
        }
        private static int ToInt(object number)
        {
            if (number.GetType() == typeof(int)) return (int)number;
            return int.Parse((string)number);
        }
    }
}
