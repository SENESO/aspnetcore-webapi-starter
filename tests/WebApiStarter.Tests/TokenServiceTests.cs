using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace WebApiStarter.Tests;

// TokenService reads its signing key from IConfiguration, so the config is
// faked with Moq — no appsettings file needed. Tests prove the token carries
// the right claims and passes real signature validation.
public class TokenServiceTests
{
    // HMAC-SHA256 needs a key of at least 128 bits; 32+ chars keeps it safe.
    private const string TestKey = "test-secret-key-that-is-at-least-32-bytes-long!";

    private static TokenService CreateService(string? key = TestKey)
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Key"]).Returns(key);
        config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        config.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");
        return new TokenService(config.Object);
    }

    [Fact]
    public void GenerateToken_ReturnsTokenWithExpectedClaims()
    {
        var service = CreateService();
        var user = new User { Id = 42, Username = "eslam", Role = "Admin" };

        var (token, expiresAt) = service.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("42", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("eslam", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_TokenPassesSignatureValidation()
    {
        var service = CreateService();
        var (token, _) = service.GenerateToken(new User { Id = 1, Username = "eslam", Role = "User" });

        // Validate exactly the way the API's JWT bearer middleware would.
        var validation = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
            ValidateIssuer = true,
            ValidIssuer = "TestIssuer",
            ValidateAudience = true,
            ValidAudience = "TestAudience",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);

        // Note: ValidateToken remaps JWT claim names to .NET claim types
        // ("sub" -> NameIdentifier, "unique_name" -> Name).
        Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("eslam", principal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void GenerateToken_MissingKey_ThrowsInvalidOperation()
    {
        var service = CreateService(key: null);

        Assert.Throws<InvalidOperationException>(
            () => service.GenerateToken(new User { Id = 1, Username = "eslam" }));
    }

    [Fact]
    public void GenerateToken_ExpiryMatchesConfiguredMinutes()
    {
        var service = CreateService();
        var before = DateTime.UtcNow;

        var (_, expiresAt) = service.GenerateToken(new User { Id = 1, Username = "eslam" });

        // Configured for 60 minutes; allow a small tolerance for execution time.
        Assert.InRange(expiresAt, before.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
    }
}
