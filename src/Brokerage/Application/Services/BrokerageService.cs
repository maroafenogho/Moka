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
            var exists = await _context.Profiles
               .AnyAsync(x => x.UserId.ToString() == dto.UserId);

            if (exists)
                return Result.Failure("User already has a profile");

            var profile = new Profile
            {
                UserId = Guid.Parse(dto.UserId),
                Type = Enum.Parse<ProfileType>(dto.ProfileType),
                CompanyName = dto.CompanyName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<Profile>> GetProfileByIdAsync(Guid profileId)
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

        public async Task<Result> UpdateProfileStatusAsync(Guid profileId, string status)
        {
            var profile = await _context.Profiles
               .FirstOrDefaultAsync(p => p.Id == profileId);

            if (profile == null)
                return Result.Failure("Profile not found");

            if (!Enum.TryParse<ProfileStatus>(status, out var result))
            {
                return Result.Failure("Invalid status value: ${result}");
            }

            profile.Status = Enum.Parse<ProfileStatus>(status);
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}