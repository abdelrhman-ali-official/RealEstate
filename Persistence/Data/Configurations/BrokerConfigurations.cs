using Domain.Entities.BrokerEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class BrokerConfigurations : IEntityTypeConfiguration<Broker>
    {
        public void Configure(EntityTypeBuilder<Broker> builder)
        {
            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(b => b.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(b => b.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.AgencyName)
                .HasMaxLength(200);

            builder.Property(b => b.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(b => b.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Government)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.LogoUrl)
                .HasMaxLength(500);

            builder.Property(b => b.CreatedAt)
                .IsRequired();
        }
    }
} 