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

        EOF, ILLEGAL
    }
    struct Position
    {
        public int line;
        public int column;
        public Position(int line, int column)
        {
            this.line = line;
            this.column = column;
        }
        public Position(Position pos)
        {
            this.line = pos.line;
            this.column = pos.column;
        }
    }
    struct Token
    {
        TokenType type;
        string value;
        Position pos;

        public Token(TokenType type, string value, Position pos)
        {
            this.type = type;
            this.value = value;
            this.pos = pos;
        }
    }
    internal class Scanner
    {
        string filename;
        string? file;

        Position currentPos = new Position(0, 0);
        List<Token> tokens = new List<Token>();

        public Scanner(string filename)
        {
            this.filename = filename;
            file = ReadFile();
            if (file != null) Tokenize();
        }

        private void Tokenize()
        {
            if (file == null)
                throw new ArgumentNullException("The source file is empty");
            foreach (char c in file)
            {
                currentPos.column++;
                if (c == '\n')
                {
                    currentPos.column = 0;
                    currentPos.line++;
                    continue;
                }
                switch (c)
                {
                    // blank spaces
                    case ' ': case '\t':
                        break;

                    // operators
                    case '+':
                        AddToken(TokenType.PLUS, c.ToString());
                        break;
                    case '-':
                        AddToken(TokenType.MINUS, c.ToString());
                        break;
                    case '*':
                        AddToken(TokenType.MUL, c.ToString());
                        break;
                    case '/':
                        AddToken(TokenType.DIV, c.ToString());
                        break;
                    case '<':
                        AddToken(TokenType.LT, c.ToString());
                        break;
                    case '>':
                        AddToken(TokenType.GT, c.ToString());
                        break;
                    case '&':
                        AddToken(TokenType.AND, c.ToString());
                        break;
                    case '!':
                        AddToken(TokenType.NOT, c.ToString());
                        break;
                    case '=':
                        AddToken(TokenType.EQ, c.ToString());
                        break;

                    // parenthesises
                    case '(':
                        AddToken(TokenType.LPAREN, c.ToString());
                        break;
                    case ')':
                        AddToken(TokenType.RPAREN, c.ToString());
                        break;
                }
            }
        }

        private void AddToken(TokenType type, string value)
        {
            tokens.Add(new Token(type, value, new Position(currentPos)));
        }

        private string ReadFile()
        {
            string[] lines = File.ReadAllLines(filename);
            return String.Join("\n", lines);
        }
    }
}
