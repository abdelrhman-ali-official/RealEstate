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
    public class DeveloperConfigurations : IEntityTypeConfiguration<Developer>
    {
        public void Configure(EntityTypeBuilder<Developer> builder)
        {
            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(d => d.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Description)
                .HasMaxLength(1000);

            builder.Property(d => d.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(d => d.Website)
                .HasMaxLength(200);

            builder.Property(d => d.LogoUrl)
                .HasMaxLength(500);

            builder.Property(d => d.CreatedAt)
                .IsRequired();
        }
    }
} 