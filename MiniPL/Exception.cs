using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    public abstract class Exception
    {
        string message;
        Position pos;
        public string? Type { get; protected set; }

        public Exception(string message, Position pos)
        {
            this.message = message;
            this.pos = pos;
        }

        public void What()
        {
            Console.WriteLine($"{Type}: {message} on line {pos.line} column {pos.column}.");
        }
    }

    public class LexicalError : Exception
    {
        public LexicalError(string message, Position pos) : base(message, pos)
        {
            Type = "LexicalError";
        }
    }

    public class SyntaxError : Exception
    {
        public SyntaxError(string message, Position pos) : base(message, pos)
        {
            Type = "SyntaxError";
        }
    }
}
