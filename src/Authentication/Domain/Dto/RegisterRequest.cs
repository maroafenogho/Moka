namespace Moka.src.Authentication.Domain.Dto;

public class RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? MiddleName { get; init; }
    public RegisterProfileRequest? Profile { get; init; }
    public IReadOnlyCollection<RegisterProfileRequest>? Profiles { get; init; }

    public IReadOnlyCollection<RegisterProfileRequest> GetRequestedProfiles()
    {
        if (Profiles is { Count: > 0 })
        {
            return Profiles;
        }

        return Profile is null
            ? []
            : [Profile];
    }
}

public record RegisterProfileRequest(
    string ProfileType,
    string CompanyName);
