using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    enum TokenType
    {
        // single-char tokens
        LPAREN, RPAREN, SEMICOLON, COLON,
        PLUS, MINUS, DIV, MUL,
        EQ, LT, GT, AND, NOT,

        // multi-char tokens
        ASSIGN, DOUBLEDOT,

        // literals
        IDENTIFIER, INT_LITERAL, STRING_LITERAL,

        // keywords
        FOR, IN, IF, ELSE, DO, END, VAR, PRINT, READ,
        INT, STRING, BOOL, ASSERT,

        DUMMY, EOF, ILLEGAL
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
    struct Token
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
        public Token(Token token)
        {
            Type = token.Type;
            Value = token.Value;
            Pos = token.Pos;
        }
    }
}
