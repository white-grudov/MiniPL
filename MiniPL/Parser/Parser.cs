using System.ComponentModel.Design;

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

        private bool debugMode = false;

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

        private List<MiniPLException> exceptions = new();

        public Parser(string filename, bool debugMode)
        {
            this.debugMode = debugMode;

            Ast = new();
            Scanner = new(filename, debugMode);
        }
        public void Parse()
        {
            Scanner.Tokenize();
            Ast.Root.AddStmts(AddStmtsNode());

            if (debugMode) Ast.Root.Print();

            if (exceptions.Count > 0) throw new ErrorList(exceptions);
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
                    CheckStmt(ref stmts);
                }
                NextToken();
                ExpectToken(insideForStmt ? TokenType.FOR : TokenType.IF);
                return stmts;
            }
            while (!IsAtEnd())
            {
                CheckStmt(ref stmts);
            }
            return stmts;
        }
        private void CheckStmt(ref StmtsNode stmts)
        {
            StmtNode? stmt = AddStmtNode();
            if (stmt != null)
            {
                stmts.AddChild(stmt);
                CheckSemicolon();
            }
        }
        private StmtNode? AddStmtNode()
        {
            StmtNode? stmt = null;
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
                    ErrorToken(new SyntaxError($"Illegal token {Lookahead().Type}", Lookahead().Pos));
                    break;
            }
            return stmt;
        }
        private DeclNode? AddDeclStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedDeclTokens)
            {
                INode? node = GetChild(type);
                if (node != null)
                {
                    if (node.GetType() == typeof(ErrorNode)) return null;
                    childNodes.Add(node);
                }
            }

            DeclNode declNode = new((IdentNode)childNodes[0], (TypeNode)childNodes[1]);

            if (Lookahead().Type == TokenType.ASSIGN)
            {
                ExpectToken(TokenType.ASSIGN);
                ExprNode? expr = AddExpr();
                if (expr == null) return null;
                declNode.AddExpr(expr);
            }

            return declNode;
        }
        private AssignNode? AddAssignStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedAssignTokens)
            {
                INode? node = GetChild(type);
                if (node != null)
                {
                    if (node.GetType() == typeof(ErrorNode)) return null;
                    childNodes.Add(node);
                }
            }
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            return new AssignNode((IdentNode)childNodes[0], expr);
        }
        private ForNode? AddForStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedForTokens)
            {
                INode? node = GetChild(type);
                if (node != null)
                {
                    if (node.GetType() == typeof(ErrorNode)) return null;
                    childNodes.Add(node);
                }
            }
            ExprNode? start = AddExpr();
            if (start == null) return null;
            childNodes.Add(start);

            if (!ExpectToken(TokenType.DOUBLEDOT)) return null;

            ExprNode? end = AddExpr();
            if (end == null) return null;
            childNodes.Add(end);

            if (!ExpectToken(TokenType.DO)) return null;

            insideForStmt = true;
            ForNode forNode = new((IdentNode)childNodes[0], (ExprNode)childNodes[1], (ExprNode)childNodes[2], AddStmtsNode());
            insideForStmt = false;

            return forNode;
        }
        private IfNode? AddIfStmt()
        {
            List<INode> childNodes = new();
            if (!ExpectToken(TokenType.IF)) return null;
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            childNodes.Add(expr);

            if (!ExpectToken(TokenType.DO)) return null;

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
        private PrintNode? AddPrintStmt()
        {
            if (!ExpectToken(TokenType.PRINT)) return null;
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            return new PrintNode(expr);
        }
        private ReadNode? AddReadStmt()
        {
            List<INode> childNodes = new();
            if (!ExpectToken(TokenType.READ)) return null;
            INode? node = GetChild(TokenType.IDENTIFIER);
            if (node != null)
            {
                if (node.GetType() == typeof(ErrorNode)) return null;
                childNodes.Add(node);
            }

            return new ReadNode((IdentNode)childNodes[0]);
        }
        private ExprNode? AddExpr()
        {
            List<INode> childNodes = new();
            bool unOp = false;

            if (Lookahead().Type == TokenType.NOT)
            {
                INode? node = GetChild(TokenType.NOT);
                if (node != null)
                {
                    if (node.GetType() == typeof(ErrorNode)) return null;
                    childNodes.Add(node);
                }
            }
            if (!ExpectToken(opndTypes)) return null;

            OpndNode? firstOpnd = AddOpndNode();
            if (firstOpnd == null) return null;
            childNodes.Add(firstOpnd);

            ExprNode exprNode = new((OpndNode)childNodes[^1], currentToken.Pos);
            if (unOp)
            {
                exprNode.AddUnOp((UnOpNode)childNodes[0]);
            }
            else if (opTypes.Contains(Lookahead().Type))
            {
                childNodes.Clear();
                INode? node = GetChild(opTypes);
                if (node != null)
                {
                    if (node.GetType() == typeof(ErrorNode)) return null;
                    childNodes.Add(node);
                }

                if (!ExpectToken(opndTypes)) return null;

                OpndNode? secondOpnd = AddOpndNode();
                if (secondOpnd == null) return null;
                childNodes.Add(secondOpnd);

                exprNode.AddRightOpnd((OpNode)childNodes[0], (OpndNode)childNodes[1]);
            }
            return exprNode;
        }
        private OpndNode? AddOpndNode()
        {
            List<INode> childNodes = new();
            if (currentToken.Type == TokenType.LPAREN)
            {
                ExprNode? expr = AddExpr();
                if (expr == null) return null;
                childNodes.Add(expr);
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
            if (!ExpectToken(type)) return new ErrorNode();
            return GetNode(currentToken);
        }
        private INode? GetNode(Token token)
        {
            return token.Type switch
            {
                TokenType.IDENTIFIER                      => new IdentNode(token),
                var _ when typeNodes.Contains(token.Type) => new TypeNode(token),
                TokenType.INT_LITERAL                     => new IntNode(token),
                TokenType.STRING_LITERAL                  => new StrNode(token),
                var _ when opNodes.Contains(token.Type)   => new OpNode(token),
                TokenType.NOT                             => new UnOpNode(token),
                _                                         => null,
            };
        }
        private void NextToken()
        {
            Scanner.Tokenize();
            currentToken = !IsAtEnd() ? Scanner.CurrentToken : new Token(TokenType.ILLEGAL, "", new Position(-1, -1));
        }
        private void ErrorToken(SyntaxError error)
        {
            exceptions.Add(error);
            do
            {
                NextToken();
                if (IsAtEnd())
                {
                    if (Scanner.CurrentToken.Type != TokenType.SEMICOLON)
                        exceptions.Add(new SyntaxError("Missing semicolon", Scanner.CurrentToken.Pos));
                    throw new ErrorList(exceptions);
                }
            }
            while (currentToken.Type != TokenType.SEMICOLON);
        }
        private bool ExpectToken(object type)
        {
            if (type.GetType() == typeof(TokenType))
            {
                if (Lookahead().Type != (TokenType)type)
                {
                    ErrorToken(new SyntaxError($"Unexpected token {Lookahead().Type} (expected {type})", Lookahead().Pos));
                    return false;
                }
            }
            else if (type.GetType() == typeof(List<TokenType>))
            {
                if (!((List<TokenType>)type).Contains(Lookahead().Type))
                {
                    ErrorToken(new SyntaxError($"Unexpected token {Lookahead().Type}", Lookahead().Pos));
                    return false;
                }
            }
            else
            {
                throw new InvalidCastException("Unallowed token type");
            }
            NextToken();
            return true;
        }
        private void CheckSemicolon()
        {
            if (Lookahead().Type != TokenType.SEMICOLON)
            {
                exceptions.Add(new SyntaxError("Missing semicolon", currentToken.Pos));
            }
            else NextToken();
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
