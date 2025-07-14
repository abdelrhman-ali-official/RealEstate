using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class ChatRoomConfigurations : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PropertyId).IsRequired();
            builder.Property(x => x.User1Id).IsRequired();
            builder.Property(x => x.User2Id).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => new { x.PropertyId, x.User1Id, x.User2Id }).IsUnique();
        }
    }
} 