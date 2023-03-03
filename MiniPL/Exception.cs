namespace MiniPL
{
    public class ExMessage
    {
        public static string Form(string type, string message, Position pos)
        {
            return $"{type}: {message} on line {pos.line} column {pos.column}";
        }
    }

    public class LexicalError : Exception
    {
        private static string type = "LexicalError";
        public Position Pos { get; private set; }
        public LexicalError(string message, Position pos) 
            : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }

    public class SyntaxError : Exception
    {
        private static string type = "SyntaxError";
        public Position Pos { get; private set; }
        public SyntaxError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class SemanticError : Exception
    {
        private static string type = "SemanticError";
        public Position Pos { get; private set; }
        public SemanticError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class RuntimeError : Exception
    {
        private static string type = "RuntimeError";
        public Position Pos { get; private set; }
        public RuntimeError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
}
