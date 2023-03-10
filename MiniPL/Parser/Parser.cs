namespace MiniPL
{
    // Wrapper class for the root Program Node
    public class AST
    {
        public ProgNode Root;
        public AST()
        {
            Root = new ProgNode();
        }
    }
    /* Parser is the part of the MiniPL interpreter which is responsible for taking tokens generated
     * by the scanner and creating an abstract syntax tree. When the method Parse() is called, it starts
     * generating tree nodes up-to-down. 
     */
    public class Parser
    {
        public AST Ast { get; }
        public Scanner Scanner { get; }

        private Token currentToken;

        private readonly bool debugMode = false;

        // Helper lists for detecting which tokens should go next
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

        // Helper lists for determening which node should be created based on token type
        private readonly List<TokenType> opNodes = new()
        {
            TokenType.PLUS, TokenType.MINUS, TokenType.DIV, TokenType.MUL,
            TokenType.EQ, TokenType.LT, TokenType.GT, TokenType.AND
        };
        private readonly List<TokenType> typeNodes = new()
        {
            TokenType.INT, TokenType.STRING, TokenType.BOOL
        };

        // List of exceptions for statement mode recovery
        private readonly List<MiniPLException> exceptions = new();

        public Parser(string filename, bool debugMode)
        {
            this.debugMode = debugMode;

            Ast = new();
            Scanner = new(filename, debugMode);
        }

        // Main method of the class, which generates AST
        public void Parse()
        {
            Scanner.Tokenize();
            Ast.Root.AddStmts(AddStmtsNode());

            if (debugMode) Ast.Root.Print();

            if (exceptions.Count > 0) throw new ErrorList(exceptions);
        }

        private enum Inside
        { 
            NONE, IF, FOR
        }
        // Generates Stmts nodes either for Prog node or for If and For nodes
        private StmtsNode AddStmtsNode(Inside i = Inside.NONE)
        {
            StmtsNode stmts = new();

            // inside if or for statement
            if (i == Inside.IF || i == Inside.FOR)
            {
                while (Lookahead().Type != TokenType.END)
                {
                    if ((i == Inside.IF) && Lookahead().Type == TokenType.ELSE)
                        return stmts;
                    CheckStmt(ref stmts);
                }
                NextToken();
                ExpectToken((i == Inside.FOR) ? TokenType.FOR : TokenType.IF);
                return stmts;
            }
            while (!IsAtEnd())
            {
                CheckStmt(ref stmts);
            }
            return stmts;
        }
        // Adds StmtNode to statements and checks if is it closed by semicolon
        private void CheckStmt(ref StmtsNode stmts)
        {
            StmtNode? stmt = AddStmtNode();
            if (stmt != null)
            {
                stmts.AddChild(stmt);
                CheckSemicolon();
            }
        }
        /* Generates new StmtNode based on the first token in statement. If token does not match, error is
         * added to the list and next statement is generated.
         */
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
                    ErrorToken(new SyntaxError($"{ErrorMessage.SE_ILLEGAL_TOKEN} {Lookahead().Type}", Lookahead().Pos));
                    break;
            }
            return stmt;
        }
        // Generates declaration statement and checks if the value is assigned right after declaration
        private DeclNode? AddDeclStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedDeclTokens)
            {
                if (!ProcessChild(type, ref childNodes)) return null;
            }

            DeclNode declNode = new((IdentNode)childNodes[0], (TypeNode)childNodes[1]);

            if (Lookahead().Type == TokenType.ASSIGN)
            {
                if (!ExpectToken(TokenType.ASSIGN)) return null;
                ExprNode? expr = AddExpr();
                if (expr == null) return null;
                declNode.AddExpr(expr);
            }

            return declNode;
        }
        // Generates assignment statement
        private AssignNode? AddAssignStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedAssignTokens)
            {
                if (!ProcessChild(type, ref childNodes)) return null;
            }
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            return new AssignNode((IdentNode)childNodes[0], expr);
        }
        // Generates for loop statement with statements inside it
        private ForNode? AddForStmt()
        {
            List<INode> childNodes = new();
            foreach (var type in expectedForTokens)
            {
                if (!ProcessChild(type, ref childNodes)) return null;
            }
            ExprNode? start = AddExpr();
            if (start == null) return null;
            childNodes.Add(start);

            if (!ExpectToken(TokenType.RANGE)) return null;

            ExprNode? end = AddExpr();
            if (end == null) return null;
            childNodes.Add(end);

            if (!ExpectToken(TokenType.DO)) return null;

            ForNode forNode = new(
                (IdentNode)childNodes[0], (ExprNode)childNodes[1], (ExprNode)childNodes[2], AddStmtsNode(Inside.FOR));

            return forNode;
        }
        // Generates if condition statement with statements inside it
        private IfNode? AddIfStmt()
        {
            List<INode> childNodes = new();
            if (!ExpectToken(TokenType.IF)) return null;
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            childNodes.Add(expr);

            if (!ExpectToken(TokenType.DO)) return null;

            IfNode ifNode = new((ExprNode)childNodes[0], AddStmtsNode(Inside.IF));

            if (Lookahead().Type != TokenType.SEMICOLON)
            {
                if (!ExpectToken(TokenType.ELSE)) return null;
                ifNode.AddElseStmts(AddStmtsNode(Inside.IF));
            }

            return ifNode;
        }
        // Generates print statement
        private PrintNode? AddPrintStmt()
        {
            if (!ExpectToken(TokenType.PRINT)) return null;
            ExprNode? expr = AddExpr();
            if (expr == null) return null;
            return new PrintNode(expr);
        }
        // Generates read statement
        private ReadNode? AddReadStmt()
        {
            List<INode> childNodes = new();
            if (!ExpectToken(TokenType.READ)) return null;
            if (!ProcessChild(TokenType.IDENTIFIER, ref childNodes)) return null;

            return new ReadNode((IdentNode)childNodes[0]);
        }
        /* Generates expression statement and checks whether it has an unary or binary operator, or 
         * just an operand
         */
        private ExprNode? AddExpr()
        {
            List<INode> childNodes = new();
            bool unOp = false;

            // Checks if there is unary operator NOT
            if (Lookahead().Type == TokenType.NOT)
            {
                if (!ProcessChild(TokenType.NOT, ref childNodes)) return null;
            }
            if (!ExpectToken(opndTypes)) return null;

            OpndNode? firstOpnd = AddOpndNode();
            if (firstOpnd == null) return null;
            childNodes.Add(firstOpnd);

            // If unary operator, return unary expression statement node
            if (unOp)
            {
                return new UExprNode((UnOpNode)childNodes[0], (OpndNode)childNodes[1], currentToken.Pos);
            }
            // If binary operator, return expression node with left and right operands, and operator
            else if (opTypes.Contains(Lookahead().Type))
            {
                if (!ProcessChild(opTypes, ref childNodes)) return null;
                if (!ExpectToken(opndTypes)) return null;

                OpndNode? secondOpnd = AddOpndNode();
                if (secondOpnd == null) return null;
                childNodes.Add(secondOpnd);

                return new LRExprNode(
                    (OpndNode)childNodes[0], (OpNode)childNodes[1], (OpndNode)childNodes[2], currentToken.Pos);
            }
            // Otherwise return expression node with just an operand
            return new LExprNode((OpndNode)childNodes[0], currentToken.Pos);
        }
        // Generates operand node
        private OpndNode? AddOpndNode()
        {
            List<INode> childNodes = new();

            // If operand starts with bracket, generate expression
            if (currentToken.Type == TokenType.LPAREN)
            {
                ExprNode? expr = AddExpr();
                if (expr == null) return null;
                childNodes.Add(expr);
                if (!ExpectToken(TokenType.RPAREN)) return null;
            }
            else
            {
                INode? node = GetNode(currentToken);
                if (node != null) childNodes.Add(node);
            }
            OpndNode opndNode = new((OpndNodeChild)childNodes[0]);

            return opndNode;
        }
        /* Method is called when the parser tries to generate a new node based on its token type or check
         * the list of expected tokens. If the generated node is null, no action is performed. If the 
         * generated node is of ErrorNode type, method returns false, therefore the caller method immediately 
         * stops its execution and go to the next statement. This is done as a part of statement mode 
         * recovery error strategy.
         */
        private bool ProcessChild(object type, ref List<INode> childNodes)
        {
            INode? node = GetChild(type);
            if (node != null)
            {
                if (node.GetType() == typeof(ErrorNode)) return false;
                childNodes.Add(node);
            }
            return true;
        }
        /* Checks the current token if it matches the expected type or the list of types. If it does not,
         * the ErrorNode is returned. Otherwise, the new node generated based on the type of current token
         * is returned.
         */
        private INode? GetChild(object type)
        {
            if (type.GetType() == typeof(TokenType))
            {
                if (!ExpectToken((TokenType)type)) return new ErrorNode();
            }
            else if (type.GetType() == typeof(List<TokenType>))
            {
                if (!ExpectToken((List<TokenType>)type)) return new ErrorNode();
            }
            else return null;
            return GetNode(currentToken);
        }
        // Generates the node based on the token type
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
        // Asks for the next token from the scanner
        private void NextToken()
        {
            Scanner.Tokenize();
            currentToken = Scanner.CurrentToken.Type != TokenType.EOF
                ? Scanner.CurrentToken : new Token(TokenType.ILLEGAL, "", new Position(-1, -1));
        }
        /* New exception is added to the list and parser searches for the new statement by looking for the
         * token after the next semicolon. If semicolon is not found and it is EOF, new exception is added 
         * and the parser stops the execution.
         */
        private void ErrorToken(SyntaxError error)
        {
            exceptions.Add(error);
            do
            {
                NextToken();
                if (IsAtEnd())
                {
                    if (Scanner.CurrentToken.Type != TokenType.SEMICOLON)
                        exceptions.Add(new SyntaxError(ErrorMessage.SE_MISSING_SEMICOLON, Scanner.CurrentToken.Pos));
                    throw new ErrorList(exceptions);
                }
            }
            while (currentToken.Type != TokenType.SEMICOLON);
        }
        /* Checks if the next token is of an expected type or in the list of expected types. If it is not,
         * the new exception is added to the list and the function returns false. When the methods responsible
         * for generating nodes see that ExpectToken() returned false, they immediately stop their execution and
         * go to the next statement. This is done as a part of statement mode recovery error strategy.
         */
        private bool ExpectToken(TokenType type)
        {
            if (Lookahead().Type != type)
            {
                ErrorToken(new SyntaxError(
                    $"{ErrorMessage.SE_UNEXPECTED_TOKEN} {Lookahead().Type} (expected {type})", Lookahead().Pos)
                    );
                return false;
            }
            NextToken();
            return true;
        }
        private bool ExpectToken(List<TokenType> types)
        {
            if (!types.Contains(Lookahead().Type))
            {
                ErrorToken(new SyntaxError($"{ErrorMessage.SE_UNEXPECTED_TOKEN} {Lookahead().Type}", Lookahead().Pos));
                return false;
            }
            NextToken();
            return true;
        }
        // Check if the token at the end of the statement is semicolon
        private void CheckSemicolon()
        {
            if (Lookahead().Type != TokenType.SEMICOLON)
            {
                exceptions.Add(new SyntaxError(ErrorMessage.SE_MISSING_SEMICOLON, currentToken.Pos));
            }
            else NextToken();
        }
        // Asks for the lookahead token from the scanner
        private Token Lookahead()
        {
            return !IsAtEnd() ? Scanner.NextToken : new Token(TokenType.ILLEGAL, "", new Position(-1, -1));
        }
        // Checks if there is an EOF token
        private bool IsAtEnd()
        {
            return Scanner.NextToken.Type == TokenType.EOF;
        }
    }
}
