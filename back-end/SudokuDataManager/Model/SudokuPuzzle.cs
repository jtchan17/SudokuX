using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Model
{
    public class SudokuPuzzle : IPuzzle
    {
        public int PuzzleId { get; set; }
        public string Difficulty { get; set; }   // Easy, Medium, Hard
        public bool IsCompleted { get; set; }
        public TimeSpan CompletionTime { get; set; }

        public void PrintInfo()
        {
            Console.WriteLine(
                $"{PuzzleId}. {Difficulty} | Completed: {IsCompleted} | Time: {CompletionTime}"
            );
        }
    }
}