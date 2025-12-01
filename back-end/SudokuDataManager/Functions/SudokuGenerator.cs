using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Functions
{
    public static class SudokuGenerator
    {
        public static string GeneratePuzzle()
        {
            Random r = new Random();
            char[] grid = new char[81];

            for (int i = 0; i < 81; i++)
            {
                int num = r.Next(0, 10);
                grid[i] = num == 0 ? '.' : num.ToString()[0];
            }

            return new string(grid);
        }
    }
}