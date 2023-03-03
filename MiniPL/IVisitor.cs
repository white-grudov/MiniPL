using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    interface IVisitor
    {
        object? Visit(ProgNode node);
        object? Visit(StmtsNode node);
        object? Visit(DeclNode node);
        object? Visit(AssignNode node);
        object? Visit(ForNode node);
        object? Visit(IfNode node);
        object? Visit(PrintNode node);
        object? Visit(ReadNode node);
        object Visit(ExprNode node);
        object Visit(OpndNode node);
        object Visit(TokenNode node);
    }
}
