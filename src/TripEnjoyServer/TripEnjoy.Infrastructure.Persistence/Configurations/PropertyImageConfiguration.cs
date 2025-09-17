using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Property.Entities;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(EntityTypeBuilder<PropertyImage> builder)
        {
            builder.ToTable("PropertyImages");
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    propertyImageId => propertyImageId.Id,
                    dbValue => PropertyImageId.Create(dbValue));

            builder.Property(pi => pi.PropertyId)
                .IsRequired()
                .HasConversion(
                    propertyId => propertyId.Id,
                    dbValue => PropertyId.Create(dbValue));

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pi => pi.IsMain)
                .IsRequired();

            builder.Property(pi => pi.UploadAt)
                .IsRequired();
                
            
                
        }
    }
}