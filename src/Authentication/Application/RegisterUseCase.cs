using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Services;
using Moka.src.Shared;

namespace Moka.src.Authentication.Application;

public class RegisterUseCase(AuthenticationService authenticationService)
{
    private readonly AuthenticationService _authenticationService = authenticationService;

    public Task<Result<AuthenticationResponse>> ExecuteAsync(RegisterRequest request)
        => _authenticationService.RegisterAsync(request);
}
