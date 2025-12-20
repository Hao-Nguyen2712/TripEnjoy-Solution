using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations;

public class ReviewReplyConfiguration : IEntityTypeConfiguration<ReviewReply>
{
    public void Configure(EntityTypeBuilder<ReviewReply> builder)
    {
        builder.ToTable("ReviewReplies");
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Id)
            .ValueGeneratedNever()
            .HasConversion(
                replyId => replyId.Value,
                dbValue => ReviewReplyId.Create(dbValue));

        builder.Property(rr => rr.ReviewId)
            .ValueGeneratedNever()
            .HasConversion(
                reviewId => reviewId.Value,
                dbValue => ReviewId.Create(dbValue));

        builder.Property(rr => rr.ReplierId)
            .ValueGeneratedNever()
            .HasConversion(
                accountId => accountId.Id,
                dbValue => AccountId.Create(dbValue));

        builder.Property(rr => rr.ReplierType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(rr => rr.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(rr => rr.CreatedAt)
            .IsRequired();

        builder.Property(rr => rr.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(rr => rr.ReviewId)
            .HasDatabaseName("IX_ReviewReplies_ReviewId");

        builder.HasIndex(rr => rr.ReplierId)
            .HasDatabaseName("IX_ReviewReplies_ReplierId");

        // Unique constraint: one reply per replier per review
        builder.HasIndex(rr => new { rr.ReviewId, rr.ReplierId })
            .IsUnique()
            .HasDatabaseName("IX_ReviewReplies_ReviewId_ReplierId_Unique");
    }
}
