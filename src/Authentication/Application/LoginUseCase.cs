using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Services;
using Moka.src.Shared;

namespace Moka.src.Authentication.Application;

public class LoginUseCase(AuthenticationService authenticationService)
{
    private readonly AuthenticationService _authenticationService = authenticationService;

    public Task<Result<LoginUserResponse>> ExecuteAsync(LoginRequest request)
        => _authenticationService.LoginAsync(request);
}
