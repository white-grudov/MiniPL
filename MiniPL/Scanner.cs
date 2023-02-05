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
    // TODO: fix position count
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
        public TokenType type;
        public string value;
        public Position pos;

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

        char currentChar = '\0';
        bool isIllegalToken = false;

        Position currentPos = new Position(1, 0);
        Queue<char> symbols = new Queue<char>();
        Queue<Token> tokens = new Queue<Token>();

        StringBuilder buffer = new StringBuilder();
        Exception? exception = null;

        Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "for",    TokenType.FOR    },
            { "in",     TokenType.IN     },
            { "if",     TokenType.IF     },
            { "else",   TokenType.ELSE   },
            { "do",     TokenType.DO     },
            { "end",    TokenType.END    },
            { "var",    TokenType.VAR    },
            { "print",  TokenType.PRINT  },
            { "read",   TokenType.READ   },
            { "int",    TokenType.INT    },
            { "string", TokenType.STRING },
            { "bool",   TokenType.BOOL   },
            { "assert", TokenType.ASSERT }
        };
        Dictionary<char, char> escapeChars = new Dictionary<char, char> {
            {'n', '\n'}, {'t', '\t'}, {'\'', '\''}, {'\"', '\"'},
            {'r', '\r'}, {'f', '\f'}, {'v', '\v'}
        };
        List<char> allowedChars = new List<char>() {')', ';', ' ', '.', '+', '-', '/', '*', '\n', '\t'};
        public Scanner(string filename)
        {
            // scanning the file
            this.filename = filename;
            file = ReadFile();
            if (file == null)
                // redo with custom exceptions
                throw new ArgumentNullException("The source file is empty");

            // getting the char queue
            symbols = new Queue<char>(file);
            Tokenize();

            foreach (var token in tokens)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", token.type, token.value, token.pos);
        }

        private void Tokenize()
        {
            while (symbols.Count > 0 && !isIllegalToken)
            {
                Advance();
                switch (currentChar)
                {
                    // blank spaces
                    case ' ': case '\t':
                        break;

                    // operators
                    case '+':
                        AddToken(TokenType.PLUS, currentChar.ToString());
                        break;
                    case '-':
                        AddToken(TokenType.MINUS, currentChar.ToString());
                        break;
                    case '*':
                        AddToken(TokenType.MUL, currentChar.ToString());
                        break;
                    case '/':
                        AddToken(TokenType.DIV, currentChar.ToString());
                        break;
                    case '<':
                        AddToken(TokenType.LT, currentChar.ToString());
                        break;
                    case '>':
                        AddToken(TokenType.GT, currentChar.ToString());
                        break;
                    case '&':
                        AddToken(TokenType.AND, currentChar.ToString());
                        break;
                    case '!':
                        AddToken(TokenType.NOT, currentChar.ToString());
                        break;
                    case '=':
                        AddToken(TokenType.EQ, currentChar.ToString());
                        break;

                    // other symbols
                    case ';':
                        AddToken(TokenType.SEMICOLON, currentChar.ToString());
                        // TODO: add appearance of illegal char error handling
                        break;

                    // multichar tokens
                    case ':':
                        AddAssign();
                        break;
                    case '.':
                        AddDoubledot();
                        break;

                    // parenthesises
                    case '(':
                        AddToken(TokenType.LPAREN, currentChar.ToString());
                        break;
                    case ')':
                        AddToken(TokenType.RPAREN, currentChar.ToString());
                        break;

                    default:
                        // number literal
                        if (char.IsDigit(currentChar))
                        {
                            AddNumberLiteral();
                        }
                        // keywords and identifiers
                        else if (char.IsLetter(currentChar) || currentChar == '_')
                        {
                            AddIdentifierOrKeyword();
                        }
                        // string literal
                        else if (currentChar == '\"')
                        {
                            AddStringLiteral();
                        }
                        else
                        {
                            IllegalToken(currentChar.ToString(), "Invalid char error");
                        }
                        break;
                }
            }
            if (!isIllegalToken) AddToken(TokenType.EOF, "<EOF>");
        }
        // adds string literal token with escape symbols
        private void AddStringLiteral()
        {
            buffer.Append(currentChar);
            bool isEscapeChar = false;
            while ((Lookahead() != '\"' && !isEscapeChar) || isEscapeChar)
            {
                if (Lookahead() == '\n')
                {
                    IllegalToken(buffer.ToString(), "Unterminated string");
                    return;
                }
                Advance();
                // escape char
                if (currentChar == '\\')
                {
                    isEscapeChar = true;
                }
                // add special symbol to string literal
                else if (isEscapeChar)
                {
                    if (escapeChars.ContainsKey(currentChar))
                    {
                        buffer.Append(escapeChars[currentChar]);
                        isEscapeChar = false;
                    }
                    else
                    {
                        IllegalToken(buffer.ToString(), "Illegal char sequence");
                        return;
                    }
                }
                else buffer.Append(currentChar);
            }
            Advance();
            buffer.Append(currentChar);
            AddToken(TokenType.STRING, buffer.ToString());

            buffer.Clear();
        }
        // adds identifier and determines whether it is a keyword
        private void AddIdentifierOrKeyword()
        {
            buffer.Append(currentChar);
            while (char.IsLetterOrDigit(Lookahead()) || Lookahead() == '_')
            {
                Advance();
                buffer.Append(currentChar);
            }
            if (!allowedChars.Contains(Lookahead()))
            {
                Advance();
                buffer.Append(currentChar);

                IllegalToken(buffer.ToString(), "Illegal char sequence");
                return;
            }

            string result = buffer.ToString();
            if (keywords.ContainsKey(result))
            {
                AddToken(keywords[result], result);
            }
            else AddToken(TokenType.IDENTIFIER, result);

            buffer.Clear();
        }
        // adds number literal token
        private void AddNumberLiteral()
        {
            buffer.Append(currentChar);
            while (char.IsDigit(Lookahead()))
            { 
                Advance();
                buffer.Append(currentChar);
            }
            if (!allowedChars.Contains(Lookahead()))
            {
                Advance();
                buffer.Append(currentChar);

                IllegalToken(buffer.ToString(), "Illegal char sequence");
                return;
            }
            AddToken(TokenType.INT_LITERAL, buffer.ToString());
            buffer.Clear();
        }
        // checks next char to determine whether it is assign operator, colon or invalid token
        private void AddAssign()
        {
            buffer.Append(currentChar);
            Advance();
            if (currentChar == '=')
            {
                buffer.Append(currentChar);
                AddToken(TokenType.ASSIGN, buffer.ToString());
            }
            else if (currentChar == ' ')
            {
                AddToken(TokenType.COLON, buffer.ToString());
            }
            else
            {
                buffer.Append(currentChar);
                IllegalToken(buffer.ToString(), "Expected char error");
                return;
            }
            buffer.Clear();
        }
        // checks next token to determine whether it is double dot (for loop) or invalid token
        private void AddDoubledot()
        {
            buffer.Append(currentChar);
            Advance();
            if (currentChar == '.')
            {
                buffer.Append(currentChar);
                AddToken(TokenType.DOUBLEDOT, buffer.ToString());
            }
            else
            {
                buffer.Append(currentChar);
                IllegalToken(buffer.ToString(), "Expected char error");
                return;
            }
            buffer.Clear();
        }

        private void IllegalToken(string token, string error)
        {
            AddToken(TokenType.ILLEGAL, token);
            isIllegalToken = true;
            exception = new LexicalError(error, currentPos);
            buffer.Clear();
        }

        private void Advance()
        {
            currentChar = symbols.Dequeue();
            currentPos.column++;
            if (currentChar == '\n')
            {
                currentPos.column = 1;
                currentPos.line++;
                currentChar = symbols.Dequeue();
            }
        }

        private char Lookahead()
        {
            return symbols.Peek();
        }

        private void AddToken(TokenType type, string value)
        {
            tokens.Enqueue(new Token(type, value, new Position(currentPos.line, currentPos.column - value.Length + 1)));
        }

        private string ReadFile()
        {
            string[] lines = File.ReadAllLines(filename);
            return String.Join("\n", lines);
        }
    }
}
