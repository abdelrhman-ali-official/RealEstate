using Domain.Entities.SubscriptionEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class PackageConfigurations : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("Packages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.MonthlyPrice)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.YearlyPrice)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(10000);

            builder.Property(x => x.PropertyLimit)
                .IsRequired();

            builder.Property(x => x.ShowPropertyViews)
                .HasDefaultValue(true);

            builder.Property(x => x.ShowWishlistNotifications)
                .HasDefaultValue(true);

            builder.Property(x => x.ShowWishlistUserDetails)
                .HasDefaultValue(false);

            builder.Property(x => x.FunnelTracking)
                .HasDefaultValue(false);

            builder.Property(x => x.ExportLeads)
                .HasDefaultValue(false);

            builder.Property(x => x.DirectContactSystem)
                .HasDefaultValue(false);

            builder.Property(x => x.WhatsAppIntegration)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(x => x.Subscriptions)
                .WithOne(x => x.Package)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for performance
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.IsActive);
        }
    }
}