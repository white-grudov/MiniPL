namespace MiniPL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool debugMode = false;
            if (args.Length < 1) return; // if no path to file specified
            else if (args.Length == 2) // debug mode flag
            {
                if (args[1] == "-debug") debugMode = true;
            }
            MiniPL interpreter = new(args[0], debugMode);
            interpreter.Run();
        }
    }
}