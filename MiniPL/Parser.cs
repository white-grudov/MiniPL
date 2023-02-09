using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    using AST = ProgNode;
    internal class Parser
    {
        public AST Ast { get; }
        Scanner scanner;
        bool tokensGenerated = false;
        public Parser(string filename)
        {
            Ast = new AST(new Token());
            scanner = new Scanner(filename);
        }
        public void GenerateTokens()
        {
            scanner.Tokenize();
            tokensGenerated = true;

            foreach (var token in scanner.Tokens)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", token.Type, token.Value, token.Pos);
        }
        private Token? NextToken()
        {
            return tokensGenerated && !IsAtEnd() ? scanner.Tokens.Dequeue() : null;
        }
        private Token? Lookahead()
        {
            return scanner.Tokens.Peek();
        }
        private bool IsAtEnd()
        {
            return tokensGenerated ? scanner.Tokens.Count == 0 : false;
        }
    }
}
