using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class ChatMessageStatusConfigurations : IEntityTypeConfiguration<ChatMessageStatus>
    {
        public void Configure(EntityTypeBuilder<ChatMessageStatus> builder)
        {
            builder.HasKey(cms => cms.Id);

            builder.Property(cms => cms.MessageId)
                .IsRequired();

            builder.Property(cms => cms.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cms => cms.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(cms => cms.StatusChangedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(cms => cms.Message)
                .WithMany()
                .HasForeignKey(cms => cms.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cms => cms.MessageId);
            builder.HasIndex(cms => new { cms.MessageId, cms.UserId });

            builder.ToTable("ChatMessageStatuses");
        }
    }
}