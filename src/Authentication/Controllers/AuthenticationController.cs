using Microsoft.AspNetCore.Mvc;
using Moka.src.Authentication.Application;
using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Services;
using Moka.src.Shared;

namespace Moka.src.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(
    RegisterUseCase registerUseCase,
    LoginUseCase loginUseCase,
    AuthenticationService authenticationService) : ControllerBase
{
    private readonly RegisterUseCase _registerUseCase = registerUseCase;
    private readonly LoginUseCase _loginUseCase = loginUseCase;
    private readonly AuthenticationService _authenticationService = authenticationService;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var result = await _registerUseCase.ExecuteAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await _loginUseCase.ExecuteAsync(request);
        return result.ToActionResult();
    }

    [HttpGet("users/{userId:guid}")]
    [HttpGet("get-user/{userId:guid}")]
    public async Task<IActionResult> GetUserAsync([FromRoute] Guid userId)
    {
        var result = await _authenticationService.GetUserAsync(userId);
        return result.ToActionResult();
    }
}
