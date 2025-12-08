using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Sudoku.Api.Model;
using Sudoku.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sudoku.Api.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "this-is-a-very-long-secret-key-for-testing-purposes-minimum-32-characters"},
                {"Jwt:Issuer", "test-issuer"},
                {"Jwt:Audience", "test-audience"},
                {"Jwt:ExpireMinutes", "30"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            _tokenService = new TokenService(_configuration);
        }

        [Fact]
        public void CreateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            jwtToken.Should().NotBeNull();
        }

        [Fact]
        public void CreateToken_ShouldIncludeUserIdClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 42,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            userIdClaim.Should().NotBeNull();
            userIdClaim!.Value.Should().Be("42");
        }

        [Fact]
        public void CreateToken_ShouldIncludeUsernameClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "johndoe",
                Role = "user"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            usernameClaim.Should().NotBeNull();
            usernameClaim!.Value.Should().Be("johndoe");
        }

        [Fact]
        public void CreateToken_ShouldIncludeRoleClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                Role = "admin"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            roleClaim.Should().NotBeNull();
            roleClaim!.Value.Should().Be("admin");
        }

        [Fact]
        public void CreateToken_ShouldSetCorrectIssuerAndAudience()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            jwtToken.Issuer.Should().Be("test-issuer");
            jwtToken.Audiences.Should().Contain("test-audience");
        }

        [Fact]
        public void CreateToken_ShouldSetExpirationTime()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "user"
            };

            // Act
            var token = _tokenService.CreateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddDays(8));
        }
    }
}
