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
            parser = new Parser(filename);
            analyzer = new SemanticAnalyzer(parser.Ast);
            interpreter = new Interpreter(parser.Ast);
        }
        public void Run()
        {
            try
            { 
                parser.Parse();
                // if (debugMode) PrintAST(parser.Ast.Root);
                analyzer.Analyze();
                interpreter.Interpret();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void PrintAST(INode node)
        {
            if (node == null) return;
            Console.Write('(');
            foreach (var child in node.GetAllChildren())
            {
                Console.Write($" {child.GetType()},".Replace("MiniPL.", "").Replace("Node", ""));
                if (child.GetAllChildren().Count != 0)
                {
                    PrintAST(child);
                }
                else if (child is TokenNode)
                {
                    Console.Write($" [{((TokenNode)child).GetValue()}],");
                }
            }
            Console.WriteLine("\b)");
        }
    }
}
