using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .HasConversion(
                voucherId => voucherId.Value,
                dbValue => VoucherId.Create(dbValue));

        builder.Property(v => v.CreatorId)
            .ValueGeneratedNever()
            .HasConversion(
                creatorId => creatorId.Id,
                dbValue => AccountId.Create(dbValue))
            .IsRequired();

        builder.Property(v => v.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.Code)
            .IsUnique();

        builder.Property(v => v.Description)
            .HasMaxLength(500);

        builder.Property(v => v.DiscountType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(v => v.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(v => v.MaximumDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(v => v.UsageLimit)
            .IsRequired(false);

        builder.Property(v => v.UsageLimitPerUser)
            .IsRequired(false);

        builder.Property(v => v.UsedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(v => v.StartDate)
            .IsRequired();

        builder.Property(v => v.EndDate)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.CreatorType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .IsRequired(false);

        // Relationships
        builder.HasMany(v => v.VoucherTargets)
            .WithOne(vt => vt.Voucher)
            .HasForeignKey(vt => vt.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(v => v.Code);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => new { v.StartDate, v.EndDate });
        builder.HasIndex(v => v.CreatorId);
    }
}
