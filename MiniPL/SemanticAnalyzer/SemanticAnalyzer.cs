namespace MiniPL
{
    internal class SemanticAnalyzer : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;

        private readonly Dictionary<string, List<string>> allowedTypes = new()
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
        private List<string> boolOperators = new() { TFS(TokenType.EQ), TFS(TokenType.LT), TFS(TokenType.GT) };

        private List<MiniPLException> exceptions = new();

        public SemanticAnalyzer(AST ast)
        {
            Ast = ast;
            Context = Context.GetInstance();
        }
        public void Analyze()
        {
            Ast.Root.Accept(this);
            if (exceptions.Count > 0)
            {
                throw new ErrorList(exceptions);
            }
        }
        public void Visit(ProgNode node)
        {
            node.Stmts?.Accept(this);
        }
        public void Visit(StmtsNode node)
        {
            foreach (var child in node.GetAllChildren())
            {
                child.Accept(this);
            }
        }
        public void Visit(DeclNode node)
        {
            string name = node.Ident.Token.Value;
            string type = node.Type.Token.Value;

            if (Context.ContainsVariable(name))
            {
                throw new SemanticError("Variable is already declared.", node.Ident.Token.Pos);
            }
            Context.Declare(name, type);
            if (node.Expr == null) return;

            string exprType = (string)node.Expr.Accept(this);
            MatchTypes(exprType, type, node.Expr.Pos);
        }
        public void Visit(AssignNode node)
        {
            string name = node.Ident.Token.Value;
            CheckVariableDeclared(name, node.Ident.Token.Pos);

            // add check for the type of variable and expr to be the same
            string exprType = (string)node.Expr.Accept(this);
            string varType = Context.GetVariableType(name);

            MatchTypes(exprType, varType, node.Expr.Pos);
        }

        /* checks for "for" node:
         * - values in range are both int +
         * - ident is declared +
         * - ident value is int +
         * - first range value is less than second one (runtime error?)
         * - visit nested stmts +
         */
        public void Visit(ForNode node)
        {
            string desiredType = TFS(TokenType.INT);
            string indexName = node.Ident.Token.Value;

            CheckVariableDeclared(indexName, node.Ident.Token.Pos);

            string indexType = Context.GetVariableType(indexName);
            string lowerBoundType = (string)node.StartExpr.Accept(this);
            string upperBoundType = (string)node.EndExpr.Accept(this);

            MatchTypes(indexType, desiredType, node.Ident.Token.Pos);
            MatchTypes(lowerBoundType, desiredType, node.StartExpr.Pos);
            MatchTypes(upperBoundType, desiredType, node.EndExpr.Pos);

            node.Stmts.Accept(this);
        }
        /* checks for "if" node:
         * - condition is bool +
         * - visit if (and else) stmts +
         */
        public void Visit(IfNode node)
        {
            string desiredType = TFS(TokenType.BOOL);
            string condType = (string)node.Expr.Accept(this);
            MatchTypes(condType, desiredType, node.Expr.Pos);
                                                                     
            node.IfStmts.Accept(this);
            node.ElseStmts?.Accept(this);
        }
        public void Visit(PrintNode node)
        {
            node.Expr.Accept(this);
        }
        public void Visit(ReadNode node)
        {
            string name = node.Ident.Token.Value;
            CheckVariableDeclared(name, node.Ident.Token.Pos);
        }
        // visit exprnode should return type
        public object Visit(ExprNode node)
        {
            // expr has only one operand
            if (node.GetType() == typeof(LExprNode))
            {
                string type = (string)((LExprNode)node).LeftOpnd.Accept(this);
                node.Type = type;
                return type;
            }
            // expr has unary operator
            else if (node.GetType() == typeof(UExprNode))
            {
                UExprNode currentNode = (UExprNode)node;

                string desiredOpndType = TFS(TokenType.BOOL);
                string opndType = (string)currentNode.LeftOpnd.Accept(this);
                MatchTypes(desiredOpndType, opndType, currentNode.LeftOpnd.Pos);

                node.Type = opndType;
                return opndType;
            }
            // expr has two operands
            else if (node.GetType() == typeof(LRExprNode))
            {
                LRExprNode currentNode = (LRExprNode)node;

                string leftOpndType = (string)currentNode.LeftOpnd.Accept(this);
                string rightOpndType = (string)currentNode.RightOpnd.Accept(this);
                MatchTypes(leftOpndType, rightOpndType, currentNode.LeftOpnd.Pos);

                string opType = (string)currentNode.Op.Accept(this);
                if (!allowedTypes[opType].Contains(leftOpndType))
                {
                    exceptions.Add(new SemanticError("Variable type dismatch", currentNode.Op.Token.Pos));
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
            {
                CheckVariableDeclared(node.Token.Value, node.Token.Pos);
                return Context.GetVariableType(node.Token.Value);
            }
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
                exceptions.Add(new SemanticError("Variable is not declared", pos));
            }
        }
        private void MatchTypes(string exprType, string desiredType, Position pos)
        {
            if (exprType == null || desiredType == null) return;
            if (exprType != desiredType)
            {
                exceptions.Add(new SemanticError($"Variable type dismatch (expected {desiredType}, got {exprType})", pos));
            }
        }
    }
}
