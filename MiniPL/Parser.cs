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
        Queue<Token> tokens = new Queue<Token>();
        public Parser(string filename)
        {
            // generate token queue
            scanner = new Scanner(filename);

            if (scanner.Exception != null)
            {
                scanner.Exception.What();
                return;
            }

            foreach (var token in scanner.Tokens)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", token.type, token.value, token.pos);
        }
        private Token NextToken()
        {
            return tokens.Dequeue();
        }
        private Token Lookahead()
        {
            return tokens.Peek();
        }
    }
}
