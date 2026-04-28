using Moka.src.Authentication.Domain.Dto;
using Moka.src.Authentication.Domain.Entities;
using Moka.src.Shared;

namespace Moka.src.Authentication.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<Result<User>> GetUserByEmailAsync(string email);
        Task<Result> AddUserAsync(RegisterRequest user);
    }
}