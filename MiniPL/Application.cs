namespace MiniPL
{
    internal class Application
    {
        private readonly Parser parser;
        private readonly string filename;
        private readonly SemanticAnalyzer analyzer;
        private readonly Interpreter interpreter;

        public Application(string filename, bool debugMode = false)
        {
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
                analyzer.Analyze();
                interpreter.Interpret();
            }
            catch (MiniPLException e)
            {
                PrintError(e);
            }
            catch (ErrorList e)
            {
                foreach (var error in e.Errors)
                {
                    PrintError(error);
                }
            }
        }
        private void PrintError(MiniPLException e)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(e.Message);
            Console.ResetColor();
            if (e is FileNotFoundError) return;

            if (parser.Scanner.File != null)
            {
                Console.WriteLine();
                string line = parser.Scanner.File.Split('\n')[e.Pos.line - 1];
                int indent = 0;
                foreach (var ch in line)
                {
                    if (ch == '\t' || ch == ' ')
                    {
                        indent++;
                        line = line[1..];
                    }
                    else break;
                }
                Console.WriteLine(line);
                Console.WriteLine($"{new string(' ', e.Pos.column - indent - 1)}^");
            }
        }
    }
}
