using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.ToTable("Settlements");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasConversion(
                settlementId => settlementId.Id,
                dbValue => SettlementId.Create(dbValue));

        builder.Property(s => s.WalletId)
            .IsRequired()
            .HasConversion(
                walletId => walletId.Id,
                dbValue => WalletId.Create(dbValue));

        builder.HasOne(s => s.Wallet)
            .WithMany(w => w.Settlements)
            .HasForeignKey(s => s.WalletId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.PeriodStart)
            .IsRequired();

        builder.Property(s => s.PeriodEnd)
            .IsRequired();

        builder.Property(s => s.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.CommissionAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.NetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.PaidAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(s => s.WalletId);
        builder.HasIndex(s => new { s.WalletId, s.PeriodStart, s.PeriodEnd });
        builder.HasIndex(s => new { s.Status, s.CreatedAt });
    }
}
