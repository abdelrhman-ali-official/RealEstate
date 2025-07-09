using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class AppointmentConfigurations : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Notes)
                .HasMaxLength(2000);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Property)
                .WithMany()
                .HasForeignKey(a => a.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Developer)
                .WithMany()
                .HasForeignKey(a => a.DeveloperId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Broker)
                .WithMany()
                .HasForeignKey(a => a.BrokerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure a property is linked to either a developer or broker, not both
            builder.HasCheckConstraint("CK_Appointment_Owner", 
                "(DeveloperId IS NOT NULL AND BrokerId IS NULL) OR (DeveloperId IS NULL AND BrokerId IS NOT NULL)");
        }
    }
} 