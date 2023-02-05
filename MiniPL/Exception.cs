using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    public abstract class Exception
    {
        string type = "";
        string message;
        Position pos;

        public Exception(string message, Position pos)
        {
            this.message = message;
            this.pos = pos;
        }

        public void What()
        {
            Console.WriteLine($"{type}: {message} on line {pos.line} column {pos.column}.");
        }
    }

    public class LexicalError : Exception
    {
        readonly string type = "LexicalError";

        public LexicalError(string message, Position pos) : base(message, pos) { }
    }

    public class ParseError : Exception
    {
        readonly string type = "ParseError";

        public ParseError(string message, Position pos) : base(message, pos) { }
    }
}
