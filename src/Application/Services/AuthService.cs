using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

// Registration + login logic. Throws InvalidOperationException on bad input
// and UnauthorizedAccessException on bad credentials — the global exception
// middleware maps these to 400 and 401 responses.
public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await _users.UsernameExistsAsync(dto.Username, ct))
            throw new InvalidOperationException("Username is already taken.");

        if (await _users.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password)
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        var (token, expiresAt) = _tokens.GenerateToken(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expiresAt, Username = user.Username };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(dto.Username, ct);
        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        var (token, expiresAt) = _tokens.GenerateToken(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expiresAt, Username = user.Username };
    }
}
