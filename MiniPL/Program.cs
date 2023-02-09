/* 
 * 02.02, 2 hours, started implementing scanner
 * 03.02, 1 hour,  started implementing token table
 * 05.02, 6 hours, finished main scanner logic, removed token table, started implementing parser
 * 08.02, 3 hours, started implementing parser, small fixes for scanner
 * 09.02, 2 hours, continued implementing parser
 */
namespace MiniPL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filename = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\MiniPL\\test.mpl";
            Interpreter interpreter = new Interpreter(filename);
            interpreter.Run();
        }
    }
}