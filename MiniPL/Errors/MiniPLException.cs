namespace MiniPL
{
    public abstract class MiniPLException : Exception
    {
        public Position Pos { get; protected set; }
        public MiniPLException(string message) : base(message) { }
    }
    public class LexicalError : MiniPLException
    {
        public const string type = "LexicalError";
        public LexicalError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class SyntaxError : MiniPLException
    {
        public const string type = "SyntaxError";
        public SyntaxError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class SemanticError : MiniPLException
    {
        public const string type = "SemanticError";
        public SemanticError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class RuntimeError : MiniPLException
    {
        public const string type = "RuntimeError";
        public RuntimeError(string message, Position pos) : base(ExMessage.Form(type, message, pos))
        {
            Pos = pos;
        }
    }
    public class FileNotFoundError : MiniPLException
    {
        public FileNotFoundError(string path) : base($"File {path} does not exist") { }
    }
    public class ErrorList : Exception
    {
        public List<MiniPLException> Errors { get; private set; }
        public ErrorList(List<MiniPLException> errors)
        {
            Errors = errors;
        }
    }
}
