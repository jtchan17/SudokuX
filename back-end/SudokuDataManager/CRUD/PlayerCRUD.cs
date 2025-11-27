using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager
{
    public class PlayerCRUD
    {
        public static List<IUser> Players = new List<IUser>();

        public static void AddPlayer()
        {
            string username = InputHelper.ReadString("Enter username: ");

            Players.Add(new Player
            {
                Id = Players.Count + 1,
                Username = username
            });

            Console.WriteLine("Player added.\n");
        }

        public static void ViewPlayers()
        {
            Console.WriteLine("=== Player List ===");
            foreach (var p in Players)
                p.PrintInfo();
            Console.WriteLine();
        }

        public static void UpdatePlayer()
        {
            ViewPlayers();
            int id = InputHelper.ReadInt("Enter Player ID: ");

            var p = Players.FirstOrDefault(x => x.Id == id);

            if (p == null)
            {
                Console.WriteLine("Player not found.\n");
                return;
            }

            p.Username = InputHelper.ReadString("New username: ");
            Console.WriteLine("Player updated.\n");
        }

        public static void DeletePlayer()
        {
            ViewPlayers();
            int id = InputHelper.ReadInt("Enter Player ID to delete: ");

            Players.RemoveAll(x => x.Id == id);
            Console.WriteLine("Player removed.\n");
        }
    }
}