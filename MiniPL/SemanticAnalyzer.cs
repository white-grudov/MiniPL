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
        object? Visit(ExprNode node);
        object? Visit(OpndNode node);
        object Visit(TokenNode node);
    }
    internal class SemanticAnalyzer : IVisitor
    {
        private readonly AST Ast;
        private readonly Context Context;
        public SemanticAnalyzer(AST ast)
        {
            Ast = ast;
            Context = new Context();
        }
        public void Analyze()
        {
            Ast.Root.Accept(this);
        }

        public object? Visit(ProgNode node)
        {
            if (node.Stmts != null) node.Stmts.Accept(this);
            return null;
        }

        public object? Visit(StmtsNode node)
        {
            foreach (var child in node.GetAllChildren())
            {
                child.Accept(this);
            }
            return null;
        }

        public object? Visit(DeclNode node)
        {
            string name = (string)node.Ident.Accept(this);
            string type = (string)node.Type.Accept(this);

            if (Context.ContainsVariable(name))
            {
                throw new SemanticError("Variable is not declared.", node.Ident.Token.Pos);
            }
            Context.Declare(name, type);
            return null;
        }

        public object? Visit(AssignNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(ForNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(IfNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(PrintNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(ReadNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public object? Visit(OpndNode node)
        {
            throw new NotImplementedException();
        }

        public object Visit(TokenNode node)
        {
            return node.Token.Value;
        }
    }
}
