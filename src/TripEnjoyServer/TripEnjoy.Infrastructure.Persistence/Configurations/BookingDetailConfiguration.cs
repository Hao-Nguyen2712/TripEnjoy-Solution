using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
{
    public void Configure(EntityTypeBuilder<BookingDetail> builder)
    {
        builder.ToTable("BookingDetails");
        
        builder.HasKey(bd => bd.Id);
        
        builder.Property(bd => bd.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => BookingDetailId.Create(dbValue));

        builder.Property(bd => bd.BookingId)
            .ValueGeneratedNever()
            .HasConversion(
                bookingId => bookingId.Id,
                dbValue => BookingId.Create(dbValue));

        builder.Property(bd => bd.RoomTypeId)
            .ValueGeneratedNever()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.Property(bd => bd.Quantity)
            .IsRequired();

        builder.Property(bd => bd.Nights)
            .IsRequired();

        builder.Property(bd => bd.PricePerNight)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bd => bd.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bd => bd.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        // Foreign key relationship is configured in BookingConfiguration
    }
}
