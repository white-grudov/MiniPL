using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    class AST
    {
        public INode Root;
        public AST(Token token)
        {
            Root = new ProgNode(token);
        }
    }
    internal class Parser
    {
        public AST Ast { get; }
        public Scanner Scanner { get; }

        private Token dummyToken = new Token(TokenType.DUMMY, "", new Position(-1, -1));
        private Token currentToken;

        bool insideForStmt = false;
        bool insideIfStmt = false;

        private TokenType[] typeTypes =  { TokenType.INT, TokenType.STRING, TokenType.BOOL };
        private TokenType[] ignoreList = { TokenType.DO, TokenType.COLON, TokenType.IN, TokenType.SEMICOLON };
        private TokenType[] varTypes =   { TokenType.VAR, TokenType.IDENTIFIER, TokenType.COLON };
        private TokenType[] identTypes = { TokenType.IDENTIFIER, TokenType.ASSIGN };
        private TokenType[] forTypes =   { TokenType.FOR, TokenType.IDENTIFIER, TokenType.IN };
        private TokenType[] readTypes =  { TokenType.READ, TokenType.IDENTIFIER };
        private TokenType[] opndTypes =  { TokenType.INT_LITERAL, TokenType.STRING_LITERAL,
                                           TokenType.IDENTIFIER, TokenType.LPAREN };
        private TokenType[] opTypes =    { TokenType.PLUS, TokenType.MINUS, TokenType.DIV, TokenType.MUL,
                                           TokenType.EQ, TokenType.LT, TokenType.GT, TokenType.AND };

        public Parser(string filename)
        {
            Ast = new AST(dummyToken);
            Scanner = new Scanner(filename);
        }
        public void GenerateTokens()
        {
            Scanner.Tokenize();

            foreach (var token in Scanner.Tokens)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", token.Type, token.Value, token.Pos);
        }
        public void Parse()
        {
            Ast.Root.AddChild(AddStmtsNode());
        }
        private INode AddStmtsNode()
        {
            INode stmts = new StmtsNode(dummyToken);

            // inside if or for statement
            if (insideForStmt || insideIfStmt)
            {
                while (Lookahead().Type != TokenType.END)
                {
                    stmts.AddChild(AddStmtNode());
                    NextToken();
                }
                NextToken();
                ExpectToken(insideForStmt ? TokenType.FOR : TokenType.IF);
                NextToken();
                return stmts;
            }
            while (!IsAtEnd())
            {
                stmts.AddChild(AddStmtNode());
                NextToken();
            }

            return stmts;
        }
        private INode AddStmtNode()
        {
            INode stmt = new StmtNode(dummyToken);
            switch (Lookahead().Type)
            {
                case TokenType.VAR:
                    AddVarStmt(ref stmt);
                    break;
                case TokenType.IDENTIFIER:
                    AddIdentStmt(ref stmt);
                    break;
                case TokenType.FOR:
                    AddForStmt(ref stmt);
                    break;
                case TokenType.IF:
                    AddIfStmt(ref stmt);
                    break;
                case TokenType.PRINT:
                    AddPrintStmt(ref stmt);
                    break;
                case TokenType.READ:
                    AddReadStmt(ref stmt);
                    break;
                default:
                    throw new SyntaxError($"Illegal token {currentToken.Type}", currentToken.Pos);            
            }

            return stmt;
        }
        private void AddVarStmt(ref INode stmt)
        {
            foreach (var type in varTypes)
                AddChildToStmt(ref stmt, type);
            AddChildToStmt(ref stmt, typeTypes);

            if (Lookahead().Type == TokenType.SEMICOLON)
            {
                return;
            }
            AddChildToStmt(ref stmt, TokenType.ASSIGN);
            stmt.AddChild(AddExpr());

        }
        private void AddIdentStmt(ref INode stmt)
        {
            foreach (var type in identTypes)
                AddChildToStmt(ref stmt, type);
            stmt.AddChild(AddExpr());

        }
        private void AddForStmt(ref INode stmt)
        {
            foreach (var type in forTypes)
                AddChildToStmt(ref stmt, type);
            stmt.AddChild(AddExpr());

            AddChildToStmt(ref stmt, TokenType.DOUBLEDOT);
            stmt.AddChild(AddExpr());

            AddChildToStmt(ref stmt, TokenType.DO);

            insideForStmt = true;
            INode stmts = AddStmtsNode();
            insideForStmt = false;
            stmt.AddChild(stmts);
        }
        private void AddIfStmt(ref INode stmt)
        {
            AddChildToStmt(ref stmt, TokenType.IF);
            stmt.AddChild(AddExpr());

            AddChildToStmt(ref stmt, TokenType.DO);

            insideIfStmt = true;
            INode stmts = AddStmtsNode();
            insideIfStmt = false;
            stmt.AddChild(stmts);
        }
        private void AddPrintStmt(ref INode stmt)
        {
            AddChildToStmt(ref stmt, TokenType.PRINT);
            stmt.AddChild(AddExpr());
        }
        private void AddReadStmt(ref INode stmt)
        {
            foreach (var type in readTypes)
                AddChildToStmt(ref stmt, type);
        }
        private INode AddExpr()
        {
            bool unOp = false;
            INode expr = new ExprNode(dummyToken);

            if (Lookahead().Type == TokenType.NOT)
            {
                unOp = true;
                NextToken();
                expr.AddChild(currentToken);
            }
            ExpectToken(opndTypes);
            expr.AddChild(AddOpndNode());

            if (unOp || !opTypes.Contains(Lookahead().Type)) return expr;

            ExpectToken(opTypes);
            expr.AddChild(currentToken);

            ExpectToken(opndTypes);
            expr.AddChild(AddOpndNode());

            return expr;
        }
        private INode AddOpndNode()
        {
            INode opndNode = new OpndNode(dummyToken);
            if (currentToken.Type == TokenType.LPAREN)
            {
                opndNode.AddChild(AddExpr());
                ExpectToken(TokenType.RPAREN);
            }
            else opndNode.AddChild(currentToken);
            return opndNode;
        }
        private void AddChildToStmt(ref INode stmt, TokenType type)
        {
            ExpectToken(type);
            if (!ignoreList.Contains(type))
                stmt.AddChild(currentToken);
        }
        private void AddChildToStmt(ref INode stmt, TokenType[] types)
        {
            ExpectToken(types);
            stmt.AddChild(currentToken);
        }
        private void NextToken()
        {
            currentToken = TokensGenerated() && !IsAtEnd() ? Scanner.Tokens.Dequeue() : dummyToken;
        }
        private void ExpectToken(TokenType type)
        {
            if (Lookahead().Type != type)
                throw new SyntaxError($"Unexpected token {Lookahead().Type} (expected {type})",
                    currentToken.Pos);
            NextToken();
        }
        private void ExpectToken(TokenType[] types)
        {
            if (!types.Contains(Lookahead().Type))
                throw new SyntaxError($"Unexpected token {Lookahead().Type}", currentToken.Pos);
            NextToken();
        }
        private Token Lookahead()
        {
            return TokensGenerated() && !IsAtEnd() ? Scanner.Tokens.Peek() : dummyToken;
        }
        private bool IsAtEnd()
        {
            return TokensGenerated() ? Scanner.Tokens.Count < 2 : false;
        }
        private bool TokensGenerated()
        {
            return Scanner.Tokens.Count != 0;
        }
    }
}
