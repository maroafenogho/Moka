using Moka.src.Insurance.Application.Dtos;
using Moka.src.Shared;

namespace Moka.src.Insurance.Application.Interfaces
{
    public interface IInsurancePremiumService
    {
        Task<Result<InsurancePremiumResponseDto>> CreatePremiumAsync(CreateInsurancePremiumDto dto);
        Task<Result<List<InsurancePremiumResponseDto>>> GetPremiumsAsync(Guid userId, GetInsurancePremiumsQueryDto query);
        Task<Result<InsurancePremiumResponseDto>> UpdatePremiumAsync(Guid userId, Guid premiumId, UpdateInsurancePremiumDto dto);
    }
}
