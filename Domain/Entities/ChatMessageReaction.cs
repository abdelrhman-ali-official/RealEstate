using System;

namespace Domain.Entities
{
    public class ChatMessageReaction : BaseEntity<int>
    {
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public string ReactionType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 