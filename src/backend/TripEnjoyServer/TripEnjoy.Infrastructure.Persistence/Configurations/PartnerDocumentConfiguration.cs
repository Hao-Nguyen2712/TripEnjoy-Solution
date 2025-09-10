using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class PartnerDocumentConfiguration : IEntityTypeConfiguration<PartnerDocument>
    {
        public void Configure(EntityTypeBuilder<PartnerDocument> builder)
        {
            builder.ToTable("PartnerDocuments");

            builder.HasKey(pd => pd.Id);

            builder.Property(pd => pd.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    partnerDocumentId => partnerDocumentId.Id,
                    dbValue => PartnerDocumentId.Create(dbValue));

            builder.Property(pd => pd.PartnerId)
                .IsRequired()
                .HasConversion(
                    partnerId => partnerId.Id,
                    dbValue => PartnerId.Create(dbValue));

            builder.Property(pd => pd.DocumentType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pd => pd.DocumentUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pd => pd.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pd => pd.CreatedAt)
                .IsRequired();

            builder.Property(pd => pd.ReviewedAt)
                .IsRequired(false);
                   
        }
    }
}