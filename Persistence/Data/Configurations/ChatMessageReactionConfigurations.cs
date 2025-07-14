using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class ChatMessageReactionConfigurations : IEntityTypeConfiguration<ChatMessageReaction>
    {
        public void Configure(EntityTypeBuilder<ChatMessageReaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MessageId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ReactionType).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => new { x.MessageId, x.UserId }).IsUnique();
            builder.HasOne<ChatMessage>()
                   .WithMany()
                   .HasForeignKey(x => x.MessageId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 