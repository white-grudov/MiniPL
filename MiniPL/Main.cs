using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniPL
{
    internal class Main
    {
        private readonly Parser Parser;
        private readonly string Filename;
        private readonly Context Context;
        private readonly SemanticAnalyzer Analyzer;

        private bool debugMode = false;

        public Main(string filename, bool debugMode = false)
        {
            this.debugMode = debugMode;

            Filename = filename;
            Parser = new Parser(filename);
            Context = new Context();
            Analyzer = new SemanticAnalyzer(Parser.Ast);
        }
        public void Run()
        {
            try
            { 
                // if (debugMode) PrintTokens();
                Parser.Parse();
                if (debugMode) PrintAST(Parser.Ast.Root);
                // Analyzer.Analyze();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
/*        private void PrintTokens()
        {
            foreach (var token in Parser.Scanner.Tokens)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", token.Type, token.Value, token.Pos);
        }*/
        private void PrintAST(INode node)
        {
            if (node == null) return;
            Console.Write('(');
            foreach (var child in node.GetAllChildren())
            {
                Console.Write($" {child.GetType()},".Replace("MiniPL.", "").Replace("Node", ""));
                if (child.GetAllChildren().Count != 0)
                {
                    PrintAST(child);
                }
                else if (child is TokenNode)
                {
                    Console.Write($" [{((TokenNode)child).GetValue()}],");
                }
            }
            Console.WriteLine("\b)");
        }
    }
}
