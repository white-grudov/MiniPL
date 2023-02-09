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
        public static INode? createNode(Token token)
        {
            switch (token.Type)
            {
                // implement node creator
            }
            return null;
        }
    }
    abstract class Node : INode
    {
        public Token NodeToken { get; protected set; }
        private List<INode> children = new List<INode>();
        public Node(Token token)
        {
            NodeToken = token;
        }
        public void AddChild(INode node)
        {
            children.Add(node);
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
    class DeclareNode : Node
    {
        public DeclareNode(Token token) : base(token) { }
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

}
