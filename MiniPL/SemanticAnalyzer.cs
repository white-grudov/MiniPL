namespace MiniPL
{
    internal class SemanticAnalyzer : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;

        private Dictionary<string, List<string>> allowedTypes = new Dictionary<string, List<string>>
        {
            { TFS(TokenType.PLUS), new List<string>() { TFS(TokenType.INT), TFS(TokenType.STRING) } },
            { TFS(TokenType.MINUS), new List<string>() { TFS(TokenType.INT) } },
            { TFS(TokenType.DIV), new List<string>() { TFS(TokenType.INT) } },
            { TFS(TokenType.MUL), new List<string>() { TFS(TokenType.INT) } },

            { TFS(TokenType.EQ),
                new List<string>() { TFS(TokenType.INT), TFS(TokenType.STRING), TFS(TokenType.BOOL) } },
            { TFS(TokenType.LT), new List<string> { TFS(TokenType.INT), TFS(TokenType.BOOL) } },
            { TFS(TokenType.GT), new List<string> { TFS(TokenType.INT), TFS(TokenType.BOOL) } },
            { TFS(TokenType.AND), new List<string> { TFS(TokenType.BOOL) } }
        };
        private List<string> boolOperators = new List<string> { TFS(TokenType.EQ), TFS(TokenType.LT), TFS(TokenType.GT) };
        public SemanticAnalyzer(AST ast)
        {
            Ast = ast;
            Context = Context.GetInstance();
    }
        public void Analyze()
        {
            Ast.Root.Accept(this);
        }

        public object? Visit(ProgNode node)
        {
            node.Stmts?.Accept(this);
            return null;
        }

        public object? Visit(StmtsNode node)
        {
            foreach (var child in node.GetAllChildren())
            {
                child.Accept(this);
            }
            return null;
        }

        public object? Visit(DeclNode node)
        {
            string name = node.Ident.GetValue();
            string type = node.Type.GetValue();

            if (Context.ContainsVariable(name))
            {
                throw new SemanticError("Variable is already declared.", node.Ident.Token.Pos);
            }
            Context.Declare(name, type);
            if (node.Expr == null) return null;

            string exprType = (string)node.Expr.Accept(this);
            MatchTypes(exprType, type, node.Ident.Token.Pos);
            return null;
        }

        public object? Visit(AssignNode node)
        {
            string name = node.Ident.Token.Value;
            CheckVariableDeclared(name, node.Ident.Token.Pos);

            // add check for the type of variable and expr to be the same
            string exprType = (string)node.Expr.Accept(this);
            string varType = Context.GetVariableType(name);

            MatchTypes(exprType, varType, node.Expr.Pos);
            return null;
        }

        /* checks for "for" node:
         * - values in range are both int +
         * - ident is declared +
         * - ident value is int +
         * - first range value is less than second one (runtime error?)
         * - visit nested stmts +
         */
        public object? Visit(ForNode node)
        {
            string desiredType = TFS(TokenType.INT);
            string indexName = node.Ident.GetValue();

            CheckVariableDeclared(indexName, node.Ident.Token.Pos);

            string indexType = Context.GetVariableType(indexName);
            string lowerBoundType = (string)node.StartExpr.Accept(this);
            string upperBoundType = (string)node.EndExpr.Accept(this);

            MatchTypes(indexType, desiredType, node.Ident.Token.Pos);
            MatchTypes(lowerBoundType, desiredType, node.StartExpr.Pos);
            MatchTypes(upperBoundType, desiredType, node.EndExpr.Pos);

            node.Stmts.Accept(this);

            return null;
        }
        /* checks for "if" node:
         * - condition is bool +
         * - visit if (and else) stmts +
         */
        public object? Visit(IfNode node)
        {
            string desiredType = TFS(TokenType.BOOL);
            string condType = (string)node.Expr.Accept(this);
            MatchTypes(condType, desiredType, node.Expr.Pos);
                                                                     
            node.IfStmts.Accept(this);
            if (node.ElseStmts != null) node.ElseStmts.Accept(this);

            return null;
        }

        public object? Visit(PrintNode node)
        {
            node.Expr.Accept(this);
            return null;
        }

        public object? Visit(ReadNode node)
        {
            string name = node.Ident.GetValue();
            CheckVariableDeclared(name, node.Ident.Token.Pos);

            return null;
        }
        // visit exprnode should return type
        public object Visit(ExprNode node)
        {
            // expr has only one operand
            if (node.UnOp == null && node.Op == null)
            {
                string type = (string)node.LeftOpnd.Accept(this);
                node.Type = type;
                return type;
            }
            // expr has unary operator
            else if (node.UnOp != null && node.Op == null)
            {
                string desiredOpndType = TFS(TokenType.BOOL);
                string opndType = (string)node.LeftOpnd.Accept(this);
                MatchTypes(desiredOpndType, opndType, node.LeftOpnd.Pos);

                node.Type = opndType;
                return opndType;
            }
            // expr has two operands
            else if (node.UnOp == null && node.Op != null && node.RightOpnd != null)
            {
                string leftOpndType = (string)node.LeftOpnd.Accept(this);
                string rightOpndType = (string)node.RightOpnd.Accept(this);
                MatchTypes(leftOpndType, rightOpndType, node.LeftOpnd.Pos);

                string opType = (string)node.Op.Accept(this);
                if (!allowedTypes[opType].Contains(leftOpndType))
                {
                    throw new SemanticError("Variable type dismatch", node.Op.Token.Pos);
                }

                node.Type = leftOpndType;
                if (boolOperators.Contains(opType)) return TFS(TokenType.BOOL);
                return leftOpndType;
            }
            else throw new Exception("Unexpected ExprNode children");
        }

        public object Visit(OpndNode node)
        {
            return node.Child.Accept(this);
        }

        public object Visit(TokenNode node)
        {
            if (node.Token.Type == TokenType.IDENTIFIER)
                return Context.GetVariableType(node.Token.Value);
            return TFS(node.Token.Type);
        }
        private static string TFS(TokenType type)
        {
            return TokenTypeExtenstions.ToFriendlyString(type);
        }
        private void CheckVariableDeclared(string name, Position pos)
        {
            if (!Context.ContainsVariable(name))
            {
                throw new SemanticError("Variable is not declared.", pos);
            }
        }
        private void MatchTypes(string exprType, string desiredType, Position pos)
        {
            if (exprType != desiredType)
            {
                throw new SemanticError("Variable type dismatch", pos);
            }
        }
    }
}
