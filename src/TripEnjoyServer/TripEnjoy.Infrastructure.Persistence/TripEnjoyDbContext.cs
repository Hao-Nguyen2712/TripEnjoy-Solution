using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.Property.Entities;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.Entities;

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
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Settlement> Settlements { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<BlackListToken> BlackListTokens { get; set; } = null!;
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Domain.PropertyType.PropertyType> PropertyTypes { get; set; } = null!;
        public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<RoomTypeImage> RoomTypeImages { get; set; } = null!;
        public DbSet<RoomAvailability> RoomAvailabilities { get; set; } = null!;
        public DbSet<RoomPromotion> RoomPromotions { get; set; } = null!;
        
        // Booking Aggregate
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingDetail> BookingDetails { get; set; } = null!;
        public DbSet<BookingHistory> BookingHistories { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
