using System.Text;

namespace MiniPL
{
    /* Scanner is the part of the MiniPL Interpreter which is responsible for reading the source file
     * and generating a character stream out of it. When the Tokenize() method is called, the scanner
     * processes the character and generates a token, which is then passed to the parser. When the
     * character stream is empty, the scanner generates EOF token.
     */
    public class Scanner
    {
        public string? File { get; private set; } // the source code
        private readonly string filename;
        private bool fileIsRead = false; // checks if the source code has already been loaded

        private char currentChar = '\0';
        private readonly bool debugMode; // if true, prints tokens to the console

        private Position currentPos = new(1, 0); // for error messages
        private Queue<char> symbols = new(); // characters which compose the source code
        public Token CurrentToken { get; private set; }
        public Token NextToken { get; private set; }

        private readonly StringBuilder buffer = new(); // buffer for composing the multi-char tokens

        /* Below are dictionaries and list which help to detect keywords, single-char tokens,
         * characters to skip (blank spaces), escape characters inside of string literals,
         * and allowed characters after identifier and number literal tokens
         */
        private readonly Dictionary<string, TokenType> keywords = new()
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
        private readonly Dictionary<char, TokenType> singleChar = new()
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
        private readonly List<char> blankSpaces = new()
        {
            ' ', '\t',
        };
        private readonly Dictionary<char, char> escapeChars = new()
        {
            {'n', '\n'}, {'t', '\t'}, {'\'', '\''}, {'\"', '\"'},
            {'r', '\r'}, {'f', '\f'}, {'v', '\v'}, {'\\', '\\'}
        };
        private readonly List<char> allowedChars = new() 
        {
            ')', ';', ' ', '.', '+', '-', '/', '*', '&', '\n', '\t'
        };
        
        public Scanner(string filename, bool debugMode)
        {
            this.filename = filename;
            this.debugMode = debugMode;
        }

