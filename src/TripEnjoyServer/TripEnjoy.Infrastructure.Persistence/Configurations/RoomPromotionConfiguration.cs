using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Room.Entities;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class RoomPromotionConfiguration : IEntityTypeConfiguration<RoomPromotion>
{
    public void Configure(EntityTypeBuilder<RoomPromotion> builder)
    {
        builder.ToTable("RoomPromotions");
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .ValueGeneratedNever()
            .HasConversion(
                roomPromotionId => roomPromotionId.Value,
                dbValue => RoomPromotionId.Create(dbValue));

        builder.Property(rp => rp.RoomTypeId)
            .IsRequired()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.Property(rp => rp.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(rp => rp.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(rp => rp.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(rp => rp.EndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(rp => rp.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(rp => rp.CreatedAt)
            .IsRequired();

        builder.Property(rp => rp.UpdatedAt)
            .IsRequired(false);

        // Create index for RoomTypeId for efficient lookups
        builder.HasIndex(rp => rp.RoomTypeId)
            .HasDatabaseName("IX_RoomPromotions_RoomTypeId");
    }
}
