using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        /// <summary>
        /// Configures the EF Core mapping for the RefreshToken entity.
        /// </summary>
        /// <remarks>
        /// Maps to the "RefreshTokens" table, sets Id as the primary key (no database-generated values) with custom conversions for the value objects
        /// RefreshTokenId and AccountId. Enforces Token as required with a maximum length of 500, marks ExpireDate and CreatedAt as required,
        /// leaves RevokeAt optional, and ignores the IsUsed domain property for persistence.
        /// </remarks>
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    refreshTokenId => refreshTokenId.Id,
                    dbValue => RefreshTokenId.Create(dbValue));

            builder.Property(rt => rt.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id,
                    dbValue => AccountId.Create(dbValue));

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.ExpireDate).IsRequired();
            builder.Property(rt => rt.CreatedAt).IsRequired();
            builder.Property(rt => rt.RevokeAt).IsRequired(false);

            builder.Ignore(rt => rt.IsUsed);

        }
    }
}