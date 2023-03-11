namespace MiniPLTests
{
    [TestClass]
    public class ScannerTest
    {
        private readonly string prefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ScannerTest\\";
        private readonly string validPrefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ValidPrograms\\";

        // Checks if error is thrown when file does not exists
        [DataRow("non_existing_file.mpl")]
        [TestMethod]
        public void ReadFile_NonExistingFile_ThrowFileNotFoundError(string path)
        {
            Scanner scanner = new($"{prefix}{path}", false);

            Assert.ThrowsException<FileNotFoundError>(() => scanner.Tokenize());
        }
        // Checks if all possible valid tokens can be generated
        [DataRow("all_valid_tokens.mpl")]
        [TestMethod]
        public void Tokenize_FileWithAllTokens_AllTokensRecognized(string path)
        {
            Scanner scanner = new($"{prefix}{path}", false);

            TokenType[] expectedTokenTypes = ((TokenType[])Enum.GetValues(typeof(TokenType)))[..^1];
            TokenType[] generatedTokenTypes = scanner.GenerateTokens().Select(token => token.Type).ToArray();

            CollectionAssert.AreEqual(expectedTokenTypes, generatedTokenTypes);
        }
        // Checks if all escape characters inside string literal can be recognized
        [DataRow("escape_char_string.mpl")]
        [TestMethod]
        public void Tokenize_StringWithEscapeChars_ProcessCorrectly(string path)
        {
            Scanner scanner = new($"{prefix}{path}", false);

            string expected = "\n\t'\"\\\r\f\v";

            scanner.Tokenize();
            string generated = scanner.NextToken.Value[1..^1];

            Assert.AreEqual(expected, generated);
        }
        // Checks if comments and blank spaces are properly ignored in scanning
        [DataRow("spaces_and_comments.mpl")]
        [TestMethod]
        public void Tokenize_BlankSpacesAndComments_Ignore(string path)
        {
            Scanner scanner = new($"{prefix}{path}", false);

            List<TokenType> expected = new() 
            {
                TokenType.VAR, TokenType.IDENTIFIER, TokenType.COLON, TokenType.INT,
                TokenType.ASSIGN, TokenType.INT_LITERAL, TokenType.SEMICOLON, TokenType.EOF
            };
            List<TokenType> generated = scanner.GenerateTokens().Select(token => token.Type).ToList();

            CollectionAssert.AreEqual(expected, generated);
        }
        // Checks if corresponding error message is thrown when encountering an illegal token
        [DataRow("empty_file.mpl",           ErrorMessage.LE_SOURCE_EMPTY       )]
        [DataRow("invalid_character.mpl",    ErrorMessage.LE_INVALID_CHAR       )]
        [DataRow("unterminated_string.mpl",  ErrorMessage.LE_UNTERMINATED_STR   )]
        [DataRow("invalid_escape_seq.mpl",   ErrorMessage.LE_UNRECOGNIZED_ESCAPE)]
        [DataRow("illegal_ident.mpl",        ErrorMessage.LE_ILLEGAL_CHAR_SEQ   )]
        [DataRow("illegal_number.mpl",       ErrorMessage.LE_ILLEGAL_CHAR_SEQ   )]
        [DataRow("unterminated_comment.mpl", ErrorMessage.LE_UNENCLOSED_COMMENT )]
        [DataRow("invalid_range.mpl",        ErrorMessage.LE_RANGE_EXPECTED     )]
        [TestMethod]
        public void Tokenize_IncorrectProgram_ThrowLexicalError(string path, string errorType)
        {
            Scanner scanner = new($"{prefix}{path}", false);

            try
            {
                scanner.GenerateTokens();
            }
            catch (LexicalError e)
            {
                StringAssert.Contains(e.Message, errorType);
            }
        }
        [DataRow("1.mpl")]
        [DataRow("2.mpl")]
        [DataRow("3.mpl")]
        [DataRow("4.mpl")]
        [DataRow("5.mpl")]
        [TestMethod]
        public void Tokenize_ValidPrograms_GenerateTokens(string path)
        {
            Scanner scanner = new(validPrefix + path, false);

            var tokens = scanner.GenerateTokens();

            Assert.AreNotEqual(tokens.Count, 0);
        }
    }
}