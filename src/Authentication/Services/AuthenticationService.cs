using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Domain.Entities;
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Shared;

namespace Moka.src.Authentication.Services;

public class AuthenticationService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetUserByEmailAsync(email);

        if (existingUser.IsSuccess)
            return Result<AuthenticationResponse>.Failure("User with this email already exists");

        var userResult = User.Create(
               email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            _passwordHasher, request.Profile);

        if (userResult.IsFailure || userResult.Data is null)
            return Result<AuthenticationResponse>.Failure(userResult.Error ?? "Could not create user");

        var saveResult = await _userRepository.AddUserAsync(userResult.Data);
        if (saveResult.IsFailure)
            return Result<AuthenticationResponse>.Failure(saveResult.Error ?? "Could not save user");

        return Result<AuthenticationResponse>.Success(CreateResponse(userResult.Data));
    }

    public async Task<Result<LoginUserResponse>> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userResult = await _userRepository.GetUserByEmailAsync(email);

        if (userResult.IsFailure || userResult.Data is null)
            return Result<LoginUserResponse>.Failure("Invalid email or password");

        var user = userResult.Data;
        if (!user.VerifyPassword(request.Password, _passwordHasher))
            return Result<LoginUserResponse>.Failure("Invalid email or password");

        return Result<LoginUserResponse>.Success(new LoginUserResponse(_jwtService.GenerateToken(user)));
    }

    private AuthenticationResponse CreateResponse(User user)
    {
        return new AuthenticationResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MiddleName,
            _jwtService.GenerateToken(user));
    }
}
