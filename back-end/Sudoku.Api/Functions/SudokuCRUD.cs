using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SudokuDataManager.Model;

namespace SudokuDataManager.Functions
{
    public class SudokuCRUD
    {
        public static List<IPuzzle> Puzzles = new List<IPuzzle>();
        static Random rand = new Random();

        //Create puzzle based on user input
        public static void AddPuzzle()
        {
            string diff = UserInput.ReadString("Difficulty (Easy/Medium/Hard): ");
            string grid = UserInput.ReadString("Enter puzzle grid (81 chars): ");

            Puzzles.Add(new SudokuPuzzle
            {
                PuzzleId = Puzzles.Count + 1,
                Difficulty = diff,
                GridData = grid,
            });

            Console.WriteLine("Puzzle added.\n");
        }

        //Generate random puzzle based on different difficulty levels
        public static void GenerateRandomPuzzle()
        {
            string[] difficulties = { "Easy", "Medium", "Hard" };
            string grid = SudokuGenerator.GeneratePuzzle();
            var puzzle = new SudokuPuzzle
            {
                PuzzleId = Puzzles.Count + 1,
                Difficulty = difficulties[rand.Next(3)],
                GridData = grid,
                IsCompleted = rand.Next(2) == 1,
                CompletionTime = TimeSpan.FromMinutes(rand.Next(5, 45))
            };

            Puzzles.Add(puzzle);
            Console.WriteLine("Random puzzle generated.\n");
        }

        //View all the created puzzles
        public static void ViewPuzzles()
        {
            Console.WriteLine("=== Puzzle List ===");
            foreach (var p in Puzzles)
                p.PrintInfo();
            Console.WriteLine();
        }

        //Get specific puzzle by ID
        public static SudokuPuzzle GetSudokuPuzzle(int id)
        {
            return Puzzles.Where(p => p is SudokuPuzzle).Cast<SudokuPuzzle>().FirstOrDefault(p => p.PuzzleId == id);
        }

        //Delete puzzle by ID
        public static void DeletePuzzle()
        {
            ViewPuzzles();
            int id = UserInput.ReadInt("Enter Puzzle ID: ");

            Puzzles.RemoveAll(p => ((SudokuPuzzle)p).PuzzleId == id);
            Console.WriteLine("Puzzle deleted.\n");
        }
    }
}