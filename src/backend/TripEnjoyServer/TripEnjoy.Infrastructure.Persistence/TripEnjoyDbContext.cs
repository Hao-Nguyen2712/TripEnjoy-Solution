using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;

namespace TripEnjoy.Infrastructure.Persistence
{
    public class TripEnjoyDbContext : IdentityDbContext<ApplicationUser>
    {
        public TripEnjoyDbContext(DbContextOptions<TripEnjoyDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Partner> Partners { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<BlackListToken> BlackListTokens { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
