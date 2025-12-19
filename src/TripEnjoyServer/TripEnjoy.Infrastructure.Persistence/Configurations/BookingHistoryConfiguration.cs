using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class BookingHistoryConfiguration : IEntityTypeConfiguration<BookingHistory>
{
    public void Configure(EntityTypeBuilder<BookingHistory> builder)
    {
        builder.ToTable("BookingHistories");
        
        builder.HasKey(bh => bh.Id);
        
        builder.Property(bh => bh.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => BookingHistoryId.Create(dbValue));

        builder.Property(bh => bh.BookingId)
            .ValueGeneratedNever()
            .HasConversion(
                bookingId => bookingId.Id,
                dbValue => BookingId.Create(dbValue));

        builder.Property(bh => bh.ChangedBy)
            .HasConversion(
                userId => userId != null ? userId.Id : (Guid?)null,
                dbValue => dbValue.HasValue ? UserId.Create(dbValue.Value) : null);

        builder.Property(bh => bh.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(bh => bh.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(bh => bh.ChangedAt)
            .IsRequired();

        // Foreign key relationship is configured in BookingConfiguration
    }
}
