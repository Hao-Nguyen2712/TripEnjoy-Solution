using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Room.Entities;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class RoomTypeImageConfiguration : IEntityTypeConfiguration<RoomTypeImage>
{
    public void Configure(EntityTypeBuilder<RoomTypeImage> builder)
    {
        builder.ToTable("RoomTypeImages");
        builder.HasKey(rti => rti.Id);

        builder.Property(rti => rti.Id)
            .ValueGeneratedNever()
            .HasConversion(
                roomTypeImageId => roomTypeImageId.Value,
                dbValue => RoomTypeImageId.Create(dbValue));

        builder.Property(rti => rti.RoomTypeId)
            .IsRequired()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.Property(rti => rti.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rti => rti.IsMain)
            .IsRequired();

        builder.Property(rti => rti.UploadedAt)
            .IsRequired();
    }
}
