using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Authentication.Domain.Entities;

namespace Moka.src.Authentication.Services;

public class JwtService(IConfiguration config, ILogger<JwtService> logger) : IJwtService
{
    private const int MinimumHmacSha256KeyBytes = 32;
    private readonly IConfiguration _config = config;
    private readonly ILogger<JwtService> _logger = logger;

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = GetSigningKey();

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateToken(string token)
    {
        return ValidateTokenWithDetails(token).UserId;
    }

    public JwtValidationResult ValidateTokenWithDetails(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSigningKey(),
            ValidateIssuer = !string.IsNullOrWhiteSpace(_config["Jwt:Issuer"]),
            ValidIssuer = _config["Jwt:Issuer"],
            ValidateAudience = !string.IsNullOrWhiteSpace(_config["Jwt:Audience"]),
            ValidAudience = _config["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                const string error = "Token validated, but no valid user id claim was found.";
                _logger.LogWarning(error);
                return new JwtValidationResult(false, null, error);
            }

            return new JwtValidationResult(true, parsedUserId, null);
        }
        catch (Exception exception)
        {
            var error = $"{exception.GetType().Name}: {exception.Message}";
            _logger.LogWarning(exception, "JWT validation failed: {Reason}", error);
            return new JwtValidationResult(false, null, error);
        }
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString))
        {
            throw new InvalidOperationException("Jwt:Key is not configured.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        if (keyBytes.Length < MinimumHmacSha256KeyBytes)
        {
            throw new InvalidOperationException(
                $"Jwt:Key must be at least {MinimumHmacSha256KeyBytes} bytes for HS256; current key is {keyBytes.Length} bytes.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }
}
