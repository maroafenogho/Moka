namespace Moka.src.Authentication.Domain.Dto;

public record LoginRequest(
    string Email,
    string Password);
