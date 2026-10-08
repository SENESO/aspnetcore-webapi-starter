using Domain.Entities;

namespace Application.Interfaces;

// Issues JWTs. Implemented in Infrastructure (it owns the signing-key config).
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
