using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
    public async Task<IActionResult> GetUserAsync([FromRoute] Guid userId)
    {
        Console.WriteLine($"Token user ID: {userId}");
        var tokenUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (!Guid.TryParse(tokenUserId, out var authenticatedUserId))
            return Unauthorized();

        if (authenticatedUserId != userId)
            return Forbid();

        var result = await _authenticationService.GetUserAsync(userId);
        return result.ToActionResult();
    }
}
