using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace WebApiStarter.Tests;

// AuthService depends on IUserRepository, IPasswordHasher and ITokenService.
// Moq fakes all three so these tests run with no database, no crypto, no JWT.
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_users.Object, _hasher.Object, _tokens.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_StoresHashedPasswordAndReturnsToken()
    {
        _users.Setup(u => u.UsernameExistsAsync("eslam", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.EmailExistsAsync("eslam@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("P@ssw0rd")).Returns("hashed-value");
        _tokens.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(("jwt-token", new DateTime(2026, 1, 1)));

        User? savedUser = null;
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => savedUser = u);

        var result = await _service.RegisterAsync(new RegisterDto
        {
            Username = "eslam",
            Email = "eslam@example.com",
            Password = "P@ssw0rd"
        });

        // The plain-text password must never reach the repository.
        Assert.NotNull(savedUser);
        Assert.Equal("hashed-value", savedUser.PasswordHash);
        Assert.Equal("eslam", savedUser.Username);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("eslam", result.Username);
        _users.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsInvalidOperation()
    {
        _users.Setup(u => u.UsernameExistsAsync("eslam", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(new RegisterDto
        {
            Username = "eslam",
            Email = "new@example.com",
            Password = "P@ssw0rd"
        }));

        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        _users.Setup(u => u.UsernameExistsAsync("eslam", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.EmailExistsAsync("taken@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(new RegisterDto
        {
            Username = "eslam",
            Email = "taken@example.com",
            Password = "P@ssw0rd"
        }));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsToken()
    {
        var user = new User { Id = 7, Username = "eslam", PasswordHash = "stored-hash" };
        _users.Setup(u => u.GetByUsernameAsync("eslam", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("P@ssw0rd", "stored-hash")).Returns(true);
        _tokens.Setup(t => t.GenerateToken(user)).Returns(("jwt-token", new DateTime(2026, 1, 1)));

        var result = await _service.LoginAsync(new LoginDto { Username = "eslam", Password = "P@ssw0rd" });

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("eslam", result.Username);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User { Id = 7, Username = "eslam", PasswordHash = "stored-hash" };
        _users.Setup(u => u.GetByUsernameAsync("eslam", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "stored-hash")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginDto { Username = "eslam", Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ThrowsUnauthorized()
    {
        _users.Setup(u => u.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginDto { Username = "ghost", Password = "whatever" }));
    }
}
