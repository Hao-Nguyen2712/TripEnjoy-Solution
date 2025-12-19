using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Booking.Enums;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => PaymentId.Create(dbValue));

        builder.Property(p => p.BookingId)
            .ValueGeneratedNever()
            .HasConversion(
                bookingId => bookingId.Id,
                dbValue => BookingId.Create(dbValue));

        builder.HasOne(p => p.Booking)
            .WithMany()
            .HasForeignKey(p => p.BookingId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PaidAt)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
