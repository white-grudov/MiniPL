﻿/* 
 * 02.02, 2 hours, started implementing scanner
 * 03.02, 1 hour,  started implementing token table
 * 05.02, 6 hours, finished main scanner logic, removed token table, started implementing parser
 * 08.02, 3 hours, started implementing parser, small fixes for scanner
 * 09.02, 4 hours, continued implementing parser
 * 11.02, 2 hours, continued implementing parser
 * 12.02, 4 hours, finished parser
 * 22.02, 8 hours, rewrote parser, started implementing semantic analyzer
 * 23.02, 2 hours, continued implementing semantic analyzer
 * 02.03, 7 hours, fixed token generation, finished semantic analyzer
 * 03.03, 3 hours, implemented interpreter
 * 04.03, 2 hours, small fixes
 * 05.03, 1 hour,  started redoing error handling
 * 06.03, 1 hour,  ...
 */

/* What to do:
 * - More robust error handling
 * - Testing
 */
namespace MiniPL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) return;
            Application app = new(args[0], true);
            app.Run();
        }
    }
}