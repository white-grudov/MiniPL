using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniPL
{
    internal class Interpreter
    {
        Parser parser;
        string filename;

        public Interpreter(string filename)
        {
            this.filename = filename;
            parser = new Parser(filename);
        }
        public void Run()
        {
            try
            {
                parser.GenerateTokens();
                parser.Parse();
            }
            catch (Exception e)
            {
                PrintError(e);
            }
        }
        private void PrintError(Exception e)
        {
            Console.WriteLine($"File {filename}.\n{e.Message}:");

            int[] numbers = GetNumbers(e.Message);
            int line = numbers[1];
            int column = numbers[2];

            if (parser.Scanner.file == null) return;
            string[] lines = parser.Scanner.file.Split('\n');
            string currentLine = lines[line - 1];

            foreach (char ch in currentLine) if (ch == '\t') column += 7;
            Console.WriteLine(currentLine);

            for (int i = 0; i < column - 1; ++i) Console.Write(' ');
            Console.WriteLine('^');
        }
        private int[] GetNumbers(string message)
        { 
            string[] numbers = Regex.Split(message, @"\D+");
            int[] result = new int[numbers.Length];
            for (int i = 0; i < numbers.Length; ++i)
            {
                if (!string.IsNullOrEmpty(numbers[i]))
                {
                    int num = int.Parse(numbers[i]);
                    result[i] = num;
                }
            }
            return result;
        }
    }
}
