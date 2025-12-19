using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Id)
            .ValueGeneratedNever()
            .HasConversion(
                bookingId => bookingId.Id,
                dbValue => BookingId.Create(dbValue));

        builder.Property(b => b.UserId)
            .ValueGeneratedNever()
            .HasConversion(
                userId => userId.Id,
                dbValue => UserId.Create(dbValue));

        builder.Property(b => b.PropertyId)
            .ValueGeneratedNever()
            .HasConversion(
                propertyId => propertyId.Id,
                dbValue => PropertyId.Create(dbValue));

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Property)
            .WithMany()
            .HasForeignKey(b => b.PropertyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.CheckInDate)
            .IsRequired();

        builder.Property(b => b.CheckOutDate)
            .IsRequired();

        builder.Property(b => b.NumberOfGuests)
            .IsRequired();

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.SpecialRequests)
            .HasMaxLength(1000);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired(false);

        // Configure owned collections
        var navigationBookingDetails = builder.Metadata.FindNavigation(nameof(Booking.BookingDetails));
        navigationBookingDetails?.SetPropertyAccessMode(PropertyAccessMode.Field);

        var navigationBookingHistory = builder.Metadata.FindNavigation(nameof(Booking.BookingHistory));
        navigationBookingHistory?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.BookingDetails)
            .WithOne(bd => bd.Booking)
            .HasForeignKey(bd => bd.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BookingHistory)
            .WithOne(bh => bh.Booking)
            .HasForeignKey(bh => bh.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
