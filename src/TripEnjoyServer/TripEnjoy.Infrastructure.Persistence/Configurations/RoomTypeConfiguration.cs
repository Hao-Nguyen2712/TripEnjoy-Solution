using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.Property(rt => rt.PropertyId)
            .ValueGeneratedNever()
            .HasConversion(
                propertyId => propertyId.Id,
                dbValue => PropertyId.Create(dbValue));

        builder.HasOne(rt => rt.Property)
            .WithMany()
            .HasForeignKey(rt => rt.PropertyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(rt => rt.RoomTypeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Description)
            .HasMaxLength(1000);

        builder.Property(rt => rt.Capacity)
            .IsRequired();

        builder.Property(rt => rt.BasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(rt => rt.TotalQuantity)
            .IsRequired();

        builder.Property(rt => rt.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(rt => rt.AverageRating)
            .HasPrecision(10, 2);

        builder.Property(rt => rt.ReviewCount)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired(false);

        // Configure navigation properties
        var navigationRoomTypeImages = builder.Metadata.FindNavigation(nameof(RoomType.RoomTypeImages));
        navigationRoomTypeImages?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(rt => rt.RoomTypeImages)
            .WithOne()
            .HasForeignKey(rti => rti.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigationRoomAvailabilities = builder.Metadata.FindNavigation(nameof(RoomType.RoomAvailabilities));
        navigationRoomAvailabilities?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(rt => rt.RoomAvailabilities)
            .WithOne()
            .HasForeignKey(ra => ra.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigationRoomPromotions = builder.Metadata.FindNavigation(nameof(RoomType.RoomPromotions));
        navigationRoomPromotions?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(rt => rt.RoomPromotions)
            .WithOne()
            .HasForeignKey(rp => rp.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
