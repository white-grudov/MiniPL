namespace MiniPLTests
{
    [TestClass]
    public class SemanticTest
    {
        private readonly string prefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\SemanticTest\\";
        private readonly string validPrefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ValidPrograms\\";

        [DataRow("var_declaration.mpl", "a", "b", "str", "i")]
        [TestMethod]
        // Check if the declared variables are present in the table
        public void Analyze_VariableDeclaration_VariablesInTable(string path, params string[] variables)
        {
            Parser parser = new(prefix + path, false);
            parser.Parse();
            SemanticAnalyzer analyzer = new(parser.Ast);

            Context context = Context.GetInstance();
            analyzer.Analyze();

            foreach (var variable in variables)
            {
                Assert.IsTrue(context.ContainsVariable(variable));
            }
            context.ClearTable();
        }
        [DataRow("declared_twice.mpl", ErrorMessage.SE_VAR_DECLARED)]
        [DataRow("undeclared_var.mpl", ErrorMessage.SE_VAR_NOT_DECLARED)]
        [DataRow("type_mismatch.mpl",  ErrorMessage.SE_VAR_TYPE_DISMATCH)]
        [TestMethod]
        /* Check if semantic analyzer throws an error if variable is declared twice, variable is used
         * when undeclared and types do not match
         */
        public void Analyze_SemanticErroreousProgram_ThrowSemanticError(string path, string error)
        {
            Parser parser = new(prefix + path, false);
            parser.Parse();
            SemanticAnalyzer analyzer = new(parser.Ast);

            try
            {
                analyzer.Analyze();
            }
            catch (ErrorList errorList)
            {
                foreach (var e in errorList.Errors)
                {
                    StringAssert.Contains(e.Message, error);
                }
            }
            Context.GetInstance().ClearTable();
        }
        [DataRow("1.mpl")]
        [DataRow("2.mpl")]
        [DataRow("3.mpl")]
        [TestMethod]
        // Check if analysis is performed correctly on valid programs
        public void Analyze_ValidPrograms_AnalysisPerformedCorrectly(string path)
        {
            Parser parser = new(validPrefix + path, false);
            parser.Parse();
            SemanticAnalyzer analyzer = new(parser.Ast);

            Context context = Context.GetInstance();
            analyzer.Analyze();

            Assert.AreNotEqual(context.Table.Count, 0);
            context.ClearTable();
        }
    }
}
