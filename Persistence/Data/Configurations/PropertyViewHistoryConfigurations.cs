using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class PropertyViewHistoryConfigurations : IEntityTypeConfiguration<PropertyViewHistory>
    {
        public void Configure(EntityTypeBuilder<PropertyViewHistory> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(v => v.PropertyId)
                .IsRequired();

            builder.Property(v => v.ViewedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Property)
                .WithMany()
                .HasForeignKey(v => v.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("PropertyViewHistories");
        }
    }
} 