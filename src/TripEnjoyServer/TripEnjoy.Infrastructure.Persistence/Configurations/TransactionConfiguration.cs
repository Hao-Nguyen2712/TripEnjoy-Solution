using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .HasConversion(
                transactionId => transactionId.Id,
                dbValue => TransactionId.Create(dbValue));

        builder.Property(t => t.WalletId)
            .IsRequired()
            .HasConversion(
                walletId => walletId.Id,
                dbValue => WalletId.Create(dbValue));

        builder.Property(t => t.BookingId)
            .IsRequired(false)
            .HasConversion(
                bookingId => bookingId!.Id,
                dbValue => BookingId.Create(dbValue));

        builder.HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CompletedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.BookingId);
        builder.HasIndex(t => new { t.WalletId, t.CreatedAt });
        builder.HasIndex(t => new { t.Status, t.CreatedAt });
    }
}
