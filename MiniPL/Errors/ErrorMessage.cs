namespace MiniPL
{
    // String values for error messages in different parts of the MiniPL interpreter
    public class ErrorMessage
    {
        // Lexical errors
        public const string LE_SOURCE_EMPTY        = "Source file is empty";
        public const string LE_INVALID_CHAR        = "Invalid char error";
        public const string LE_UNTERMINATED_STR    = "Unterminated string";
        public const string LE_UNRECOGNIZED_ESCAPE = "Unrecognized escape sequence";
        public const string LE_ILLEGAL_CHAR_SEQ    = "Illegal char sequence";
        public const string LE_UNENCLOSED_COMMENT  = "Unenclosed comment";
        public const string LE_RANGE_EXPECTED      = "Expected \"..\", got \".\" instead";

        // Syntax errors
        public const string SE_UNEXPECTED_TOKEN    = "Unexpected token";
        public const string SE_MISSING_SEMICOLON   = "Missing semicolon";
        public const string SE_ILLEGAL_TOKEN       = "Illegal token";

        // Semantic errors
        public const string SE_VAR_NOT_DECLARED    = "Variable is not declared";
        public const string SE_VAR_TYPE_DISMATCH   = "Variable type dismatch";
        public const string SE_VAR_DECLARED        = "Variable is already declared";

        // Runtime errors
        public const string RE_CAST_TO_INT         = "Unable to cast input to int";
        public const string RE_UNINITIALIZED_VAR   = "Usage of uninitialized variable";
        public const string RE_DIVISION_BY_ZERO    = "Division by zero";
    }
    // Error message formatting
    public class ExMessage
    {
        public static string Form(string type, string message, Position pos)
        {
            return $"{type}: {message} on line {pos.line} column {pos.column}";
        }
    }
}
