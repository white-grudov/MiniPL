namespace MiniPL
{
    // for further expansion with visitor pattern
    interface INode
    {
        public List<INode> GetAllChildren();
        public object Accept(IVisitor visitor);
    }
    abstract class Node : INode
    {
        public abstract List<INode> GetAllChildren();
        public abstract object Accept(IVisitor visitor);
    }
    // root node
    class ProgNode : Node
    {
        public StmtsNode? Stmts { get; protected set; }
        public ProgNode(StmtsNode? stmts = null)
        {
            Stmts = stmts;
        }
        public void AddStmts(StmtsNode stmts)
        {
            Stmts = stmts;
        }
        public override List<INode> GetAllChildren()
        {
            if (Stmts == null) return new List<INode>();
            return new List<INode>() { Stmts };
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    abstract class StmtNode : Node { }
    class StmtsNode : Node
    {
        public List<StmtNode> StmtNodes { get; protected set; }
        public StmtsNode()
        {
            StmtNodes = new List<StmtNode>();
        }
        public void AddChild(StmtNode child)
        {
            StmtNodes.Add(child);
        }
        public override List<INode> GetAllChildren()
        {
            List<INode> children = new List<INode>();
            foreach (var child in StmtNodes)
            {
                children.Add(child);
            }
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class DeclNode : StmtNode
    {
        public IdentNode Ident { get; protected set; }
        public TypeNode Type { get; protected set; }
        public ExprNode? Expr { get; protected set; }
        public DeclNode(IdentNode ident, TypeNode type, ExprNode? expr = null)
        {
            Ident = ident;
            Type = type;
            Expr = expr;
        }
        public void AddExpr(ExprNode expr)
        {
            Expr = expr;
        }
        public override List<INode> GetAllChildren()
        {
            List<INode> children = new List<INode>() { Ident, Type };
            if (Expr != null) children.Add(Expr);
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class AssignNode : StmtNode
    {
        public IdentNode Ident { get; protected set; }
        public ExprNode Expr { get; protected set; }
        public AssignNode(IdentNode ident, ExprNode expr)
        {
            Ident = ident;
            Expr = expr;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { Ident, Expr };
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class ForNode : StmtNode
    {
        public IdentNode Ident { get; protected set; }
        public ExprNode StartExpr { get; protected set; }
        public ExprNode EndExpr { get; protected set; }
        public StmtsNode Stmts { get; protected set; }
        public ForNode(IdentNode ident, ExprNode startExpr, ExprNode endExpr, StmtsNode stmts)
        {
            Ident = ident;
            StartExpr = startExpr;
            EndExpr = endExpr;
            Stmts = stmts;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { Ident, StartExpr, EndExpr, Stmts };
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class IfNode : StmtNode
    {
        public ExprNode Expr { get; protected set; }
        public StmtsNode IfStmts { get; protected set; }
        public StmtsNode? ElseStmts { get; protected set; }
        public IfNode(ExprNode expr, StmtsNode ifStmts, StmtsNode? elseStmts = null)
        {
            Expr = expr;
            IfStmts = ifStmts;
            ElseStmts = elseStmts;
        }
        public void AddElseStmts(StmtsNode elseStmts)
        {
            ElseStmts = elseStmts;
        }
        public override List<INode> GetAllChildren()
        {
            List<INode> children = new List<INode>() { Expr, IfStmts };
            if (ElseStmts != null) children.Add(ElseStmts);
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class ReadNode : StmtNode
    {
        public IdentNode Ident { get; protected set; }
        public ReadNode(IdentNode ident)
        {
            Ident = ident;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { Ident };
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class PrintNode : StmtNode
    {
        public ExprNode Expr { get; protected set; }
        public PrintNode(ExprNode expr)
        {
            Expr = expr;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { Expr };
        }
        public override object? Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class ExprNode : Node, OpndNodeChild
    {
        private enum States
        {
            ONLY_LEFT_OPND, UN_OP, LEFT_RIGHT_OPND
        }
        public UnOpNode? UnOp { get; protected set; }
        public OpndNode LeftOpnd { get; protected set; }
        public OpNode? Op { get; protected set; }
        public OpndNode? RightOpnd { get; protected set; }
        public Position Pos { get; protected set; }
        public string? Type { get; set; }

        private States state = States.ONLY_LEFT_OPND;
        public ExprNode(OpndNode leftOpnd, Position pos)
        {
            LeftOpnd = leftOpnd;
            UnOp = null;
            Op = null;
            RightOpnd = null;
            Pos = pos;
        }
        public void AddRightOpnd(OpNode op, OpndNode rightOpnd)
        {
            if (state == States.UN_OP)
                throw new Exception("Node is already with unary operator!");
            Op = op;
            RightOpnd = rightOpnd;
            state = States.LEFT_RIGHT_OPND;
        }
        public void AddUnOp(UnOpNode unOp)
        {
            if (state == States.LEFT_RIGHT_OPND)
                throw new Exception("Node is already with binary operator!");
            UnOp = unOp;
            state = States.UN_OP;
        }
        public override List<INode> GetAllChildren()
        {
            switch (state)
            {
                case States.ONLY_LEFT_OPND:
                    return new List<INode>() { LeftOpnd };
                case States.UN_OP:
                    return new List<INode>() { UnOp, LeftOpnd };
                case States.LEFT_RIGHT_OPND:
                    return new List<INode>() { LeftOpnd, Op, RightOpnd };
                default:
                    return new List<INode>();
            }
        }
        public override object Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    interface OpndNodeChild : INode { }
    class OpndNode : Node
    {
        public INode Child { get; protected set; }
        public Position Pos { get; protected set; }
        public OpndNode(OpndNodeChild child)
        {
            Child = child;
            if (child.GetType() == typeof(ExprNode))
            {
                Pos = ((ExprNode)child).Pos;
            }
            else
            {
                Pos = ((TokenNode)child).Token.Pos;
            }
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { Child };
        }
        public override object Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class TokenNode : Node
    {
        public Token Token { get; protected set; }
        public TokenNode(Token token)
        {
            Token = token;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>();
        }
        public string GetValue()
        {
            return Token.Value;
        }
        public override object Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    class OpNode : TokenNode
    {
        public OpNode(Token token) : base(token) { }
    }
    class UnOpNode : TokenNode
    {
        public UnOpNode(Token token) : base(token) { }
    }
    class IdentNode : TokenNode, OpndNodeChild
    {
        public IdentNode(Token token) : base(token) { }
    }
    class TypeNode : TokenNode
    {
        public TypeNode(Token token) : base(token) { }
    }
    class IntNode : TokenNode, OpndNodeChild
    {
        public IntNode(Token token) : base(token) { }
    }
    class StrNode : TokenNode, OpndNodeChild
    {
        public StrNode(Token token) : base(token) { }
    }
}
