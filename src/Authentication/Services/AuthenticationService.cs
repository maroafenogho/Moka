using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Domain.Entities;
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Brokerage.Domain.Enums;
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
            _passwordHasher);

        if (userResult.IsFailure || userResult.Data is null)
            return Result<AuthenticationResponse>.Failure(userResult.Error ?? "Could not create user");

        foreach (var profileRequest in request.GetRequestedProfiles())
        {
            if (!Enum.TryParse<ProfileType>(profileRequest.ProfileType, true, out var profileType))
                return Result<AuthenticationResponse>.Failure($"Invalid profile type: {profileRequest.ProfileType}");

            var addProfileResult = userResult.Data.AddProfile(profileType, profileRequest.CompanyName);
            if (addProfileResult.IsFailure)
                return Result<AuthenticationResponse>.Failure(addProfileResult.Error ?? "Could not add profile");
        }

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

    public async Task<Result<UserResponse>> GetUserAsync(Guid userId)
    {
        var userResult = await _userRepository.GetUserByIdAsync(userId);

        if (userResult.IsFailure || userResult.Data is null)
            return Result<UserResponse>.Failure(userResult.Error ?? "User not found");

        return Result<UserResponse>.Success(CreateUserResponse(userResult.Data));
    }

    private AuthenticationResponse CreateResponse(User user)
    {
        return new AuthenticationResponse(
            user.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MiddleName,
            [.. user.Profiles.Select(MapProfile)],
            _jwtService.GenerateToken(user));
    }

    private static UserResponse CreateUserResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MiddleName,
            [.. user.Profiles.Select(MapProfile)]);
    }

    private static ProfileResponse MapProfile(Profile profile)
    {
        return new ProfileResponse(
            profile.Id,
            profile.Type.ToString(),
            profile.Status.ToString(),
            profile.CompanyName,
            profile.CreatedAt);
    }
}
