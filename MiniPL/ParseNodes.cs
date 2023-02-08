using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    abstract class Node
    {
        
    }
    // root node
    class ProgNode : Node
    {
        StmtsNode Stmts { get; set; }
        public ProgNode(StmtsNode stmts)
        {
            Stmts = stmts;
        }
    }
    class StmtsNode : Node
    {
        List<StmtNode> Stmts { get; set; }
        public StmtsNode(List<StmtNode> stmts)
        {
            Stmts = stmts;
        }
    }
    class StmtNode : Node
    {

    }
}
