using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Room.Entities;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class RoomAvailabilityConfiguration : IEntityTypeConfiguration<RoomAvailability>
{
    public void Configure(EntityTypeBuilder<RoomAvailability> builder)
    {
        builder.ToTable("RoomAvailabilities");
        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Id)
            .ValueGeneratedNever()
            .HasConversion(
                roomAvailabilityId => roomAvailabilityId.Value,
                dbValue => RoomAvailabilityId.Create(dbValue));

        builder.Property(ra => ra.RoomTypeId)
            .IsRequired()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.Property(ra => ra.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(ra => ra.AvailableQuantity)
            .IsRequired();

        builder.Property(ra => ra.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ra => ra.CreatedAt)
            .IsRequired();

        builder.Property(ra => ra.UpdatedAt)
            .IsRequired(false);

        // Create composite index for RoomTypeId and Date for efficient lookups
        builder.HasIndex(ra => new { ra.RoomTypeId, ra.Date })
            .IsUnique()
            .HasDatabaseName("IX_RoomAvailabilities_RoomTypeId_Date");
    }
}
