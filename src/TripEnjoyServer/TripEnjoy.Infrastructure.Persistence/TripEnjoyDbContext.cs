using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.Property.Entities;

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
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Domain.PropertyType.PropertyType> PropertyTypes { get; set; } = null!;
        public DbSet<PropertyImage> PropertyImages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
