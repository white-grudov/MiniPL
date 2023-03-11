namespace MiniPL
{
    /* Semantic Analyzer is the part of the MiniPL interpreter which uses the Visitor pattern
     * to check the semantic correctness of the program, in particular variables declaration
     * and usage, and type matching.
     */
    public class SemanticAnalyzer : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;

        // Dictionary of the allowed variable types for all the operators
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
        private readonly List<string> boolOperators = new() { TFS(TokenType.EQ), TFS(TokenType.LT), TFS(TokenType.GT) };

        // list of exceptions for the statement mode recovery
        private readonly List<MiniPLException> exceptions = new();

        public SemanticAnalyzer(AST ast)
        {
            Ast = ast;
            Context = Context.GetInstance();
        }
        /* The main method of the class, which takes the root node of AST and executes the
         * semantic check of all the child nodes
         */
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
        /* Visits DeclNode and checks if the variable is not dclared yet and matches the types
         * of the variable and the expression
         */
        public void Visit(DeclNode node)
        {
            string name = node.Ident.Token.Value;
            string type = node.Type.Token.Value;

            if (Context.ContainsVariable(name))
            {
                exceptions.Add(new SemanticError(ErrorMessage.SE_VAR_DECLARED, node.Ident.Token.Pos));
            }
            Context.Declare(name, type);
            if (node.Expr == null) return;

            string exprType = (string)node.Expr.Accept(this);
            MatchTypes(exprType, type, node.Expr.Pos);
        }
        /* Visits the AssignNode and checks if the variable is already declared and 
         * matches the types of the variable and the expression
         */
        public void Visit(AssignNode node)
        {
            string name = node.Ident.Token.Value;
            CheckVariableDeclared(name, node.Ident.Token.Pos);

            string exprType = (string)node.Expr.Accept(this);
            string varType = Context.GetVariableType(name);

            MatchTypes(exprType, varType, node.Expr.Pos);
        }
        /* Visits the ForNode and checks if the values in range are both of int type,
         * control variable is declared and of int type, and visits nested statements
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
        /* Visits IfNode and checks if the condition expression is of bool type and
         * visits nested statements
         */
        public void Visit(IfNode node)
        {
            string desiredType = TFS(TokenType.BOOL);
            string condType = (string)node.Expr.Accept(this);
            MatchTypes(condType, desiredType, node.Expr.Pos);
                                                                     
            node.IfStmts.Accept(this);
            node.ElseStmts?.Accept(this);
        }
        // Visits expression in PrintNode
        public void Visit(PrintNode node)
        {
            node.Expr.Accept(this);
        }
        // Checks if variable in ReadNode is declared
        public void Visit(ReadNode node)
        {
            string name = node.Ident.Token.Value;
            CheckVariableDeclared(name, node.Ident.Token.Pos);
        }
        // Visits ExprNode and returns the type of the expression
        public object Visit(ExprNode node)
        {
            // Expression has only operand
            if (node.GetType() == typeof(LExprNode))
            {
                string type = (string)((LExprNode)node).LeftOpnd.Accept(this);
                node.Type = type;
                return type;
            }
            // Expression has unary operator, checks if the operand type is bool
            else if (node.GetType() == typeof(UExprNode))
            {
                UExprNode currentNode = (UExprNode)node;

                string desiredOpndType = TFS(TokenType.BOOL);
                string opndType = (string)currentNode.LeftOpnd.Accept(this);
                MatchTypes(desiredOpndType, opndType, currentNode.LeftOpnd.Pos);

                node.Type = opndType;
                return opndType;
            }
            // Expression has two operands, checks if operands' types match and operator accepts them
            else if (node.GetType() == typeof(LRExprNode))
            {
                LRExprNode currentNode = (LRExprNode)node;

                string leftOpndType = (string)currentNode.LeftOpnd.Accept(this);
                string rightOpndType = (string)currentNode.RightOpnd.Accept(this);
                MatchTypes(leftOpndType, rightOpndType, currentNode.LeftOpnd.Pos);

                string opType = (string)currentNode.Op.Accept(this);
                if (!allowedTypes[opType].Contains(leftOpndType))
                {
                    exceptions.Add(new SemanticError(ErrorMessage.SE_VAR_TYPE_DISMATCH, currentNode.Op.Token.Pos));
                }

                node.Type = leftOpndType;
                if (boolOperators.Contains(opType)) return TFS(TokenType.BOOL);
                return leftOpndType;
            }
            else throw new Exception("Unexpected ExprNode children");
        }
        // Visits OpndNode and visits neither expression or token node
        public object Visit(OpndNode node)
        {
            return node.Child.Accept(this);
        }
        // Returns the type of TokenNode
        public object Visit(TokenNode node)
        {
            if (node.Token.Type == TokenType.IDENTIFIER)
            {
                CheckVariableDeclared(node.Token.Value, node.Token.Pos);
                return Context.GetVariableType(node.Token.Value);
            }
            return TFS(node.Token.Type);
        }
        // Returns unified representation of the token type
        private static string TFS(TokenType type)
        {
            return TokenTypeExtenstions.ToFriendlyString(type);
        }
        // Checks if the variable is already declared
        private void CheckVariableDeclared(string name, Position pos)
        {
            if (!Context.ContainsVariable(name))
            {
                exceptions.Add(new SemanticError(ErrorMessage.SE_VAR_NOT_DECLARED, pos));
            }
        }
        // Checks if the two types are the same
        private void MatchTypes(string exprType, string desiredType, Position pos)
        {
            if (exprType == null || desiredType == null) return;
            if (exprType != desiredType)
            {
                exceptions.Add(
                    new SemanticError($"{ErrorMessage.SE_VAR_TYPE_DISMATCH} (expected {desiredType}, got {exprType})", pos));
            }
        }
    }
}
