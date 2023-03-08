namespace MiniPL
{
    internal class MiniPL
    {
        private readonly Parser Parser;
        private readonly SemanticAnalyzer Analyzer;
        private readonly Interpreter Interpreter;

        public MiniPL(string filename, bool debugMode = false)
        { 
            Parser = new Parser(filename, debugMode);
            Analyzer = new SemanticAnalyzer(Parser.Ast);
            Interpreter = new Interpreter(Parser.Ast);
        }
        public void Run()
        {
            try
            {
                Parser.Parse();
                Analyzer.Analyze();
                Interpreter.Interpret();
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

            if (Parser.Scanner.File != null)
            {
                Console.WriteLine();
                string line = Parser.Scanner.File.Split('\n')[e.Pos.line - 1];
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
