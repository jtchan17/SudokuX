using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sudoku.Api.Controllers;
using Sudoku.Api.Data;
using Sudoku.Api.DTO;
using Sudoku.Api.Model;
using Sudoku.Api.Services;
using System.Security.Claims;

namespace Sudoku.Api.Tests.Controllers
{
    public class AuthControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _tokenServiceMock = new Mock<ITokenService>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(
                _userServiceMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenServiceMock.Object,
                _loggerMock.Object
            );
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationSuccessful()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "password123"
            };

            _userServiceMock
                .Setup(x => x.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User?)null);

            var newUser = new User
            {
                Id = 1,
                Username = registerRequest.Username,
                Role = "user"
            };

            _userServiceMock
                .Setup(x => x.CreateUserAsync(registerRequest.Username, registerRequest.Password))
                .ReturnsAsync(newUser);

            _tokenServiceMock
                .Setup(x => x.CreateToken(newUser))
                .Returns("fake-jwt-token");

            var refreshToken = new RefreshToken
            {
                Token = "fake-refresh-token",
                UserId = newUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _refreshTokenServiceMock
                .Setup(x => x.CreateRefreshTokenAsync(newUser))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            var response = okResult.Value as AuthResponse;
            response.Should().NotBeNull();
            response!.Token.Should().Be("fake-jwt-token");
            response.RefreshToken.Should().Be("fake-refresh-token");
            response.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Password = "password123"
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "existinguser",
                Role = "user"
            };

            _userServiceMock
                .Setup(x => x.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Username already taken.");
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "validuser",
                Password = "correctpassword"
            };

            var user = new User
            {
                Id = 1,
                Username = loginRequest.Username,
                Role = "user"
            };

            _userServiceMock
                .Setup(x => x.ValidateUserAsync(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.CreateToken(user))
                .Returns("fake-jwt-token");

            var refreshToken = new RefreshToken
            {
                Token = "fake-refresh-token",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _refreshTokenServiceMock
                .Setup(x => x.CreateRefreshTokenAsync(user))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            
            var response = okResult!.Value as AuthResponse;
            response.Should().NotBeNull();
            response!.Token.Should().Be("fake-jwt-token");
            response.RefreshToken.Should().Be("fake-refresh-token");
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "invaliduser",
                Password = "wrongpassword"
            };

            _userServiceMock
                .Setup(x => x.ValidateUserAsync(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnOk_WhenRefreshTokenIsValid()
        {
            // Arrange
            var refreshRequest = new RefreshRequest
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            var existingRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                UserId = user.Id,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _refreshTokenServiceMock
                .Setup(x => x.ValidateRefreshTokenAsync(refreshRequest.RefreshToken))
                .ReturnsAsync(existingRefreshToken);

            _tokenServiceMock
                .Setup(x => x.CreateToken(user))
                .Returns("new-jwt-token");

            var newRefreshToken = new RefreshToken
            {
                Token = "new-refresh-token",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _refreshTokenServiceMock
                .Setup(x => x.CreateRefreshTokenAsync(user))
                .ReturnsAsync(newRefreshToken);

            // Act
            var result = await _controller.Refresh(refreshRequest);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            
            var response = okResult!.Value as AuthResponse;
            response.Should().NotBeNull();
            response!.Token.Should().Be("new-jwt-token");
            response.RefreshToken.Should().Be("new-refresh-token");

            // Verify old token was revoked
            _refreshTokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync(existingRefreshToken), Times.Once);
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshRequest = new RefreshRequest
            {
                RefreshToken = "invalid-refresh-token"
            };

            _refreshTokenServiceMock
                .Setup(x => x.ValidateRefreshTokenAsync(refreshRequest.RefreshToken))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _controller.Refresh(refreshRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
