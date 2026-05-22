namespace Moka.src.Authentication.Domain.Dto;

public record AuthenticationResponse(
    int Id,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    IReadOnlyCollection<ProfileResponse> Profiles,
    string Token);

public record LoginUserResponse(string Token);

public record UserResponse(
    int Id,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    IReadOnlyCollection<ProfileResponse> Profiles);

public record ProfileResponse(
    int ProfileId,
    string ProfileType,
    string Status,
    string? CompanyName,
    DateTime CreatedAt);
