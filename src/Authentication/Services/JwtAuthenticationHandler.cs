using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Moka.src.Authentication.Domain.Interfaces;

namespace Moka.src.Authentication.Services;

public class JwtAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IJwtService jwtService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "MokaJwt";

    private readonly ILogger<JwtAuthenticationHandler> _logger = logger.CreateLogger<JwtAuthenticationHandler>();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            _logger.LogInformation("Authorization header was not provided.");
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        const string bearerPrefix = "Bearer ";
        if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Authorization header must use the Bearer scheme.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header."));
        }

        var token = authorizationHeader[bearerPrefix.Length..].Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogInformation("Bearer token was empty.");
            return Task.FromResult(AuthenticateResult.Fail("Bearer token was empty."));
        }

        LogTokenMetadata(token);

        var validationResult = jwtService.ValidateTokenWithDetails(token);
        if (!validationResult.IsValid || validationResult.UserId is null)
        {
            _logger.LogWarning(
                "Bearer token could not be validated. Reason: {Reason}",
                validationResult.Error ?? "Unknown validation failure.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, validationResult.UserId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, validationResult.UserId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private void LogTokenMetadata(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            _logger.LogInformation(
                "Bearer token metadata: issuer={Issuer}, audience={Audience}, expiresUtc={ExpiresUtc}.",
                jwt.Issuer,
                string.Join(",", jwt.Audiences),
                jwt.ValidTo);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Bearer token could not be read as a JWT.");
        }
    }
}
