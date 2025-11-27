using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager
{
    public static class InputHelper
    {
        public static int ReadInt(string prompt)
        {
            int value;

            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (int.TryParse(input, out value))
                    return value;

                Console.WriteLine("Invalid number. Try again.");
            }
        }

        public static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
}