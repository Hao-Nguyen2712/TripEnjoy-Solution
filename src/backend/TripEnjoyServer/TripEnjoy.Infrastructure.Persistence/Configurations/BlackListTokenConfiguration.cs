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