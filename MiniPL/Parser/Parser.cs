namespace MiniPL
{
    class AST
    {
        public ProgNode Root;
        public AST()
        {
            Root = new ProgNode();
        }
    }
    internal class Parser
    {
        public AST Ast { get; }
        public Scanner Scanner { get; }

        private Token currentToken;

        private bool insideForStmt = false;
        private bool insideIfStmt = false;

        private readonly List<TokenType> opndTypes = new()
        {
            TokenType.INT_LITERAL, TokenType.STRING_LITERAL, TokenType.IDENTIFIER, TokenType.LPAREN
        };
        private readonly List<TokenType> opTypes = new()
        {
            TokenType.PLUS, TokenType.MINUS, TokenType.DIV, TokenType.MUL,
            TokenType.EQ, TokenType.LT, TokenType.GT, TokenType.AND
        };
        private readonly List<object> expectedDeclTokens = new()
        {
            TokenType.VAR, TokenType.IDENTIFIER, TokenType.COLON,
            new List<TokenType> { TokenType.INT, TokenType.STRING, TokenType.BOOL }
        };
        private readonly List<TokenType> expectedAssignTokens = new()
        {
            TokenType.IDENTIFIER, TokenType.ASSIGN
        };
        private readonly List<TokenType> expectedForTokens = new()
        {
            TokenType.FOR, TokenType.IDENTIFIER, TokenType.IN
        };
        private readonly List<TokenType> opNodes = new()
        {
            TokenType.PLUS, TokenType.MINUS, TokenType.DIV, TokenType.MUL,
            TokenType.EQ, TokenType.LT, TokenType.GT, TokenType.AND
        };
        private readonly List<TokenType> typeNodes = new()
        {
            TokenType.INT, TokenType.STRING, TokenType.BOOL
        };
        public Parser(string filename, bool debugMode)
        {
            Ast = new();
            Scanner = new(filename, debugMode);
        }
        public void Parse()
        {
            Scanner.Tokenize();
            Ast.Root.AddStmts(AddStmtsNode());
        }
        private StmtsNode AddStmtsNode()
        {
            StmtsNode stmts = new();

            // inside if or for statement
            if (insideForStmt || insideIfStmt)
            {
                while (Lookahead().Type != TokenType.END)
                {
                    if (insideIfStmt && Lookahead().Type == TokenType.ELSE)
                        return stmts;
                    stmts.AddChild(AddStmtNode());
                    NextToken();
                }
                NextToken();
                ExpectToken(insideForStmt ? TokenType.FOR : TokenType.IF);
                return stmts;
            }
            while (!IsAtEnd())
            {
                stmts.AddChild(AddStmtNode());
                ExpectToken(TokenType.SEMICOLON);
            }

            return stmts;
        }
        private StmtNode AddStmtNode()
        {
            StmtNode stmt;
            switch (Lookahead().Type)
            {
                case TokenType.VAR:
                    stmt = AddDeclStmt();
                    break;
                case TokenType.IDENTIFIER:
                    stmt = AddAssignStmt();
                    break;
                case TokenType.FOR:
                    stmt = AddForStmt();
                    break;
                case TokenType.IF:
                    stmt = AddIfStmt();
                    break;
                case TokenType.PRINT:
                    stmt = AddPrintStmt();
                    break;
                case TokenType.READ:
                    stmt = AddReadStmt();
                    break;
                default:
                    throw new SyntaxError($"Illegal token {currentToken.Type}", currentToken.Pos);            
            }

            return stmt;
        }
        private DeclNode AddDeclStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedDeclTokens)
            {
                INode? node = GetChild(type);
                if (node != null) childNodes.Add(node);
            }

            DeclNode declNode = new((IdentNode)childNodes[0], (TypeNode)childNodes[1]);

            if (Lookahead().Type != TokenType.SEMICOLON)
            {
                ExpectToken(TokenType.ASSIGN);
                declNode.AddExpr(AddExpr());
            }

            return declNode;
        }
        private AssignNode AddAssignStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedAssignTokens)
            {
                INode? node = GetChild(type);
                if (node != null) childNodes.Add(node);
            }
            return new AssignNode((IdentNode)childNodes[0], AddExpr());
        }
        private ForNode AddForStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedForTokens)
            {
                INode? node = GetChild(type);
                if (node != null) childNodes.Add(node);
            }
            childNodes.Add(AddExpr());
            ExpectToken(TokenType.DOUBLEDOT);
            childNodes.Add(AddExpr());
            ExpectToken(TokenType.DO);

            insideForStmt = true;
            ForNode forNode = new((IdentNode)childNodes[0], (ExprNode)childNodes[1], (ExprNode)childNodes[2], AddStmtsNode());
            insideForStmt = false;

            return forNode;
        }
        private IfNode AddIfStmt()
        {
            List<INode> childNodes = new();
            ExpectToken(TokenType.IF);
            childNodes.Add(AddExpr());

            ExpectToken(TokenType.DO);

            insideIfStmt = true;
            IfNode ifNode = new((ExprNode)childNodes[0], AddStmtsNode());

            if (Lookahead().Type != TokenType.SEMICOLON)
            {
                ExpectToken(TokenType.ELSE);
                ifNode.AddElseStmts(AddStmtsNode());
            }
            insideIfStmt = false;

            return ifNode;
        }
        private PrintNode AddPrintStmt()
        {
            ExpectToken(TokenType.PRINT);
            return new PrintNode(AddExpr());
        }
        private ReadNode AddReadStmt()
        {
            List<INode> childNodes = new();
            ExpectToken(TokenType.READ);
            INode? node = GetChild(TokenType.IDENTIFIER);
            if (node != null) childNodes.Add(node);

            return new ReadNode((IdentNode)childNodes[0]);
        }
        private ExprNode AddExpr()
        {
            List<INode> childNodes = new();
            bool unOp = false;

            if (Lookahead().Type == TokenType.NOT)
            {
                INode? node = GetChild(TokenType.NOT);
                if (node != null) childNodes.Add(node);
            }
            ExpectToken(opndTypes);
            childNodes.Add(AddOpndNode());

            ExprNode exprNode = new((OpndNode)childNodes[childNodes.Count - 1], currentToken.Pos);
            if (unOp)
            {
                exprNode.AddUnOp((UnOpNode)childNodes[0]);
            }
            else if (opTypes.Contains(Lookahead().Type))
            {
                childNodes.Clear();
                INode? node = GetChild(opTypes);
                if (node != null) childNodes.Add(node);

                ExpectToken(opndTypes);
                childNodes.Add(AddOpndNode());

                exprNode.AddRightOpnd((OpNode)childNodes[0], (OpndNode)childNodes[1]);
            }
            return exprNode;
        }
        private OpndNode AddOpndNode()
        {
            List<INode> childNodes = new();
            if (currentToken.Type == TokenType.LPAREN)
            {
                childNodes.Add(AddExpr());
                ExpectToken(TokenType.RPAREN);
            }
            else
            {
                INode? node = GetNode(currentToken);
                if (node != null) childNodes.Add(node);
            }
            OpndNode opndNode = new OpndNode((OpndNodeChild)childNodes[0]);

            return opndNode;
        }
        private INode? GetChild(object type)
        {
            ExpectToken(type);
            return GetNode(currentToken);
        }
        private INode? GetNode(Token token)
        {
            switch (token.Type)
            {
                case TokenType.IDENTIFIER:
                    return new IdentNode(token);
                case var _ when typeNodes.Contains(token.Type):
                    return new TypeNode(token);
                case TokenType.INT_LITERAL:
                    return new IntNode(token);
                case TokenType.STRING_LITERAL:
                    return new StrNode(token);
                case var _ when opNodes.Contains(token.Type):
                    return new OpNode(token);
                case TokenType.NOT:
                    return new UnOpNode(token);
                default:
                    return null;
            }
        }
        private void NextToken()
        {
            Scanner.Tokenize();
            currentToken = !IsAtEnd() ? Scanner.CurrentToken : new Token(TokenType.ILLEGAL, "", new Position(-1, -1));
        }
        private void ExpectToken(object type)
        {
            if (type.GetType() == typeof(TokenType))
            {
                if (Lookahead().Type != (TokenType)type)
                {
                    throw new SyntaxError($"Unexpected token {Lookahead().Type} (expected {type})", Lookahead().Pos);
                }
            }
            else if (type.GetType() == typeof(List<TokenType>))
            {
                if (!((List<TokenType>)type).Contains(Lookahead().Type))
                {
                    throw new SyntaxError($"Unexpected token {Lookahead().Type}", Lookahead().Pos);
                }
            }
            else
            {
                throw new InvalidCastException("Unallowed token type");
            }
            NextToken();
        }
        private Token Lookahead()
        {
            return !IsAtEnd() ? Scanner.NextToken : new Token(TokenType.ILLEGAL, "", new Position(-1, -1));
        }
        private bool IsAtEnd()
        {
            return Scanner.NextToken.Type == TokenType.EOF;
        }
    }
}
