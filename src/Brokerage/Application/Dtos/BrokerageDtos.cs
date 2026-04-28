
namespace Moka.src.Brokerage.Application.Dtos
{
    public class CreateProfileDto
    {
        public required string UserId { get; set; }
        public required string CompanyName { get; set; }
        public required string ProfileType { get; set; }
    }
}