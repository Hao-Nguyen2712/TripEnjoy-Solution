using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Dòng này rất quan trọng, nó sẽ tự động tìm và áp dụng
            // tất cả các lớp IEntityTypeConfiguration trong assembly này.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
