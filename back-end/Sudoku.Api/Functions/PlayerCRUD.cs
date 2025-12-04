using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SudokuDataManager.Model;

namespace SudokuDataManager.Functions
{
    public class PlayerCRUD
    {
        public static List<IUser> Players = new List<IUser>();

        //Create player based on user input (username)
        public static void AddPlayer()
        {
            string username = UserInput.ReadString("Enter username: ");

            Players.Add(new Player
            {
                Id = Players.Count + 1,
                Username = username
            });

            Console.WriteLine("Player added.\n");
        }

        //View all the created players
        public static void ViewPlayers()
        {
            Console.WriteLine("=== Player List ===");
            foreach (var p in Players)
                p.PrintInfo();
            Console.WriteLine();
        }

        //Update player information based on ID
        public static void UpdatePlayer()
        {
            ViewPlayers();
            int id = UserInput.ReadInt("Enter Player ID: ");

            var p = Players.FirstOrDefault(x => x.Id == id);

            if (p == null)
            {
                Console.WriteLine("Player not found.\n");
                return;
            }

            p.Username = UserInput.ReadString("New username: ");
            Console.WriteLine("Player updated.\n");
        }

        //Delete player by ID
        public static void DeletePlayer()
        {
            ViewPlayers();
            int id = UserInput.ReadInt("Enter Player ID to delete: ");

            Players.RemoveAll(x => x.Id == id);
            Console.WriteLine("Player removed.\n");
        }
    }
}