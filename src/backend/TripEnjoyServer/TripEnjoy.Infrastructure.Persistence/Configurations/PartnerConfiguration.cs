using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
    {
        /// <summary>
        /// Configures EF Core mappings for the Partner entity.
        /// </summary>
        /// <remarks>
        /// Maps Partner to the "Partners" table, sets the primary key, disables database-generated Ids,
        /// and applies value conversions and constraints for properties (Id, AccountId, CompanyName,
        /// ContactNumber, Address, and Status) such as required flags and max lengths.
        /// </remarks>
        public void Configure(EntityTypeBuilder<Partner> builder)
        {
            builder.ToTable("Partners");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    partnerId => partnerId.Id,
                    dbValue => PartnerId.Create(dbValue));

            builder.Property(p => p.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id,
                    dbValue => AccountId.Create(dbValue));

            builder.Property(p => p.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ContactNumber)
                .HasMaxLength(20);

            builder.Property(p => p.Address)
                .HasMaxLength(500);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

        }
    }
}