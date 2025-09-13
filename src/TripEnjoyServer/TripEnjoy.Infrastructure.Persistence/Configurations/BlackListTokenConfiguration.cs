using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class BlackListTokenConfiguration : IEntityTypeConfiguration<BlackListToken>
    {
        /// <summary>
        /// Configures the EF Core mapping for the BlackListToken entity.
        /// </summary>
        /// <remarks>
        /// Maps the entity to the "BlackListTokens" table, sets the primary key, and configures column conversions and constraints:
        /// - Id: not database-generated; persisted using BlackListTokenId.Id and reconstructed via BlackListTokenId.Create(...).
        /// - AccountId: required; persisted using AccountId.Id and reconstructed via AccountId.Create(...).
        /// - Token: required; maximum length 500.
        /// - Expiration: required.
        /// - CreatedAt: required.
        /// </remarks>
        public void Configure(EntityTypeBuilder<BlackListToken> builder)
        {
            builder.ToTable("BlackListTokens");

            builder.HasKey(bt => bt.Id);

            builder.Property(bt => bt.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    blackListTokenId => blackListTokenId.Id,
                    dbValue => BlackListTokenId.Create(dbValue));

            builder.Property(bt => bt.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id,
                    dbValue => AccountId.Create(dbValue));

            builder.Property(bt => bt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(bt => bt.Expiration)
                .IsRequired();

            builder.Property(bt => bt.CreatedAt)
                .IsRequired();
        }
    }
}