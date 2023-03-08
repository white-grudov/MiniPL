namespace MiniPL
{
    public class ErrorMessage
    {
        public const string LE_SOURCE_EMPTY = "Source file is empty";
        public const string LE_INVALID_CHAR = "Invalid char error";
        public const string LE_UNTERMINATED_STR = "Unterminated string";
        public const string LE_UNRECOGNIZED_ESCAPE = "Unrecognized escape sequence";
        public const string LE_ILLEGAL_CHAR_SEQ = "Illegal char sequence";
        public const string LE_UNENCLOSED_COMMENT = "Unenclosed comment";
        public const string LE_RANGE_EXPECTED = "Expected \"..\", got \".\" instead";
    }
    public class ExMessage
    {
        public static string Form(string type, string message, Position pos)
        {
            return $"{type}: {message} on line {pos.line} column {pos.column}";
        }
    }
}
