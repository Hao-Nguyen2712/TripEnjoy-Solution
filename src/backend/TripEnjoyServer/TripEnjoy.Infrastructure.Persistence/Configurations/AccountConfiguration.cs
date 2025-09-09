using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            ConfigureAccountTable(builder);
        }

        /// <summary>
        /// Configures the EF Core mapping for the Account aggregate root.
        /// </summary>
        /// <remarks>
        /// Maps Account to the "Accounts" table, configures the primary key (Id) with no DB generation and a value converter
        /// between the AccountId value object and its primitive representation, and configures scalar properties
        /// (AspNetUserId, AccountEmail with max length 256, IsDeleted, CreatedAt, UpdatedAt) as required.
        /// Establishes one-to-one relationships to User, Partner, and Wallet (foreign keys on the related entities),
        /// and one-to-many relationships for RefreshTokens and BlackListTokens (foreign keys on the token entities).
        /// For both token collections, the navigation property access mode is set to Field when available.
        /// </remarks>
        private void ConfigureAccountTable(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(a => a.Id);
            builder.Property(m => m.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => AccountId.Create(value)
            );

            builder.Property(a => a.AspNetUserId).IsRequired();
            builder.Property(a => a.AccountEmail).IsRequired()
                                                 .HasMaxLength(256);
            builder.Property(a => a.IsDeleted).IsRequired();
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt).IsRequired();

            builder.HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<User>(u => u.AccountId);
            builder.HasOne(a => a.Partner)
                .WithOne()
                .HasForeignKey<Partner>(p => p.AccountId);
            builder.HasOne(a => a.Wallet)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.AccountId);

            var navigation = builder.Metadata.FindNavigation(nameof(Account.RefreshTokens));
            navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(a => a.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.AccountId);

            var navigationBlackListTokens = builder.Metadata.FindNavigation(nameof(Account.BlackListTokens));
            navigationBlackListTokens?.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(a => a.BlackListTokens)
                .WithOne()
                .HasForeignKey(bt => bt.AccountId);
        }
    }
}