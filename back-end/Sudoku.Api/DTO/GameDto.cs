namespace Sudoku.Api.DTO
{
    public class GameCreateDto
    {
        public string Puzzle { get; set; }
        public string? Solution { get; set; }
        public int ElapsedSeconds { get; set; }
        public string? Name { get; set; }
        public string? Difficulty { get; set; }
        public string? Marks { get; set; }
        public bool IsCompleted { get; set; }
        public System.DateTime? LastPlayedAt { get; set; }
        public int HintsUsed { get; set; }
    }

    public class GameDto : GameCreateDto
    {
        public int Id { get; set; }
    }
}
