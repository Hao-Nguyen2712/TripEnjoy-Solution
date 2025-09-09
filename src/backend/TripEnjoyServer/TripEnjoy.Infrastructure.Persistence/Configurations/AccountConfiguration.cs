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