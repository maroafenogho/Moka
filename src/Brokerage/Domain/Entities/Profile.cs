using Moka.src.Authentication.Domain.Entities;
using Moka.src.Brokerage.Domain.Enums;

namespace Moka.src.Brokerage.Domain.Entities
{
    public class Profile
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public ProfileType Type { get; set; }
        public ProfileStatus Status { get; set; } = ProfileStatus.Active;
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsBroker() => Type == ProfileType.Broker;
        public bool IsUnderWriter() => Type == ProfileType.Underwriter;
        public bool IsActive() => Status == ProfileStatus.Active;
    }
}