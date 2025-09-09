using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;

namespace TripEnjoy.Infrastructure.Persistence
{
    public class TripEnjoyDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TripEnjoyDbContext"/> using the provided EF Core options.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{TripEnjoyDbContext}"/> used to configure the context (connection, provider, etc.).</param>
        public TripEnjoyDbContext(DbContextOptions<TripEnjoyDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Partner> Partners { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<BlackListToken> BlackListTokens { get; set; } = null!;
        /// <summary>
        /// Configures the EF Core model for this context by applying all IEntityTypeConfiguration implementations
        /// found in the executing assembly, then invokes the base implementation to apply Identity-related mappings.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> used to build the EF Core model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
