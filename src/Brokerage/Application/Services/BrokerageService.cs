using Microsoft.EntityFrameworkCore;
using Moka.src.Brokerage.Application.Dtos;
using Moka.src.Brokerage.Application.Interfaces;
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Brokerage.Domain.Enums;
using Moka.src.Shared;

namespace Moka.src.Brokerage.Application.Services
{
    public class BrokerageService(AppDbContext context) : IBrokerageService
    {
        private readonly AppDbContext _context = context;

        public async Task<Result> CreateProfileAsync(CreateProfileDto dto)
        {
            if (!Guid.TryParse(dto.UserId, out var userId))
                return Result.Failure("Invalid user id");

            if (!Enum.TryParse<ProfileType>(dto.ProfileType, true, out var profileType))
                return Result.Failure($"Invalid profile type: {dto.ProfileType}");

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return Result.Failure("Company name required");

            var userExists = await _context.Users
               .AnyAsync(user => user.UserId == userId);

            if (!userExists)
                return Result.Failure("User not found");

            var exists = await _context.Profiles
               .AnyAsync(profile => profile.UserId == userId && profile.Type == profileType);

            if (exists)
                return Result.Failure($"User already has a {profileType} profile");

            var profile = new Profile
            {
                UserId = userId,
                Type = profileType,
                CompanyName = dto.CompanyName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<Profile>> GetProfileByIdAsync(int profileId)
        {
            var profile = await _context.Profiles
               .FirstOrDefaultAsync(p => p.Id == profileId);

            return profile != null ? Result<Profile>.Success(profile) : Result<Profile>.Failure("Profile not found");
        }

        public async Task<Result<List<Profile>>> GetProfilesAsync()
        {
            var profiles = await _context.Profiles.ToListAsync();
            return Result<List<Profile>>.Success(profiles);
        }

        public async Task<Result> UpdateProfileStatusAsync(int profileId, string status)
        {
            var profile = await _context.Profiles
               .FirstOrDefaultAsync(p => p.Id == profileId);

            if (profile == null)
                return Result.Failure("Profile not found");

            if (!Enum.TryParse<ProfileStatus>(status, true, out var result))
            {
                return Result.Failure($"Invalid status value: {status}");
            }

            profile.Status = result;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}
