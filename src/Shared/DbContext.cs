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
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId);

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
        }
    }
}