
using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuDataManager{


    class Program
    {
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

                string userInput = "";
                try
                {
                    userInput = Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error message: " + e);
                }

                switch (userInput)
                {
                    case "1": 
                        PlayerMenu(); 
                        break;

                    case "2": 
                        PuzzleMenu(); 
                        break;

                    case "4": 
                        exit = true; 
                        break;

                    default: 
                        Console.WriteLine("Invalid option.\n"); 
                        break;
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
            Console.WriteLine();
        }
    }
}
