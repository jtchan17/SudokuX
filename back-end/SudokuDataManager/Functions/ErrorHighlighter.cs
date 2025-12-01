using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Functions
{
    public class ErrorHighlighter
    {
        public static void Highlight(int[,] board, int row, int col, int number)
        {
            Console.WriteLine("\n=== Conflict Highlight ===");

            for (int r = 0; r < 9; r++)
            {
                if (r % 3 == 0) Console.WriteLine("+-------+-------+-------+");

                for (int c = 0; c < 9; c++)
                {
                    if (c % 3 == 0) Console.Write("| ");

                    if (r == row && c == col)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("X ");
                        Console.ResetColor();
                    }
                    else if (board[r, c] == number &&
                             (r == row ||
                              c == col ||
                              (r / 3 == row / 3 && c / 3 == col / 3)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(board[r, c] + " ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(board[r, c] == 0 ? ". " : board[r, c] + " ");
                    }
                }

                Console.WriteLine("|");
            }

            Console.WriteLine("+-------+-------+-------+\n");
        }
    }
}