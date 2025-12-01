
using System;
using System.Collections.Generic;
using System.Linq;
using SudokuDataManager.Model;
using SudokuDataManager.Functions;

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
                    case "3": Analysis.ShowStatistics(); break;
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
                case "1": SudokuCRUD.AddPuzzle(); break;
                case "2": SudokuCRUD.GenerateRandomPuzzle(); break;
                case "3": SudokuCRUD.ViewPuzzles(); break;
                case "4": SudokuCRUD.DeletePuzzle(); break;
            }
        }
    }
}
