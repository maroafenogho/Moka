namespace Moka.src.Authentication.Domain.Dto;

public record AuthenticationResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    string Token);
