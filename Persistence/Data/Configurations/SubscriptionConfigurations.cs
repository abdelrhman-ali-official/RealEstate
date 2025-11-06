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
    public class SubscriptionConfigurations : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PackageId)
                .IsRequired();

            builder.Property(x => x.BrokerId)
                .IsRequired(false);

            builder.Property(x => x.DeveloperId)
                .IsRequired(false);

            builder.Property(x => x.SubscribedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.PlanType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Monthly");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.CurrentPropertyCount)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(x => x.Package)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Broker)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.BrokerId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(x => x.Developer)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.DeveloperId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Indexes for performance
            builder.HasIndex(x => new { x.BrokerId, x.IsActive });
            builder.HasIndex(x => new { x.DeveloperId, x.IsActive });
            builder.HasIndex(x => x.PackageId);
            builder.HasIndex(x => x.SubscribedAt);

            // Constraint: Only one active subscription per user
            builder.HasIndex(x => new { x.BrokerId, x.IsActive })
                .HasFilter("[BrokerId] IS NOT NULL AND [IsActive] = 1")
                .IsUnique();

            builder.HasIndex(x => new { x.DeveloperId, x.IsActive })
                .HasFilter("[DeveloperId] IS NOT NULL AND [IsActive] = 1")
                .IsUnique();

            // Check constraint: Either BrokerId or DeveloperId must be set, but not both
            builder.HasCheckConstraint("CK_Subscription_UserType", 
                "([BrokerId] IS NOT NULL AND [DeveloperId] IS NULL) OR ([BrokerId] IS NULL AND [DeveloperId] IS NOT NULL)");
        }
    }
}