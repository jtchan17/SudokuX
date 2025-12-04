using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Functions
{
    public static class UserInput
    {
        //Read interger from console with prompt
        public static int ReadInt(string prompt)
        {
            int value;

            while (true)
            {
                try
                {
                    Console.Write(prompt);
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out value))
                    {
                        return value;
                    }
                    Console.WriteLine("Invalid number. Try again.");
                    
                }catch(Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        //Read string from console with prompt
        public static string ReadString(string prompt)
        {
            string value = "";
            try
            {
                Console.Write(prompt);
                value = Console.ReadLine();
                
                
            }catch(Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            return value;
        }
        
    }
}