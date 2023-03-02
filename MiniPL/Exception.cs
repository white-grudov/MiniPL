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
        public LexicalError(string message, Position pos) 
            : base(ExMessage.Form(type, message, pos)) { }
    }

    public class SyntaxError : Exception
    {
        private static string type = "SyntaxError";
        public SyntaxError(string message, Position pos)
            : base(ExMessage.Form(type, message, pos)) { }
    }
    public class SemanticError : Exception
    {
        private static string type = "SemanticError";
        public SemanticError(string message, Position pos)
            : base(ExMessage.Form(type, message, pos)) { }
    }
}
