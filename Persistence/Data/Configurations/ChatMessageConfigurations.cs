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
            builder.HasOne<ChatRoom>()
                   .WithMany()
                   .HasForeignKey(x => x.ChatRoomId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<ChatMessage>()
                   .WithMany()
                   .HasForeignKey(x => x.RepliedToMessageId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 