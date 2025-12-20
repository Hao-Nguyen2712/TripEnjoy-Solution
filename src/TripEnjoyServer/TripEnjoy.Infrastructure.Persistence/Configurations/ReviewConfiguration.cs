using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .HasConversion(
                reviewId => reviewId.Value,
                dbValue => ReviewId.Create(dbValue));

        builder.Property(r => r.BookingDetailId)
            .ValueGeneratedNever()
            .HasConversion(
                bookingDetailId => bookingDetailId.Value,
                dbValue => BookingDetailId.Create(dbValue));

        builder.Property(r => r.UserId)
            .ValueGeneratedNever()
            .HasConversion(
                userId => userId.Id,
                dbValue => UserId.Create(dbValue));

        builder.Property(r => r.RoomTypeId)
            .ValueGeneratedNever()
            .HasConversion(
                roomTypeId => roomTypeId.Value,
                dbValue => RoomTypeId.Create(dbValue));

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RoomType)
            .WithMany()
            .HasForeignKey(r => r.RoomTypeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.BookingDetail)
            .WithMany()
            .HasForeignKey(r => r.BookingDetailId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Configure navigation properties
        var navigationReviewImages = builder.Metadata.FindNavigation(nameof(Review.ReviewImages));
        navigationReviewImages?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.ReviewImages)
            .WithOne(ri => ri.Review)
            .HasForeignKey(ri => ri.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigationReviewReplies = builder.Metadata.FindNavigation(nameof(Review.ReviewReplies));
        navigationReviewReplies?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.ReviewReplies)
            .WithOne(rr => rr.Review)
            .HasForeignKey(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for query performance
        builder.HasIndex(r => r.RoomTypeId)
            .HasDatabaseName("IX_Reviews_RoomTypeId");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Reviews_UserId");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Reviews_Status");

        builder.HasIndex(r => r.BookingDetailId)
            .IsUnique()
            .HasDatabaseName("IX_Reviews_BookingDetailId_Unique");

        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_Reviews_CreatedAt");
    }
}
