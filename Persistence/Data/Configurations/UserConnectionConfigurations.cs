using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class UserConnectionConfigurations : IEntityTypeConfiguration<UserConnection>
    {
        public void Configure(EntityTypeBuilder<UserConnection> builder)
        {
            builder.HasKey(uc => uc.Id);

            builder.Property(uc => uc.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(uc => uc.ConnectionId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(uc => uc.ConnectedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(uc => uc.LastSeenAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(uc => uc.IsOnline)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(uc => uc.DeviceInfo)
                .HasMaxLength(500);

            builder.Property(uc => uc.UserAgent)
                .HasMaxLength(1000);

            builder.HasIndex(uc => uc.UserId);
            builder.HasIndex(uc => new { uc.UserId, uc.ConnectionId });

            builder.ToTable("UserConnections");
        }
    }
}