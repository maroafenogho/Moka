namespace Moka.Application.Features.Auth.Register;

// We use 'record' for DTOs because they are immutable and lightweight
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? MiddleName);