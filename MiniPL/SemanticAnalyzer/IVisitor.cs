using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    public interface IVisitor
    {
        void Visit(ProgNode node);
        void Visit(StmtsNode node);
        void Visit(DeclNode node);
        void Visit(AssignNode node);
        void Visit(ForNode node);
        void Visit(IfNode node);
        void Visit(PrintNode node);
        void Visit(ReadNode node);
        object Visit(ExprNode node);
        object Visit(OpndNode node);
        object Visit(TokenNode node);
    }
}
