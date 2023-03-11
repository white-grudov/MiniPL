namespace MiniPL
{
    // Interface for nodes
    public interface INode
    {
        public List<INode> GetAllChildren();
        public object? Accept(IVisitor visitor);
        public void Print(int indent = 0);
    }
    // Abstract class Node which defines Print() method
    public abstract class Node : INode
    {
        public abstract List<INode> GetAllChildren();
        public abstract object? Accept(IVisitor visitor);
        public void Print(int indent = 0)
        {
            string result = $"{new string(' ', indent)}{GetType().Name}";
            if (this is TokenNode)
            {
                result += $" [{((TokenNode)this).Token.Value}]";
            }
            Console.WriteLine(result);

            foreach (var node in GetAllChildren())
            {
                node.Print(indent + 2);
            }
        }
    }
    // Error node for processing parse errrors
    public class ErrorNode : Node
    {
        public override object? Accept(IVisitor visitor) { return null; }
        public override List<INode> GetAllChildren() { return new List<INode>(); }
    }
    // Program root node
    public class ProgNode : Node
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
            visitor.Visit(this);
            return null;
        }
    }
    // Abstract node for statement
    public abstract class StmtNode : Node { }
    // Statements node that stores the list of statements
    public class StmtsNode : Node
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
            List<INode> children = new();
            foreach (var child in StmtNodes)
            {
                children.Add(child);
            }
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            return null;
        }
    }
    // Declaration node which has identifier, type and optional expression
    public class DeclNode : StmtNode
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
            List<INode> children = new() { Ident, Type };
            if (Expr != null) children.Add(Expr);
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            return null;
        }
    }
    // Assignment node which has identifier and expression
    public class AssignNode : StmtNode
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
            visitor.Visit(this);
            return null;
        }
    }
    // For node which stores identifier, two range expressions and nested statements
    public class ForNode : StmtNode
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
            visitor.Visit(this);
            return null;
        }
    }
    // If node which stores conditional expression, nested statements and optional else statements
    public class IfNode : StmtNode
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
            List<INode> children = new() { Expr, IfStmts };
            if (ElseStmts != null) children.Add(ElseStmts);
            return children;
        }
        public override object? Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            return null;
        }
    }
    // Read node which stores identifier
    public class ReadNode : StmtNode
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
            visitor.Visit(this);
            return null;
        }
    }
    // Print node which stores expression
    public class PrintNode : StmtNode
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
            visitor.Visit(this);
            return null;
        }
    }
    // Abstract expr node class which also inherits from operand child node interface and stores position
    public abstract class ExprNode : Node, OpndNodeChild
    {
        public Position Pos { get; protected set; }
        public string? Type { get; set; }
        public override object Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    // Unary operator expression node derived class
    public class UExprNode : ExprNode
    {
        public UnOpNode UnOp { get; protected set; }
        public OpndNode LeftOpnd { get; protected set; }
        public UExprNode(UnOpNode unOp, OpndNode leftOpnd, Position pos)
        {
            UnOp = unOp;
            LeftOpnd = leftOpnd;
            Pos = pos;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { UnOp, LeftOpnd };
        }
    }
    // Only one operand expression node derived class
    public class LExprNode : ExprNode
    {
        public OpndNode LeftOpnd { get; protected set; }
        public LExprNode(OpndNode leftOpnd, Position pos)
        {
            LeftOpnd = leftOpnd;
            Pos = pos;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { LeftOpnd };
        }
    }
    // Two operands expression node derived class
    public class LRExprNode : ExprNode
    {
        public OpndNode LeftOpnd { get; protected set; }
        public OpNode Op { get; protected set; }
        public OpndNode RightOpnd { get; protected set; }
        public LRExprNode(OpndNode leftOpnd, OpNode op, OpndNode rightOpnd, Position pos)
        {
            LeftOpnd = leftOpnd;
            Op = op;
            RightOpnd = rightOpnd;
            Pos = pos;
        }
        public override List<INode> GetAllChildren()
        {
            return new List<INode>() { LeftOpnd, Op, RightOpnd };
        }
    }
    // Interface for operand node children
    public interface OpndNodeChild : INode
    {
        public new object Accept(IVisitor visitor);
    }
    // Operand node which stores operand node child and position
    public class OpndNode : Node
    {
        public OpndNodeChild Child { get; protected set; }
        public Position Pos { get; protected set; }
        public OpndNode(OpndNodeChild child)
        {
            Child = child;
            if (child is ExprNode)
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
    // Abstract token node class for nodes which are based solely on corresponding tokens
    abstract public class TokenNode : Node
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
        public override object Accept(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
    // Nodes derived from token node
    public class OpNode : TokenNode
    {
        public OpNode(Token token) : base(token) { }
    }
    public class UnOpNode : TokenNode
    {
        public UnOpNode(Token token) : base(token) { }
    }
    public class IdentNode : TokenNode, OpndNodeChild
    {
        public IdentNode(Token token) : base(token) { }
    }
    public class TypeNode : TokenNode
    {
        public TypeNode(Token token) : base(token) { }
    }
    public class IntNode : TokenNode, OpndNodeChild
    {
        public IntNode(Token token) : base(token) { }
    }
    public class StrNode : TokenNode, OpndNodeChild
    {
        public StrNode(Token token) : base(token) { }
    }
}
