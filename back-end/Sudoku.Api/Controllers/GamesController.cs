using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sudoku.Api.Data;
using Sudoku.Api.DTO;
using Sudoku.Api.Model;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SudokuGameApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GamesController(AppDbContext db)
        {
            _db = db;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uid = GetUserId();
            var games = await _db.GameProgresses
                .Where(g => g.UserId == uid)
                .Select(g => new GameDto { Id = g.Id, Puzzle = g.Puzzle, Solution = g.Solution, ElapsedSeconds = g.ElapsedSeconds, Name = g.Name, Difficulty = g.Difficulty, Marks = g.Marks, IsCompleted = g.IsCompleted, LastPlayedAt = g.LastPlayedAt, HintsUsed = g.HintsUsed })
                .ToListAsync();
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var uid = GetUserId();
            var g = await _db.GameProgresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (g == null) return NotFound();
            return Ok(new GameDto { Id = g.Id, Puzzle = g.Puzzle, Solution = g.Solution, ElapsedSeconds = g.ElapsedSeconds, Name = g.Name, Difficulty = g.Difficulty, Marks = g.Marks, IsCompleted = g.IsCompleted, LastPlayedAt = g.LastPlayedAt, HintsUsed = g.HintsUsed });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GameCreateDto dto)
        {
            var uid = GetUserId();
            var g = new GameProgress { UserId = uid, Puzzle = dto.Puzzle, Solution = dto.Solution, ElapsedSeconds = dto.ElapsedSeconds, Name = dto.Name, Difficulty = dto.Difficulty, Marks = dto.Marks, IsCompleted = dto.IsCompleted, LastPlayedAt = dto.LastPlayedAt, HintsUsed = dto.HintsUsed };
            _db.GameProgresses.Add(g);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = g.Id }, new GameDto { Id = g.Id, Puzzle = g.Puzzle, Solution = g.Solution, ElapsedSeconds = g.ElapsedSeconds, Name = g.Name, Difficulty = g.Difficulty, Marks = g.Marks, IsCompleted = g.IsCompleted, LastPlayedAt = g.LastPlayedAt, HintsUsed = g.HintsUsed });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GameCreateDto dto)
        {
            var uid = GetUserId();
            var g = await _db.GameProgresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (g == null) return NotFound();
            g.Puzzle = dto.Puzzle;
            g.Solution = dto.Solution;
            g.ElapsedSeconds = dto.ElapsedSeconds;
            g.Name = dto.Name;
            g.Difficulty = dto.Difficulty;
            g.Marks = dto.Marks;
            g.IsCompleted = dto.IsCompleted;
            g.LastPlayedAt = dto.LastPlayedAt;
            g.HintsUsed = dto.HintsUsed;
            g.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = GetUserId();
            var g = await _db.GameProgresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (g == null) return NotFound();
            _db.GameProgresses.Remove(g);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("stats/general")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGeneralStats()
        {
            var completedGames = await _db.GameProgresses
                .Where(g => g.IsCompleted)
                .Select(g => new { g.Difficulty, g.ElapsedSeconds, g.HintsUsed })
                .ToListAsync();

            var stats = completedGames
                .GroupBy(g => g.Difficulty ?? "easy")
                .Select(group => new
                {
                    difficulty = group.Key,
                    totalGames = group.Count(),
                    bestTime = group.Min(g => g.ElapsedSeconds),
                    avgTime = (int)group.Average(g => g.ElapsedSeconds),
                    totalHints = group.Sum(g => g.HintsUsed),
                    avgHints = group.Average(g => g.HintsUsed)
                });

            return Ok(stats);
        }
    }
}
