using System.Collections.Generic;

namespace Sudoku.Api.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        // simple role support
        public string Role { get; set; } = "User";
        public ICollection<GameProgress> Games { get; set; } = new List<GameProgress>();
    }
}
