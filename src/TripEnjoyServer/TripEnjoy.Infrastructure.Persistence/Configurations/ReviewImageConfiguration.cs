using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.ToTable("ReviewImages");
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Id)
            .ValueGeneratedNever()
            .HasConversion(
                imageId => imageId.Value,
                dbValue => ReviewImageId.Create(dbValue));

        builder.Property(ri => ri.ReviewId)
            .ValueGeneratedNever()
            .HasConversion(
                reviewId => reviewId.Value,
                dbValue => ReviewId.Create(dbValue));

        builder.Property(ri => ri.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ri => ri.UploadedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(ri => ri.ReviewId)
            .HasDatabaseName("IX_ReviewImages_ReviewId");
    }
}
