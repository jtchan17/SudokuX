using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sudoku.Api.Data;
using Sudoku.Api.Model;
using Sudoku.Api.Services;

namespace Sudoku.Api.Tests.Services
{
    public class RefreshTokenServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;

        public RefreshTokenServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:RefreshExpireMinutes", "10080"} // 7 days in minutes
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            _refreshTokenService = new RefreshTokenService(_context, _configuration);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldCreateNewToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.UserId.Should().Be(user.Id);
            refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            refreshToken.RevokedAt.Should().BeNull();
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldSaveTokenToDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);

            // Assert
            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken.Token);
            savedToken.Should().NotBeNull();
            savedToken!.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldGenerateUniqueTokens()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var token1 = await _refreshTokenService.CreateRefreshTokenAsync(user);
            var token2 = await _refreshTokenService.CreateRefreshTokenAsync(user);

            // Assert
            token1.Token.Should().NotBe(token2.Token);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnToken_WhenTokenIsValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "dummy-hash",
                Role = "user"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);

            // Act
            var validatedToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken.Token);

            // Assert
            validatedToken.Should().NotBeNull();
            validatedToken!.Token.Should().Be(refreshToken.Token);
            validatedToken.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            var nonExistentToken = "invalid-token-12345";

            // Act
            var validatedToken = await _refreshTokenService.ValidateRefreshTokenAsync(nonExistentToken);

            // Assert
            validatedToken.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenIsRevoked()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "dummy-hash",
                Role = "user"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);
            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);

            // Act
            var validatedToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken.Token);

            // Assert
            validatedToken.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenIsExpired()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "dummy-hash",
                Role = "user"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var expiredToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                RevokedAt = null
            };
            _context.RefreshTokens.Add(expiredToken);
            await _context.SaveChangesAsync();

            // Act
            var validatedToken = await _refreshTokenService.ValidateRefreshTokenAsync(expiredToken.Token);

            // Assert
            validatedToken.Should().BeNull();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ShouldRevokeToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "dummy-hash",
                Role = "user"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);

            // Act
            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);

            // Assert
            var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken.Token);
            revokedToken.Should().NotBeNull();
            revokedToken!.RevokedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ShouldNotThrow_WhenTokenDoesNotExist()
        {
            // Arrange
            var nonExistentToken = new RefreshToken
            {
                Token = "non-existent-token",
                UserId = 999,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            // Act
            Func<Task> act = async () => await _refreshTokenService.RevokeRefreshTokenAsync(nonExistentToken);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
