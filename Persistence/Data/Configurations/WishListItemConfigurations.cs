using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class WishListItemConfigurations : IEntityTypeConfiguration<WishListItem>
    {
        public void Configure(EntityTypeBuilder<WishListItem> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserId)
                .IsRequired()
                .HasMaxLength(450); // Identity user ID length

            builder.Property(w => w.PropertyId)
                .IsRequired();

            builder.Property(w => w.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure relationships
            builder.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(w => w.Property)
                .WithMany()
                .HasForeignKey(w => w.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique constraint to prevent duplicate wishlist items
            builder.HasIndex(w => new { w.UserId, w.PropertyId })
                .IsUnique();

            // Configure table name
            builder.ToTable("WishListItems");
        }
    }
} 