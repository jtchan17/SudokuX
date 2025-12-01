
using System;
using System.Collections.Generic;
using System.Linq;
using SudokuDataManager.Model;

namespace SudokuDataManager{


    class Program
    {
        static List<Player> players = new List<Player>();
        static List<SudokuPuzzle> puzzles = new List<SudokuPuzzle>();
        static Player currentPlayer = null;
        static Random rand = new Random();

        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("=== Sudoku Data Management System ===");
                Console.WriteLine("1. Player Management");
                Console.WriteLine("2. Puzzle Management");
                Console.WriteLine("3. Data Analysis");
                Console.WriteLine("4. Exit");
                Console.Write("Choose: ");

                switch (Console.ReadLine())
                {
                    case "1": PlayerMenu(); break;
                    case "2": PuzzleMenu(); break;
                    case "3": AnalysisService.ShowStatistics(); break;
                    case "4": exit = true; break;
                    default: Console.WriteLine("Invalid option.\n"); break;
                }
            }
        }

        static void PlayerMenu()
        {
            Console.WriteLine("\n=== Player Menu ===");
            Console.WriteLine("1. Add Player");
            Console.WriteLine("2. View Players");
            Console.WriteLine("3. Update Player");
            Console.WriteLine("4. Delete Player");
            Console.WriteLine("5. Back");
            Console.Write("Choose: ");
            Console.WriteLine();

            switch (Console.ReadLine())
            {
                case "1": PlayerCRUD.AddPlayer(); break;
                case "2": PlayerCRUD.ViewPlayers(); break;
                case "3": PlayerCRUD.UpdatePlayer(); break;
                case "4": PlayerCRUD.DeletePlayer(); break;
            }
        }

        static void PuzzleMenu()
        {
            Console.WriteLine("\n=== Puzzle Menu ===");
            Console.WriteLine("1. Add Puzzle");
            Console.WriteLine("2. Generate Random Puzzle");
            Console.WriteLine("3. View Puzzles");
            Console.WriteLine("4. Delete Puzzle");
            Console.WriteLine("5. Back");
            Console.Write("Choose: ");

            switch (Console.ReadLine())
            {
                case "1": PuzzleCRUD.AddPuzzle(); break;
                case "2": PuzzleCRUD.GenerateRandomPuzzle(); break;
                case "3": PuzzleCRUD.ViewPuzzles(); break;
                case "4": PuzzleCRUD.DeletePuzzle(); break;
            }
        }
    }

    // ============================
    // ANALYSIS SERVICE (LINQ)
    // ============================

    public static class AnalysisService
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

    // ============================
    // PUZZLE GENERATOR (RANDOM 9x9)
    // ============================

    public static class PuzzleGenerator
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
