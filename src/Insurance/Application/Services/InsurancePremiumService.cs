using Microsoft.EntityFrameworkCore;
using Moka.src.Brokerage.Domain.Enums;
using Moka.src.Insurance.Application.Dtos;
using Moka.src.Insurance.Application.Interfaces;
using Moka.src.Insurance.Domain.Entities;
using Moka.src.Insurance.Domain.Enums;
using Moka.src.Shared;

namespace Moka.src.Insurance.Application.Services
{
    public class InsurancePremiumService(AppDbContext context) : IInsurancePremiumService
    {
        private readonly AppDbContext _context = context;

        public async Task<Result<InsurancePremiumResponseDto>> CreatePremiumAsync(CreateInsurancePremiumDto dto)
        {
            if (dto.Amount <= 0)
                return Result<InsurancePremiumResponseDto>.Failure("Amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(dto.PolicyType))
                return Result<InsurancePremiumResponseDto>.Failure("Policy type is required");

            var brokerProfile = await _context.Profiles
                .FirstOrDefaultAsync(profile => profile.Id == dto.BrokerProfileId);

            if (brokerProfile == null)
                return Result<InsurancePremiumResponseDto>.Failure("Broker profile not found");

            if (!brokerProfile.IsBroker())
                return Result<InsurancePremiumResponseDto>.Failure("Profile is not a broker");

            if (!brokerProfile.IsActive())
                return Result<InsurancePremiumResponseDto>.Failure("Broker profile is not active");

            var underwriterProfile = await _context.Profiles
                .FirstOrDefaultAsync(profile => profile.Id == dto.UnderwriterProfileId);

            if (underwriterProfile == null)
                return Result<InsurancePremiumResponseDto>.Failure("Underwriter profile not found");

            if (!underwriterProfile.IsUnderWriter())
                return Result<InsurancePremiumResponseDto>.Failure("Profile is not an underwriter");

            if (!underwriterProfile.IsActive())
                return Result<InsurancePremiumResponseDto>.Failure("Underwriter profile is not active");

            var premium = new InsurancePremium
            {
                BrokerProfileId = brokerProfile.Id,
                UnderwriterProfileId = underwriterProfile.Id,
                Amount = dto.Amount,
                PolicyType = dto.PolicyType.Trim(),
                Status = PremiumStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.InsurancePremiums.Add(premium);
            await _context.SaveChangesAsync();

            premium.BrokerProfile = brokerProfile;
            premium.UnderwriterProfile = underwriterProfile;

            return Result<InsurancePremiumResponseDto>.Success(ToResponse(premium));
        }

        public async Task<Result<InsurancePremiumResponseDto>> UpdatePremiumAsync(Guid userId, Guid premiumId, UpdateInsurancePremiumDto dto)
        {
            if (dto.Amount.HasValue && dto.Amount.Value <= 0)
                return Result<InsurancePremiumResponseDto>.Failure("Amount must be greater than zero");

            if (dto.PolicyType != null && string.IsNullOrWhiteSpace(dto.PolicyType))
                return Result<InsurancePremiumResponseDto>.Failure("Policy type cannot be empty");

            PremiumStatus? status = null;
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!Enum.TryParse<PremiumStatus>(dto.Status, true, out var parsedStatus))
                    return Result<InsurancePremiumResponseDto>.Failure($"Invalid premium status: {dto.Status}");

                status = parsedStatus;
            }

            if (!dto.Amount.HasValue && dto.PolicyType == null && status == null)
                return Result<InsurancePremiumResponseDto>.Failure("No update values were provided");

            var profiles = await _context.Profiles
                .Where(profile => profile.UserId == userId && profile.Status == ProfileStatus.Active)
                .ToListAsync();

            if (profiles.Count == 0)
            {
                var hasInactiveProfiles = await _context.Profiles
                    .AnyAsync(profile => profile.UserId == userId && profile.Status != ProfileStatus.Active);

                if (hasInactiveProfiles)
                    return Result<InsurancePremiumResponseDto>.Failure("No active profile found for this user. Inactive profiles exist, please activate a profile to proceed.");
                else
                    return Result<InsurancePremiumResponseDto>.Failure("No profile found for this user.");
            }

            var isAdmin = profiles.Any(profile => profile.Type == ProfileType.Admin);
            var brokerProfileIds = profiles
                .Where(profile => profile.Type == ProfileType.Broker)
                .Select(profile => profile.Id)
                .ToList();

            var underwriterProfileIds = profiles
                .Where(profile => profile.Type == ProfileType.Underwriter)
                .Select(profile => profile.Id)
                .ToList();

            var premium = await _context.InsurancePremiums
                .Include(premium => premium.BrokerProfile)
                .Include(premium => premium.UnderwriterProfile)
                .FirstOrDefaultAsync(premium => premium.Id == premiumId);

            if (premium == null)
                return Result<InsurancePremiumResponseDto>.Failure("Premium not found");

            var isOwner = brokerProfileIds.Contains(premium.BrokerProfileId) || underwriterProfileIds.Contains(premium.UnderwriterProfileId);
            if (!isAdmin && !isOwner)
            {
                return Result<InsurancePremiumResponseDto>.Failure("User is not authorized to update this premium");
            }

            if (dto.Amount.HasValue)
                premium.Amount = dto.Amount.Value;

            if (dto.PolicyType != null)
                premium.PolicyType = dto.PolicyType.Trim();

            if (status.HasValue)
                premium.Status = status.Value;

            await _context.SaveChangesAsync();

            return Result<InsurancePremiumResponseDto>.Success(ToResponse(premium));
        }

