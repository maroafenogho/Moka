using Moka.src.Brokerage.Domain.Entities;

namespace Moka.src.Authentication.Domain.Dto;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? MiddleName,
    Profile? Profile);
