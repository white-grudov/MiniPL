namespace MiniPL
{
    internal class Application
    {
        private readonly Parser parser;
        private readonly string filename;
        private readonly SemanticAnalyzer analyzer;
        private readonly Interpreter interpreter;

        private bool debugMode = false;

        public Application(string filename, bool debugMode = false)
        {
            this.debugMode = debugMode;

            this.filename = filename;
            parser = new Parser(filename, debugMode);
            analyzer = new SemanticAnalyzer(parser.Ast);
            interpreter = new Interpreter(parser.Ast);
        }
        public void Run()
        {
            try
            { 
                parser.Parse();
                if (debugMode) parser.Ast.Root.Print();
                analyzer.Analyze();
                interpreter.Interpret();
            }
            catch (MiniPLException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