        public async Task<Result<List<InsurancePremiumResponseDto>>> GetPremiumsAsync(Guid userId, GetInsurancePremiumsQueryDto query)
        {
            if (query.MinAmount.HasValue && query.MinAmount.Value < 0)
                return Result<List<InsurancePremiumResponseDto>>.Failure("Minimum amount cannot be negative");

            if (query.MaxAmount.HasValue && query.MaxAmount.Value < 0)
                return Result<List<InsurancePremiumResponseDto>>.Failure("Maximum amount cannot be negative");

            if (query.MinAmount.HasValue && query.MaxAmount.HasValue && query.MinAmount.Value > query.MaxAmount.Value)
                return Result<List<InsurancePremiumResponseDto>>.Failure("Minimum amount cannot be greater than maximum amount");

            if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
                return Result<List<InsurancePremiumResponseDto>>.Failure("From date cannot be later than to date");

            PremiumStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (!Enum.TryParse<PremiumStatus>(query.Status, true, out var parsedStatus))
                    return Result<List<InsurancePremiumResponseDto>>.Failure($"Invalid premium status: {query.Status}");

                status = parsedStatus;
            }

            ProfileType? profileType = null;
            if (!string.IsNullOrWhiteSpace(query.ProfileType))
            {
                if (!Enum.TryParse<ProfileType>(query.ProfileType, true, out var parsedProfileType))
                    return Result<List<InsurancePremiumResponseDto>>.Failure($"Invalid profile type: {query.ProfileType}");

                if (parsedProfileType == ProfileType.Admin)
                    return Result<List<InsurancePremiumResponseDto>>.Failure("Profile type filter must be Broker or Underwriter");

                profileType = parsedProfileType;
            }

            var profiles = await _context.Profiles
                .Where(profile => profile.UserId == userId && profile.Status == ProfileStatus.Active)
                .ToListAsync();

            if (profiles.Count == 0)
            {
                var hasInactiveProfiles = await _context.Profiles
                    .AnyAsync(profile => profile.UserId == userId && profile.Status != ProfileStatus.Active);

                if (hasInactiveProfiles)
                    return Result<List<InsurancePremiumResponseDto>>.Failure("No active profile found for this user. Inactive profiles exist, please activate a profile to proceed.");
                else
                    return Result<List<InsurancePremiumResponseDto>>.Failure("No profile found for this user.");
            }

            var isAdmin = profiles.Any(profile => profile.Type == ProfileType.Admin);
            var brokerProfileIds = profiles
                .Where(profile => profile.Type == ProfileType.Broker)
                .Select(profile => profile.Id)
                .ToList();

            var underwriterProfileIds = profiles
                .Where(profile => profile.Type == ProfileType.Underwriter)
                .Select(profile => profile.Id)
                .ToList();

            var premiumQuery = _context.InsurancePremiums
                .Where(premium => true); // Start with all premiums, filter below

            if (!isAdmin)
            {
                premiumQuery = premiumQuery.Where(premium =>
                    brokerProfileIds.Contains(premium.BrokerProfileId) ||
                    underwriterProfileIds.Contains(premium.UnderwriterProfileId));
            }

            if (profileType == ProfileType.Broker)
                premiumQuery = premiumQuery.Where(premium => brokerProfileIds.Contains(premium.BrokerProfileId));

            if (profileType == ProfileType.Underwriter)
                premiumQuery = premiumQuery.Where(premium => underwriterProfileIds.Contains(premium.UnderwriterProfileId));

            if (query.BrokerProfileId.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.BrokerProfileId == query.BrokerProfileId.Value);

            if (query.UnderwriterProfileId.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.UnderwriterProfileId == query.UnderwriterProfileId.Value);

            if (status.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(query.PolicyType))
                if (!string.IsNullOrWhiteSpace(query.PolicyType))
                    premiumQuery = premiumQuery.Where(premium =>
                        premium.PolicyType.ToLower().Trim() == query.PolicyType.ToLower().Trim());
            if (query.MinAmount.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.Amount >= query.MinAmount.Value);

            if (query.MaxAmount.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.Amount <= query.MaxAmount.Value);

            if (query.FromDate.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.CreatedAt >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                premiumQuery = premiumQuery.Where(premium => premium.CreatedAt <= query.ToDate.Value);

            var premiums = await premiumQuery
                .OrderByDescending(premium => premium.CreatedAt)
                .Select(premium => new InsurancePremiumResponseDto
                {
                    Id = premium.Id,
                    BrokerProfileId = premium.BrokerProfileId,
                    BrokerCompanyName = premium.BrokerProfile.CompanyName,
                    UnderwriterProfileId = premium.UnderwriterProfileId,
                    UnderwriterCompanyName = premium.UnderwriterProfile.CompanyName,
                    Amount = premium.Amount,
                    PolicyType = premium.PolicyType,
                    Status = premium.Status,
                    CreatedAt = premium.CreatedAt
                })
                .ToListAsync();

            return Result<List<InsurancePremiumResponseDto>>.Success(premiums);
        }

        private static InsurancePremiumResponseDto ToResponse(InsurancePremium premium)
        {
            return new InsurancePremiumResponseDto
            {
                Id = premium.Id,
                BrokerProfileId = premium.BrokerProfileId,
                BrokerCompanyName = premium.BrokerProfile.CompanyName,
                UnderwriterProfileId = premium.UnderwriterProfileId,
                UnderwriterCompanyName = premium.UnderwriterProfile.CompanyName,
                Amount = premium.Amount,
                PolicyType = premium.PolicyType,
                Status = premium.Status,
                CreatedAt = premium.CreatedAt
            };
        }
    }
}
