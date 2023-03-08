﻿namespace MiniPL
{
    public enum TokenType
    {
        // single-char tokens
        LPAREN, RPAREN, SEMICOLON, COLON,
        PLUS, MINUS, DIV, MUL,
        EQ, LT, GT, AND, NOT,

        // multi-char tokens
        ASSIGN, RANGE,

        // literals
        IDENTIFIER, INT_LITERAL, STRING_LITERAL,

        // keywords
        FOR, IN, IF, ELSE, DO, END, VAR, PRINT, READ,
        INT, STRING, BOOL, ASSERT,

        EOF, ILLEGAL
    }
    public static class TokenTypeExtenstions
    {
        public static string ToFriendlyString(this TokenType me)
        {
            switch (me)
            {
                case TokenType.INT: case TokenType.INT_LITERAL:
                    return "int";
                case TokenType.STRING: case TokenType.STRING_LITERAL:
                    return "string";
                case TokenType.BOOL:
                    return "bool";
                default:
                    return me.ToString().ToLower();
            }
        }
    }

    public struct Position
    {
        public int line;
        public int column;
        public Position(int line, int column)
        {
            this.line = line;
            this.column = column;
        }
        public override string ToString()
        {
            return string.Format("Ln: {0, -4} Cl: {1, -4}", line, column);
        }
    }
    public struct Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public Position Pos { get; }

        public Token(TokenType type, string value, Position pos)
        {
            Type = type;
            Value = value;
            Pos = pos;
        }
    }
}
