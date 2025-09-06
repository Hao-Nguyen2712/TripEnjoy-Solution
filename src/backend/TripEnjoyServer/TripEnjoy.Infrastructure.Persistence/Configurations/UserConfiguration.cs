using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    userId => userId.Id, // Correct property is .Value
                    dbValue => UserId.Create(dbValue));

            builder.Property(u => u.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id, // Correct property is .Value
                    dbValue => AccountId.Create(dbValue));
            
            // Cấu hình các thuộc tính khác
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.Address)
                .HasMaxLength(500);
        }
    }
}
