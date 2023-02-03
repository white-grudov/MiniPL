using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    abstract class Exception
    {
        string type = "";
        string message;
        Position pos;

        protected Exception(string message, Position pos)
        {
            this.message = message;
            this.pos = pos;
        }

        public void What()
        {
            Console.WriteLine($"{type}: {message} on line {pos.line} column {pos.column}.");
        }
    }

    class LexicalError : Exception
    {
        string type = "LexicalError";

        LexicalError(string message, Position pos) : base(message, pos) { }
    }
}
