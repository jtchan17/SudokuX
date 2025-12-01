using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Functions
{
    public static class Analysis
    {
        public static void ShowStatistics()
        {
            Console.WriteLine("=== Game Statistics ===");

            var completed = PuzzleCRUD.Puzzles.Where(p => p.IsCompleted);

            Console.WriteLine($"Total Puzzles: {PuzzleCRUD.Puzzles.Count}");
            Console.WriteLine($"Completed Puzzles: {completed.Count()}");

            if (completed.Any())
            {
                double avg = completed.Average(p => p.CompletionTime.TotalMinutes);
                Console.WriteLine($"Average Completion Time: {avg:F2} mins");
            }

            var byDifficulty = PuzzleCRUD.Puzzles
                .GroupBy(p => p.Difficulty)
                .Select(g => new { Difficulty = g.Key, Count = g.Count() });

            Console.WriteLine("\nPuzzles by Difficulty:");
            foreach (var item in byDifficulty)
                Console.WriteLine($"{item.Difficulty}: {item.Count}");

            Console.WriteLine();
        }
    }
}