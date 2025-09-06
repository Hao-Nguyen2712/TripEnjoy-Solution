using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            // 1. Chỉ định khóa chính
            builder.HasKey(w => w.Id);

            // 2. Cấu hình cách chuyển đổi Value Object Id
            builder.Property(w => w.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    walletId => walletId.Id,
                    dbValue => WalletId.Create(dbValue));

            
            builder.Property(w => w.AccountId)
                .IsRequired()
                .HasConversion(
                    accountId => accountId.Id,
                    dbValue => AccountId.Create(dbValue));
            
            // 4. Cấu hình các thuộc tính còn lại
            builder.Property(w => w.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(w => w.CreatedAt).IsRequired();
            builder.Property(w => w.UpdatedAt).IsRequired();
        
        }
    }
}