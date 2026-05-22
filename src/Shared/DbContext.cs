using Microsoft.EntityFrameworkCore;
using Moka.src.Authentication.Domain.Entities;
using Moka.src.Brokerage.Domain.Entities;
using Moka.src.Insurance.Domain.Entities;

namespace Moka.src.Shared
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<InsurancePremium> InsurancePremiums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Always include this

            modelBuilder.Entity<User>()
                .HasMany(u => u.Profiles)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .HasPrincipalKey(u => u.UserId);

            modelBuilder.Entity<Profile>()
                .HasIndex(p => new { p.UserId, p.Type })
                .IsUnique();

            modelBuilder.Entity<Profile>()
                .Property(p => p.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Profile>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<InsurancePremium>()
                .HasOne(p => p.BrokerProfile)
                .WithMany()
                .HasForeignKey(p => p.BrokerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InsurancePremium>()
                .HasOne(p => p.UnderwriterProfile)
                .WithMany()
                .HasForeignKey(p => p.UnderwriterProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InsurancePremium>()
                .Property(p => p.Status)
                .HasConversion<string>();
        }
    }
}
