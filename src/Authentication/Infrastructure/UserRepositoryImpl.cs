using Moka.src.Authentication.Domain.Entities;
using Moka.src.Authentication.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moka.src.Shared;
using Moka.src.Authentication.Domain.Dto;

namespace Moka.src.Authentication.Infrastructure
{
    public class EfUserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Result<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
               .FirstOrDefaultAsync(u => u.Email == email);
            return user != null ? Result<User>.Success(user) : Result<User>.Failure("User not found");
        }

        public async Task<Result> AddUserAsync(RegisterRequest user)
        {
            var userEntity = new User
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = user.Password // In a real app, hash the password before storing
            };
            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}