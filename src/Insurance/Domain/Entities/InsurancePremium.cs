
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Insurance.Domain.Enums;
using Moka.src.Shared;

namespace Moka.src.Insurance.Domain.Entities
{
    public class InsurancePremium : Entity
    {

        public int BrokerProfileId { get; set; }
        public Profile BrokerProfile { get; set; }

        public int UnderwriterProfileId { get; set; }
        public Profile UnderwriterProfile { get; set; }
        public decimal Amount { get; set; }
        public string PolicyType { get; set; }

        public PremiumStatus Status { get; set; } = PremiumStatus.Pending;

        public DateTime CreatedAt { get; set; }
    }
}