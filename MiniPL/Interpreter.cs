using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    internal class Interpreter
    {
        Parser parser;
        
        public Interpreter(string filename)
        {
            parser = new Parser(filename);
        }
        public void Run()
        {
            try
            {
                parser.GenerateTokens();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
