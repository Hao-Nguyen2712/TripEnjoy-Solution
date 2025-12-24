using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;
using TripEnjoy.Domain.Voucher.Entities;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class VoucherTargetConfiguration : IEntityTypeConfiguration<VoucherTarget>
{
    public void Configure(EntityTypeBuilder<VoucherTarget> builder)
    {
        builder.ToTable("VoucherTargets");
        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Id)
            .ValueGeneratedNever()
            .HasConversion(
                voucherTargetId => voucherTargetId.Value,
                dbValue => VoucherTargetId.Create(dbValue));

        builder.Property(vt => vt.VoucherId)
            .ValueGeneratedNever()
            .HasConversion(
                voucherId => voucherId.Value,
                dbValue => VoucherId.Create(dbValue))
            .IsRequired();

        builder.Property(vt => vt.TargetType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(vt => vt.PartnerId)
            .ValueGeneratedNever()
            .HasConversion(
                partnerId => partnerId != null ? partnerId.Id : (Guid?)null,
                dbValue => dbValue.HasValue ? AccountId.Create(dbValue.Value) : null);

        builder.Property(vt => vt.PropertyId)
            .ValueGeneratedNever()
            .HasConversion(
                propertyId => propertyId != null ? propertyId.Id : (Guid?)null,
                dbValue => dbValue.HasValue ? PropertyId.Create(dbValue.Value) : null);

        builder.Property(vt => vt.RoomTypeId)
            .ValueGeneratedNever()
            .HasConversion(
                roomTypeId => roomTypeId != null ? roomTypeId.Value : (Guid?)null,
                dbValue => dbValue.HasValue ? RoomTypeId.Create(dbValue.Value) : null);

        // Indexes for performance
        builder.HasIndex(vt => vt.VoucherId);
        builder.HasIndex(vt => vt.TargetType);
        builder.HasIndex(vt => vt.PartnerId)
            .HasFilter("PartnerId IS NOT NULL");
        builder.HasIndex(vt => vt.PropertyId)
            .HasFilter("PropertyId IS NOT NULL");
        builder.HasIndex(vt => vt.RoomTypeId)
            .HasFilter("RoomTypeId IS NOT NULL");
    }
}
