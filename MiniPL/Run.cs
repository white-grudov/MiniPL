/* 
 * 02.02, 2 hours, started implementing scanner
 * 03.02, 1 hour,  started implementing token table
 * 05.02, 6 hours, finished main scanner logic, removed token table, started implementing parser
 * 08.02, 3 hours, started implementing parser, small fixes for scanner
 * 09.02, 4 hours, continued implementing parser
 * 11.02, 2 hours, continued implementing parser
 * 12.02, 4 hours, finished parser
 * 22.02, 8 hours, rewrote parser, started implementing semantic analyzer
 * 23.02, 2 hours, continued implementing semantic analyzer
 */
namespace MiniPL
{
    internal class Run
    {
        static void Main(string[] args)
        {
            string filename = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\MiniPL\\test.mpl";
            Main interpreter = new Main(filename, true);
            interpreter.Run();
        }
    }
}