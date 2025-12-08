using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudokuGameApi.Controllers;
using Sudoku.Api.Data;
using Sudoku.Api.DTO;
using Sudoku.Api.Model;
using System.Security.Claims;

namespace Sudoku.Api.Tests.Controllers
{
    public class GamesControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new GamesController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupUserContext(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnUserGames()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            var game1 = new GameProgress
            {
                UserId = userId,
                Puzzle = "puzzle1",
                Solution = "solution1",
                ElapsedSeconds = 120,
                Difficulty = "easy",
                HintsUsed = 2,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            var game2 = new GameProgress
            {
                UserId = userId,
                Puzzle = "puzzle2",
                Solution = "solution2",
                ElapsedSeconds = 180,
                Difficulty = "medium",
                HintsUsed = 5,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            var otherUserGame = new GameProgress
            {
                UserId = 999,
                Puzzle = "puzzle3",
                Solution = "solution3",
                ElapsedSeconds = 90,
                Difficulty = "easy",
                HintsUsed = 1,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameProgresses.AddRange(game1, game2, otherUserGame);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var games = okResult!.Value as List<GameDto>;
            
            games.Should().NotBeNull();
            games!.Count.Should().Be(2);
            games.Should().AllSatisfy(g => g.Puzzle.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task Create_ShouldSaveGameToDatabase()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            var createDto = new GameCreateDto
            {
                Puzzle = "new-puzzle",
                Solution = "new-solution",
                ElapsedSeconds = 150,
                Difficulty = "hard",
                HintsUsed = 3,
                IsCompleted = true
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            
            var savedGame = await _context.GameProgresses.FirstOrDefaultAsync(g => g.Puzzle == "new-puzzle");
            savedGame.Should().NotBeNull();
            savedGame!.UserId.Should().Be(userId);
            savedGame.Solution.Should().Be("new-solution");
            savedGame.ElapsedSeconds.Should().Be(150);
            savedGame.Difficulty.Should().Be("hard");
            savedGame.HintsUsed.Should().Be(3);
            savedGame.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedGame()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            var createDto = new GameCreateDto
            {
                Puzzle = "test-puzzle",
                Solution = "test-solution",
                ElapsedSeconds = 200,
                Difficulty = "medium",
                HintsUsed = 4,
                IsCompleted = true
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var gameResponse = createdResult!.Value as GameDto;
            
            gameResponse.Should().NotBeNull();
            gameResponse!.Puzzle.Should().Be("test-puzzle");
            gameResponse.Solution.Should().Be("test-solution");
            gameResponse.ElapsedSeconds.Should().Be(200);
            gameResponse.Difficulty.Should().Be("medium");
            gameResponse.HintsUsed.Should().Be(4);
            gameResponse.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Get_ShouldReturnGame_WhenGameExistsAndBelongsToUser()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            var game = new GameProgress
            {
                UserId = userId,
                Puzzle = "puzzle1",
                Solution = "solution1",
                ElapsedSeconds = 100,
                Difficulty = "easy",
                HintsUsed = 1,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameProgresses.Add(game);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Get(game.Id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var gameResponse = okResult!.Value as GameDto;
            
            gameResponse.Should().NotBeNull();
            gameResponse!.Id.Should().Be(game.Id);
            gameResponse.Puzzle.Should().Be("puzzle1");
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            // Act
            var result = await _controller.Get(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenGameBelongsToOtherUser()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId);

            var otherUserGame = new GameProgress
            {
                UserId = 999,
                Puzzle = "other-puzzle",
                Solution = "other-solution",
                ElapsedSeconds = 50,
                Difficulty = "easy",
                HintsUsed = 0,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameProgresses.Add(otherUserGame);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Get(otherUserGame.Id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetGeneralStats_ShouldReturnAggregatedStatistics()
        {
            // Arrange
            var games = new List<GameProgress>
            {
                new GameProgress
                {
                    UserId = 1,
                    Puzzle = "p1",
                    Solution = "s1",
                    ElapsedSeconds = 100,
                    Difficulty = "easy",
                    HintsUsed = 2,
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow
                },
                new GameProgress
                {
                    UserId = 1,
                    Puzzle = "p2",
                    Solution = "s2",
                    ElapsedSeconds = 150,
                    Difficulty = "easy",
                    HintsUsed = 3,
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow
                },
                new GameProgress
                {
                    UserId = 2,
                    Puzzle = "p3",
                    Solution = "s3",
                    ElapsedSeconds = 200,
                    Difficulty = "medium",
                    HintsUsed = 5,
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.GameProgresses.AddRange(games);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetGeneralStats();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }
    }
}
