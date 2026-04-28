
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Shared;

namespace Moka.src.Authentication.Domain.Entities
{
    public class User : Entity
    {
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Profile? Profile { get; set; }

        private User() { }

        public static Result<User> Create(string email, string password, string firstName, string lastName, string? middleName, IPasswordHasher hasher)
        {
            if (hasher is null)
                return Result<User>.Failure("Password service unavailable");

            email = email.Trim().ToLowerInvariant();
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            middleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();

            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Failure("Email required");

            if (string.IsNullOrWhiteSpace(firstName))
                return Result<User>.Failure("First name required");

            if (string.IsNullOrWhiteSpace(lastName))
                return Result<User>.Failure("Last name required");

            if (password.Length < 6)
                return Result<User>.Failure("Password too short");

            var user = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                CreatedAt = DateTime.UtcNow,
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
