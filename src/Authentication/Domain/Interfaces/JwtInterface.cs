using Moka.src.Authentication.Domain.Entities;

namespace Moka.src.Authentication.Domain.Interfaces;


public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
    JwtValidationResult ValidateTokenWithDetails(string token);
}

public record JwtValidationResult(
    bool IsValid,
    Guid? UserId,
    string? Error);
