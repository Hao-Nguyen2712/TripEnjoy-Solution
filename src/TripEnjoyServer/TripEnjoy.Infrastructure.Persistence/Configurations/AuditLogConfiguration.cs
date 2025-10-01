using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.AuditLog;
using TripEnjoy.Domain.AuditLog.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.Id,
                    dbValue => AuditLogId.Create(dbValue));
            builder.Property(a => a.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id,
                    dbValue => AccountId.Create(dbValue));
            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(a => a.EntityId)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(a => a.OldValue)
                .IsRequired()
                .HasMaxLength(4000); // Increased from 256 to 4000 to handle larger JSON data
            builder.Property(a => a.NewValue)
                .IsRequired()
                .HasMaxLength(4000); // Increased from 256 to 4000 to handle larger JSON data
            builder.Property(a => a.CreatedAt)
                .IsRequired();

              builder.HasOne(log => log.Account) // Mỗi AuditLog có một Account
                   .WithMany() // Account có nhiều AuditLog (nhưng không có thuộc tính điều hướng)
                   .HasForeignKey(log => log.AccountId) // Khóa ngoại là AccountId
                   .OnDelete(DeleteBehavior.Restrict); // Ngăn không cho xóa Account nếu vẫn còn log liên quan
        }
        
    }
}