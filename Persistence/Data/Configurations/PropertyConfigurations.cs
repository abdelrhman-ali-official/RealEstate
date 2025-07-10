using Domain.Entities.DeveloperEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class PropertyConfigurations : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            // Developer relationship (optional)
            builder.HasOne(p => p.Developer)
                .WithMany(d => d.Properties)
                .HasForeignKey(p => p.DeveloperId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Broker relationship (optional) - no navigation property on Broker side
            builder.HasOne(p => p.Broker)
                .WithMany()
                .HasForeignKey(p => p.BrokerId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Ensure a property belongs to either a Developer or Broker, but not both
            builder.HasCheckConstraint("CK_Property_Owner", 
                "(DeveloperId IS NOT NULL AND BrokerId IS NULL) OR (DeveloperId IS NULL AND BrokerId IS NOT NULL)");

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Government)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.FullAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.Area)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(p => p.Rooms)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(p => p.Bathrooms)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(p => p.Type)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Purpose)
                .IsRequired();

            builder.Property(p => p.MainImageUrl)
                .HasMaxLength(500);

            builder.Property(p => p.AdditionalImages)
                .HasMaxLength(2000); // JSON string for multiple images

            builder.Property(p => p.CreatedAt)
                .IsRequired();
        }
    }
} 