/* 
 * 02.02, 2 hours, started implementing scanner
 * 03.02, 1 hour,  started implementing token table
 * 05.02, 5 hours, finished main scanner logic, still not sure about table
 */
namespace MiniPL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filename = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\MiniPL\\test.mpl";
            Scanner scanner = new Scanner(filename);
        }
    }
}