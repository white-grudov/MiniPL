using System.Text;

namespace MiniPLTests
{
    [TestClass]
    public class InterpreterTest
    {
        private readonly string prefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\InterpreterTest\\";
        private readonly string validPrefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ValidPrograms\\";

        private readonly StringBuilder ConsoleOutput = new();

        [TestInitialize]
        // Set console output
        public void Init()
        {
            Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }
        [DataRow("ariphmetics.mpl", -4, 15, 6, 14, 9)]
        [TestMethod]
        // Check if interpreter evaluates ariphmetic operations correctly
        public void Interpret_AriphmeticOperations_ProduceCorrectResult(string path, params int[] results)
        {
            Parser parser = new(prefix + path, false);
            SemanticAnalyzer analyzer = new(parser.Ast);
            Interpreter interpreter = new(parser.Ast);

            parser.Parse();
            analyzer.Analyze();
            interpreter.Interpret();

            Context context = Context.GetInstance();
            List<int> intValues = context.Table.Values.Select(x => (int)x.Value).ToList();
            var expectedGenerated = results.Zip(intValues, (e, g) => new { Expected = e, Generated = g });

            foreach (var eg in expectedGenerated)
            {
                Assert.AreEqual(eg.Expected, eg.Generated);
            }
            context.ClearTable();
        }
        [DataRow("var_declaration.mpl", "1", "test", "4", "hello world")]
        [DataRow("for_if_test.mpl", "10", "5", "25")]
        [TestMethod]
        // Check if interpreter assigns right values to variables
        public void Interpret_VariableDeclarationAndAssignment_CorrectResult(string path, params string[] results)
        {
            Parser parser = new(prefix + path, false);
            SemanticAnalyzer analyzer = new(parser.Ast);
            Interpreter interpreter = new(parser.Ast);

            parser.Parse();
            analyzer.Analyze();
            interpreter.Interpret();

            Context context = Context.GetInstance();
            List<string> values = context.Table.Values.Select(x => x.Value.ToString()).ToList();
            var expectedGenerated = results.Zip(values, (e, g) => new { Expected = e, Generated = g });

            foreach (var eg in expectedGenerated)
            {
                Assert.AreEqual(eg.Expected, eg.Generated);
            }
            context.ClearTable();
        }
        [DataRow("div_by_zero.mpl", ErrorMessage.RE_DIVISION_BY_ZERO)]
        [DataRow("uninitialized_var.mpl", ErrorMessage.RE_UNINITIALIZED_VAR)]
        [TestMethod]
        // Check if interpreter throws an error when division by zero or usage of uninitialized variable
        public void Interpret_IllegalVariableUsage_ThrowRuntimeError(string path, string error)
        {
            Parser parser = new(prefix + path, false);
            SemanticAnalyzer analyzer = new(parser.Ast);
            Interpreter interpreter = new(parser.Ast);

            parser.Parse();
            analyzer.Analyze();

            try
            {
                interpreter.Interpret();
            }
            catch (RuntimeError e)
            {
                StringAssert.Contains(e.Message, error);
            }
            Context.GetInstance().ClearTable();
        }
        [DataRow("input_not_int.mpl", ErrorMessage.RE_CAST_TO_INT, "test")]
        [DataRow("input_not_int.mpl", ErrorMessage.RE_CAST_TO_INT, "\"1\"")]
        [DataRow("input_not_int.mpl", ErrorMessage.RE_CAST_TO_INT, "42.0")]
        [TestMethod]
        // Check if interpreter throws an error when invalid read input to int variable
        public void Interpret_InputNonIntValue_ThrowRuntimeError(string path, string error, string input)
        {
            Parser parser = new(prefix + path, false);
            SemanticAnalyzer analyzer = new(parser.Ast);
            Interpreter interpreter = new(parser.Ast);

            parser.Parse();
            analyzer.Analyze();
            Console.SetIn(new StringReader(input));

            try
            {
                interpreter.Interpret();
            }
            catch (RuntimeError e)
            {
                StringAssert.Contains(e.Message, error);
            }
            Context.GetInstance().ClearTable();
        }
        [DataRow("1.mpl", "", "16")]
        [DataRow("2.mpl", "1", "0 : Hello, World!")]
        [DataRow("3.mpl", "5", "The result is: 120")]
        [DataRow("4.mpl", "15", "610")]
        [DataRow("5.mpl", "20", "The sum of 20 numbers is: 210")]
        [TestMethod]
        // Check if valid programs are executed with correct input/output
        public void Interpret_ValidPrograms_ExecuteProgram(string path, string input, string expected)
        {
            Parser parser = new(validPrefix + path, false);
            SemanticAnalyzer analyzer = new(parser.Ast);
            Interpreter interpreter = new(parser.Ast);

            parser.Parse();
            analyzer.Analyze();
            Context context = Context.GetInstance();

            Console.SetIn(new StringReader(input));
            interpreter.Interpret();
            StringAssert.Contains(ConsoleOutput.ToString(), expected);
            
            context.ClearTable();
        }
    }
}
