using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Functions
{
    public static class SudokuValidator
    {
        public static bool IsNumberInRow(int[,] board, int row, int number)
        {
            for (int col = 0; col < 9; col++)
                if (board[row, col] == number)
                    return true;

            return false;
        }

        public static bool IsNumberInColumn(int[,] board, int col, int number)
        {
            for (int row = 0; row < 9; row++)
                if (board[row, col] == number)
                    return true;

            return false;
        }

        public static bool IsNumberInBox(int[,] board, int row, int col, int number)
        {
            int startRow = row - row % 3;
            int startCol = col - col % 3;

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[startRow + r, startCol + c] == number)
                        return true;

            return false;
        }

        public static bool IsValidMove(int[,] board, int row, int col, int number)
        {
            return !IsNumberInRow(board, row, number) &&
                   !IsNumberInColumn(board, col, number) &&
                   !IsNumberInBox(board, row, col, number);
        }

        public static string GetConflictType(int[,] board, int row, int col, int number)
        {
            if (IsNumberInRow(board, row, number))
                return "Row conflict";

            if (IsNumberInColumn(board, col, number))
                return "Column conflict";

            if (IsNumberInBox(board, row, col, number))
                return "Box conflict";

            return "No conflict";
        }
    }
}