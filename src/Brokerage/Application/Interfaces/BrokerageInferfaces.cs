using Moka.src.Brokerage.Application.Dtos;
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Shared;

namespace Moka.src.Brokerage.Application.Interfaces
{
    public interface IBrokerageService
    {
        Task<Result> CreateProfileAsync(CreateProfileDto dto);
        Task<Result<Profile>> GetProfileByIdAsync(Guid profileId);
        Task<Result> UpdateProfileStatusAsync(Guid profileId, string status);
        Task<Result<List<Profile>>> GetProfilesAsync();

    }
}