using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    // for further expansion with visitor pattern
    interface INode
    {
        public void AddChild(INode node);
        public void AddChild(Token token);
    }
    abstract class Node : INode
    {
        public Token NodeToken { get; protected set; }
        private List<INode> children = new List<INode>();

        private List<TokenType> opNodes = new List<TokenType>() {
            TokenType.PLUS, TokenType.MINUS, TokenType.DIV, TokenType.MUL,
            TokenType.EQ, TokenType.LT, TokenType.GT, TokenType.AND
        };
        private List<TokenType> typeNodes = new List<TokenType>() {
            TokenType.INT, TokenType.STRING, TokenType.BOOL
        };
        public Node(Token token)
        {
            NodeToken = token;
        }
        public void AddChild(INode node)
        {
            children.Add(node);
        }
        public void AddChild(Token token)
        {
            INode? node = createNode(token);
            if (node != null)
                children.Add(node);
            else throw new SyntaxError("Unexpected token", token.Pos);
        }
        private INode? createNode(Token token)
        {
            switch (token.Type)
            {
                case TokenType.IDENTIFIER:
                    return new IdentNode(token);
                case TokenType.VAR:
                    return new DeclNode(token);
                case var _ when typeNodes.Contains(token.Type):
                    return new TypeNode(token);
                case TokenType.ASSIGN:
                    return new AssignNode(token);
                case TokenType.PRINT:
                    return new PrintNode(token);
                case TokenType.READ:
                    return new ReadNode(token);
                case TokenType.FOR:
                    return new ForNode(token);
                case TokenType.IF:
                    return new IfNode(token);
                case TokenType.DOUBLEDOT:
                    return new RangeNode(token);
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
    }
    // root node
    class ProgNode : Node
    {
        public ProgNode(Token token) : base(token) { }
    }
    class StmtsNode : Node
    {
        public StmtsNode(Token token) : base(token) { }
    }
    class StmtNode : Node
    {
        public StmtNode(Token token) : base(token) { }
    }
    class AssignNode : Node
    {
        public AssignNode(Token token) : base(token) { }
    }
    class DeclNode : Node
    {
        public DeclNode(Token token) : base(token) { }
    }
    class ForNode : Node
    {
        public ForNode(Token token) : base(token) { }
    }
    class IfNode : Node
    {
        public IfNode(Token token) : base(token) { }
    }
    class ReadNode : Node
    {
        public ReadNode(Token token) : base(token) { }
    }
    class PrintNode : Node
    {
        public PrintNode(Token token) : base(token) { }
    }
    class ExprNode : Node
    {
        public ExprNode(Token token) : base(token) { }
    }
    class OpndNode : Node
    {
        public OpndNode(Token token) : base(token) { }
    }
    class OpNode : Node
    {
        public OpNode(Token token) : base(token) { }
    }
    class UnOpNode : Node
    {
        public UnOpNode(Token token) : base(token) { }
    }
    class IdentNode : Node
    {
        public IdentNode(Token token) : base(token) { }
    }
    class TypeNode : Node
    {
        public TypeNode(Token token) : base(token) { }
    }
    class RangeNode : Node
    {
        public RangeNode(Token token) : base(token) { }
    }
    class AssertNode : Node
    {
        public AssertNode(Token token) : base(token) { }
    }
    class IntNode : Node
    {
        public IntNode(Token token) : base(token) { }
    }
    class StrNode : Node
    {
        public StrNode(Token token) : base(token) { }
    }
}
