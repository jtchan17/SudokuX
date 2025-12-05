using System;

namespace Sudoku.Api.Model
{
    public class GameProgress : GameBaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        // Save the puzzle state (81-char string with '.' for empty)
        public string Puzzle { get; set; }

        // Optional solution snapshot
        public string? Solution { get; set; }

        // Elapsed play time in seconds
        public int ElapsedSeconds { get; set; }

        // Difficulty level (e.g. easy, medium, hard)
        public string? Difficulty { get; set; }

        // Pencil marks or additional metadata stored as JSON (optional)
        public string? Marks { get; set; }

        // Whether the puzzle has been completed
        public bool IsCompleted { get; set; }

        // Last played timestamp
        public DateTime? LastPlayedAt { get; set; }

        // Optional name/description
        public string? Name { get; set; }
    }
}
