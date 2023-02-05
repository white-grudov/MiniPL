using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    internal class Parser
    {
        Scanner scanner;
        Queue<Token> tokens = new Queue<Token>();
        public Parser(string filename)
        {
            scanner = new Scanner(filename);

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
