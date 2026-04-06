
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Shared;

namespace Moka.src.Authentication.Domain.Entities
{
    public class User : Entity
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? MiddleName { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private User()
        {
            Email = string.Empty;
            PasswordHash = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            CreatedAt = DateTime.UtcNow;
            MiddleName = null;
        }

        public static Result<User> Create(string email, string password, IPasswordHasher hasher)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Failure("Email required");

            if (password.Length < 6)
                return Result<User>.Failure("Password too short");

            var user = new User
            {
                Email = email,
                PasswordHash = hasher.Hash(password)
            };

            return Result<User>.Success(user);
        }

        public bool VerifyPassword(string password, IPasswordHasher hasher)
        {
            return hasher.Verify(password, PasswordHash);
        }
    }
}