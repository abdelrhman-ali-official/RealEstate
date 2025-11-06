using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class ChatMessageConfigurations : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ChatRoomId).IsRequired();
            builder.Property(x => x.SenderId).IsRequired();
            builder.Property(x => x.Message).IsRequired();
            builder.Property(x => x.SentAt).IsRequired();
            builder.Property(x => x.IsDelivered).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.DeliveredAt).IsRequired(false);
            builder.Property(x => x.ReadAt).IsRequired(false);
            
            // Configure relationship with ChatRoom using the navigation property
            builder.HasOne(x => x.ChatRoom)
                   .WithMany()
                   .HasForeignKey(x => x.ChatRoomId)
                   .OnDelete(DeleteBehavior.Cascade);
            
            // Configure self-referencing relationship for RepliedToMessage
            builder.HasOne(x => x.RepliedToMessage)
                   .WithMany()
                   .HasForeignKey(x => x.RepliedToMessageId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 