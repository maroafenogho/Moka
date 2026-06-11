using System.ComponentModel.DataAnnotations;
using Moka.src.Insurance.Domain.Enums;

namespace Moka.src.Insurance.Application.Dtos
{
    public class CreateInsurancePremiumDto
    {
        public int BrokerProfileId { get; set; }
        public int UnderwriterProfileId { get; set; }

        //Inline validation (There is another called fluent validation, but this is simpler for now)
        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }
        public required string PolicyType { get; set; }
    }

    public class UpdateInsurancePremiumDto
    {
        public decimal? Amount { get; set; }
        public string? PolicyType { get; set; }
        public string? Status { get; set; }
    }

    public class GetInsurancePremiumsQueryDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? PolicyType { get; set; }
        public decimal? Amount { get; set; }
        public int? BrokerProfileId { get; set; }
        public int? UnderwriterProfileId { get; set; }
        public string? ProfileType { get; set; }
    }

    public class InsurancePremiumResponseDto
    {
        public int Id { get; set; }
        public int BrokerProfileId { get; set; }
        public string? BrokerCompanyName { get; set; }
        public int UnderwriterProfileId { get; set; }
        public string? UnderwriterCompanyName { get; set; }
        public decimal Amount { get; set; }
        public string PolicyType { get; set; } = string.Empty;
        public PremiumStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
