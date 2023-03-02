using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniPL
{
    internal class Scanner
    {
        string filename;
        public string? file;

        char currentChar = '\0';
        bool isIllegalToken = false;

        Position currentPos = new Position(1, 0);
        Queue<char> symbols = new Queue<char>();
        public Token CurrentToken { get; private set; }
        public Token NextToken { get; private set; }

        StringBuilder buffer = new StringBuilder();

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
        Dictionary<char, TokenType> singleChar = new Dictionary<char, TokenType>()
        {
            { '+', TokenType.PLUS      },
            { '-', TokenType.MINUS     },
            { '*', TokenType.MUL       },
            { '<', TokenType.LT        },
            { '>', TokenType.GT        },
            { '&', TokenType.AND       },
            { '!', TokenType.NOT       },
            { '=', TokenType.EQ        },
            { ';', TokenType.SEMICOLON },
            { '(', TokenType.LPAREN    },
            { ')', TokenType.RPAREN    }
        };
        List<char> blankSpaces = new List<char>() { ' ', '\t' };
        Dictionary<char, char> escapeChars = new Dictionary<char, char> {
            {'n', '\n'}, {'t', '\t'}, {'\'', '\''}, {'\"', '\"'},
            {'r', '\r'}, {'f', '\f'}, {'v', '\v'}
        };
        List<char> allowedChars = new List<char>() { ')', ';', ' ', '.', '+', '-', '/', '*', '\n', '\t' };

        public Scanner(string filename)
        {
            // scanning the file
            this.filename = filename;
            file = ReadFile();
            // getting the char queue
            if (file == null)
            {
                throw new LexicalError("Source file is empty", currentPos);
            }
            symbols = new Queue<char>(file);

            Tokenize();
        }

        public void Tokenize()
        {
            if (symbols.Count > 0 && !isIllegalToken)
            {
                Advance();
                switch (currentChar)
                {
                    // blank spaces
                    case var _ when blankSpaces.Contains(currentChar):
                        Tokenize();
                        return;

                    // single character tokens
                    case var _ when singleChar.ContainsKey(currentChar):
                        AddToken(singleChar[currentChar], currentChar.ToString());
                        break;

                    // other symbols
                    case ':':
                        AddAssign();
                        break;
                    case '.':
                        AddDoubledot();
                        break;
                    case '/':
                        AddComment();
                        break;

                    // number literal
                    case var _ when char.IsDigit(currentChar):
                        AddNumberLiteral();
                        break;
                    // keywords and identifiers
                    case var _ when char.IsLetter(currentChar) || currentChar == '_':
                        AddIdentifierOrKeyword();
                        break;
                    // string literal
                    case var _ when currentChar == '\"':
                        AddStringLiteral();
                        break;

                    default:
                        IllegalToken(currentChar.ToString(), "Invalid char error");
                        break;
                }
            }
            else AddToken(TokenType.EOF, "");
        }
        // adds string literal token with escape symbols
        private void AddStringLiteral()
        {
            buffer.Append(currentChar);
            bool isEscapeChar = false;
            while (Lookahead() != '\"' && !isEscapeChar || isEscapeChar)
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
            AddToken(TokenType.STRING_LITERAL, buffer.ToString());

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
        // checks next char to determine whether it is a comment or div operator
        private void AddComment()
        {
            buffer.Append(currentChar);
            // single-line comment
            if (Lookahead() == '/')
            {
                while (!IsAtEnd() && Lookahead() != '\n') Advance();
                Tokenize();
            }
            // multi-line comment
            else if (Lookahead() == '*')
            {
                bool commentClosed = false;
                Advance();
                while (!IsAtEnd())
                {
                    if (currentChar == '*' && Lookahead() == '/')
                    {
                        Advance();
                        commentClosed = true;
                        break;
                    }
                    Advance();
                }
                if (!commentClosed)
                {
                    IllegalToken("", "Unenclosed comment");
                    return;
                }
                Tokenize();
            }
            // divider
            else
            {
                AddToken(TokenType.DIV, buffer.ToString());
            }
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
            buffer.Clear();

            throw new LexicalError(error, currentPos);
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
        private bool IsAtEnd()
        {
            return symbols.Count == 0;
        }
        private char Lookahead()
        {
            return symbols.Count != 0 ? symbols.Peek() : '\0';
        }

        private void AddToken(TokenType type, string value)
        {
            CurrentToken = NextToken;
            NextToken = new Token(type, value, new Position(currentPos.line, currentPos.column - value.Length + 1));
        }

        private string ReadFile()
        {
            string[] lines = File.ReadAllLines(filename);
            return string.Join("\n", lines);
        }
    }
}
