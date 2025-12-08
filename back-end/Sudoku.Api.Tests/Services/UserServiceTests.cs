using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sudoku.Api.Data;
using Sudoku.Api.Model;
using Sudoku.Api.Services;

namespace Sudoku.Api.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _userService = new UserService(_context);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateNewUser()
        {
            // Arrange
            var username = "testuser";
            var password = "testpassword123";

            // Act
            var user = await _userService.CreateUserAsync(username, password);

            // Assert
            user.Should().NotBeNull();
            user.Username.Should().Be(username);
            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.Role.Should().Be("User");
        }

        [Fact]
        public async Task CreateUserAsync_ShouldHashPassword()
        {
            // Arrange
            var username = "testuser";
            var password = "testpassword123";

            // Act
            var user = await _userService.CreateUserAsync(username, password);

            // Assert
            user.PasswordHash.Should().NotBe(password);
            user.PasswordHash.Should().NotBeNullOrEmpty();
            // ASP.NET Identity PasswordHasher format, not BCrypt
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "existinguser";
            await _userService.CreateUserAsync(username, "password");

            // Act
            var result = await _userService.GetByUsernameAsync(username);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.GetByUsernameAsync("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var username = "validuser";
            var password = "correctpassword";
            await _userService.CreateUserAsync(username, password);

            // Act
            var result = await _userService.ValidateUserAsync(username, password);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(username);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "validuser";
            await _userService.CreateUserAsync(username, "correctpassword");

            // Act
            var result = await _userService.ValidateUserAsync(username, "wrongpassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.ValidateUserAsync("nonexistent", "password");

            // Assert
            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
