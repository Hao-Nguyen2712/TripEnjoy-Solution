
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.PropertyType.ValueObjects;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.PropertyType;

namespace TripEnjoy.Infrastructure.Persistence.Configurations
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            builder.ToTable("Properties");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    propertyId => propertyId.Id,
                    dbValue => PropertyId.Create(dbValue));

                builder.Property(p => p.PartnerId)
                .ValueGeneratedNever()
                .HasConversion(
                    partnerId => partnerId.Id,
                    dbValue => PartnerId.Create(dbValue));

            builder.Property(p => p.PropertyTypeId)
                .ValueGeneratedNever()
                .HasConversion(
                    propertyTypeId => propertyTypeId.Id,
                    dbValue => PropertyTypeId.Create(dbValue));
                    
            builder.HasOne<Partner>() 
                      .WithMany() 
                      .HasForeignKey(p => p.PartnerId) 
                      .IsRequired().OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne<PropertyType>()
                   .WithMany() 
                   .HasForeignKey(p => p.PropertyTypeId)
                   .IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.Description)
                .HasMaxLength(500);
            builder.Property(p => p.Address)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.Country)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.Latitude)
                .HasPrecision(10, 8);
            builder.Property(p => p.Longitude)
                .HasPrecision(11, 8);
            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.AverageRating)
                .HasPrecision(10, 2);
            builder.Property(p => p.ReviewCount);
            builder.Property(p => p.CreatedAt)
                .IsRequired();
            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);
        }
    }
}