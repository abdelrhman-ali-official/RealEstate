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
            builder.Property(x => x.User1Id).IsRequired().HasMaxLength(450);
            builder.Property(x => x.User2Id).IsRequired().HasMaxLength(450);
            builder.Property(x => x.CreatedAt).IsRequired();
            
            // Navigation properties
            builder.HasOne(x => x.Property)
                .WithMany()
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(x => x.User1)
                .WithMany()
                .HasForeignKey(x => x.User1Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(x => x.User2)
                .WithMany()
                .HasForeignKey(x => x.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(x => new { x.PropertyId, x.User1Id, x.User2Id }).IsUnique();
            builder.HasIndex(x => x.PropertyId);
            builder.HasIndex(x => x.User1Id);
            builder.HasIndex(x => x.User2Id);
            
            builder.ToTable("ChatRooms");
        }
    }
} 