        /* The most important method of the class, which, when called, generates one token and stores it in 
         * NextToken variable, while the previous token is assigned to CurrentToken. The method advances
         * to the next character and then checks it with token patterns. If character is invalid, the
         * exception is thrown.
         */
        public void Tokenize()
        {
            // The reading file functionality is in this method due to error handling realization
            if (!fileIsRead) ReadFile();
            if (symbols.Count > 0)
            {
                NextChar();
                switch (currentChar)
                {
                    // Blank spaces: [ \t\n]+
                    case var _ when blankSpaces.Contains(currentChar):
                        Tokenize();
                        return;

                    // Single-char tokens: [+\-*<>!&=;()]
                    case var _ when singleChar.ContainsKey(currentChar):
                        AddToken(singleChar[currentChar], currentChar.ToString());
                        break;

                    // Single-char tokens or multi-char ones, which can start with single-char ones

                    // Assign/Colon: :=|:
                    case ':':
                        AddAssign();
                        break;
                    // Doubledot: ..
                    case '.':
                        AddDoubledot();
                        break;
                    // Division/Single-line/Multi-line comments: \/ | \/\/.*$ | \/\*.*?\*\/
                    case '/':
                        AddComment();
                        break;

                    // Number literal: [0-9]+
                    case var _ when char.IsDigit(currentChar):
                        AddNumberLiteral();
                        break;
                    // Keywords/Identifiers: [a-zA-Z_][a-zA-Z0-9_]*
                    case var _ when char.IsLetter(currentChar) || currentChar == '_':
                        AddIdentifierOrKeyword();
                        break;
                    // String literal: "[^"\\\n]*({0,1}:\\.[^"\\\n]*)*"
                    case var _ when currentChar == '\"':
                        AddStringLiteral();
                        break;

                    // Character which is not start of any token
                    default:
                        IllegalToken(currentChar.ToString(), ErrorMessage.LE_INVALID_CHAR);
                        break;
                }
            }
            // If EOF
            else AddToken(TokenType.EOF, "");
        }
        // Method was written solely for test purposes
        public List<Token> GenerateTokens()
        {
            var tokens = new List<Token>();
            while (NextToken.Type != TokenType.EOF)
            {
                Tokenize();
                tokens.Add(NextToken);
            }
            return tokens;
        }
        // Adds string literal token with escape symbols
        private void AddStringLiteral()
        {
            buffer.Append(currentChar);
            bool isEscapeChar = false;
            // Takes the chars until the next one is not "
            while (Lookahead() != '\"' && !isEscapeChar || isEscapeChar)
            {
                // Checks if the string is unterminated
                if (Lookahead() == '\n')
                {
                    IllegalToken(buffer.ToString(), ErrorMessage.LE_UNTERMINATED_STR);
                    return;
                }
                NextChar();

                // Add special symbol to string literal
                if (isEscapeChar)
                {
                    if (escapeChars.ContainsKey(currentChar))
                    {
                        buffer.Append(escapeChars[currentChar]);
                        isEscapeChar = false;
                    }
                    // Invalid escape character
                    else
                    {
                        IllegalToken(buffer.ToString(), ErrorMessage.LE_UNRECOGNIZED_ESCAPE);
                        return;
                    }
                }
                else
                {
                    // Escape character
                    if (currentChar == '\\')
                    {
                        isEscapeChar = true;
                    }
                    else buffer.Append(currentChar);
                }
            }
            NextChar();
            buffer.Append(currentChar);
            AddToken(TokenType.STRING_LITERAL, buffer.ToString());

            buffer.Clear();
        }
        // Adds identifier and determines whether it is a keyword
        private void AddIdentifierOrKeyword()
        {
            buffer.Append(currentChar);
            while (char.IsLetterOrDigit(Lookahead()) || Lookahead() == '_')
            {
                NextChar();
                buffer.Append(currentChar);
            }
            if (!allowedChars.Contains(Lookahead()) && !IsAtEnd())
            {
                NextChar();
                buffer.Append(currentChar);

                IllegalToken(buffer.ToString(), ErrorMessage.LE_ILLEGAL_CHAR_SEQ);
                return;
            }

            string result = buffer.ToString();
            // Check if the resulting identifier is in keyword dict
            if (keywords.ContainsKey(result))
            {
                AddToken(keywords[result], result);
            }
            else AddToken(TokenType.IDENTIFIER, result);

            buffer.Clear();
        }
        // Adds number literal token
        private void AddNumberLiteral()
        {
            buffer.Append(currentChar);
            while (char.IsDigit(Lookahead()))
            {
                NextChar();
                buffer.Append(currentChar);
            }
            if (!allowedChars.Contains(Lookahead()) && !IsAtEnd())
            {
                NextChar();
                buffer.Append(currentChar);

                IllegalToken(buffer.ToString(), ErrorMessage.LE_ILLEGAL_CHAR_SEQ);
                return;
            }
            AddToken(TokenType.INT_LITERAL, buffer.ToString());
            buffer.Clear();
        }
        // Checks next char to determine whether it is a comment or div operator
        private void AddComment()
        {
            buffer.Append(currentChar);
            // Single-line comment
            if (Lookahead() == '/')
            {
                while (!IsAtEnd() && Lookahead() != '\n') NextChar();
                buffer.Clear();
                Tokenize();
            }
            // Multi-line comment
            else if (Lookahead() == '*')
            {
                bool commentClosed = false;
                NextChar();
                while (!IsAtEnd())
                {
                    if (currentChar == '*' && Lookahead() == '/')
                    {
                        NextChar();
                        commentClosed = true;
                        break;
                    }
                    NextChar();
                }
                if (!commentClosed)
                {
                    IllegalToken("", ErrorMessage.LE_UNENCLOSED_COMMENT);
                    return;
                }
                buffer.Clear();
                Tokenize();
            }
            // Divider
            else
            {
                AddToken(TokenType.DIV, buffer.ToString());
            }
            buffer.Clear();
        }
        // Checks next char to determine whether it is assign operator or colon
        private void AddAssign()
        {
            buffer.Append(currentChar);
            if (Lookahead() == '=')
            {
                NextChar();
                buffer.Append(currentChar);
                AddToken(TokenType.ASSIGN, buffer.ToString());
            }
            else
            {
                AddToken(TokenType.COLON, buffer.ToString());
            }
            buffer.Clear();
        }
        // Checks next token to determine whether it is double dot (range operator) or invalid token
        private void AddDoubledot()
        {
            buffer.Append(currentChar);
            NextChar();
            if (currentChar == '.')
            {
                buffer.Append(currentChar);
                AddToken(TokenType.RANGE, buffer.ToString());
            }
            else
            {
                buffer.Append(currentChar);
                IllegalToken(buffer.ToString(), ErrorMessage.LE_RANGE_EXPECTED);
                return;
            }
            buffer.Clear();
        }
        /* Generates an illegal token and throws an error. Scanner uses panic mode recovery, so when it
         * generates an error, the program immediately stops execution.
         */
        private void IllegalToken(string token, string error)
        {
            AddToken(TokenType.ILLEGAL, token);
            buffer.Clear();

            throw new LexicalError(error, currentPos);
        }
        // Next char is dequeued and the current position is updated
        private void NextChar()
        {
            if (IsAtEnd())
            {
                currentChar = ' ';
                return;
            }
            currentChar = symbols.Dequeue();
            currentPos.column++;
            if (currentChar == '\n')
            {
                currentPos.column = 0;
                currentPos.line++;
                NextChar();
            }
        }
        // Checks if there are more characters left
        private bool IsAtEnd()
        {
            return symbols.Count == 0;
        }
        // Return the next character, but does not dequeue it
        private char Lookahead()
        {
            return symbols.Count != 0 ? symbols.Peek() : '\0';
        }
        // New token is created
        private void AddToken(TokenType type, string value)
        {
            CurrentToken = NextToken;
            NextToken = new Token(type, value, new Position(currentPos.line, currentPos.column - value.Length + 1));
            if (debugMode)
                Console.WriteLine("{0, -15} {1, -30} {2, 0}", NextToken.Type, NextToken.Value, NextToken.Pos);
        }
        // Read the source code and loads it to the scanner. If file does not exists or empty throws an error
        private void ReadFile()
        {
            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(filename);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundError(filename);
            }
            File = string.Join("\n", lines);
            if (File == "")
            {
                throw new LexicalError(ErrorMessage.LE_SOURCE_EMPTY, new Position(1, 1));
            }
            symbols = new Queue<char>(File);
            fileIsRead = true;
        }
    }
}
