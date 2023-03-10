using System.Text;

namespace MiniPLTests
{
    [TestClass]
    public class ParserTest
    {
        private readonly string prefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ParserTest\\";
        private readonly string validPrefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ValidPrograms\\";

        [DataRow("declaration_statements.mpl")]
        [DataRow("assignment_statements.mpl")]
        [DataRow("read_statements.mpl")]
        [DataRow("print_statements.mpl")]
        [DataRow("for_statements.mpl")]
        [DataRow("if_statements.mpl")]
        [TestMethod]
        public void Parse_ValidStatements_BuildAST(string path)
        {
            Parser parser = new(prefix + path, false);

            parser.Parse();

            Assert.IsNotNull(parser.Ast.Root.Stmts);
        }
        [DataRow("illegal_statement_start.mpl", ErrorMessage.SE_ILLEGAL_TOKEN)]
        [DataRow("missing_semicolon.mpl", ErrorMessage.SE_MISSING_SEMICOLON)]
        [DataRow("unexpected_tokens.mpl", ErrorMessage.SE_UNEXPECTED_TOKEN)]
        [DataRow("various_errors.mpl", SyntaxError.type)]
        [TestMethod]
        public void Parse_IllegalToken_ThrowSyntaxError(string path, string error)
        {
            Parser parser = new(prefix + path, false);

            try
            {
                parser.Parse();
            }
            catch (ErrorList errorList)
            {
                foreach (var e in errorList.Errors)
                {
                    Assert.IsInstanceOfType(e, typeof(SyntaxError));
                    StringAssert.Contains(e.Message, error);
                }
            }
        }
        [DataRow("1.mpl")]
        [DataRow("2.mpl")]
        [DataRow("3.mpl")]
        [TestMethod]
        public void Parse_ValidPrograms_BuildAST(string path)
        {
            Parser parser = new (validPrefix + path, false);

            parser.Parse();

            Assert.IsNotNull(parser.Ast.Root.Stmts);
        }
    }
}
