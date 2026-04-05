using Moka.src.Authentication.Entities;

namespace Moka.src.Authentication.Domain.Interfaces;


public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}