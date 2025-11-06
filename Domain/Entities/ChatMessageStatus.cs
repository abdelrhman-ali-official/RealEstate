using System;

namespace Domain.Entities
{
    public class ChatMessageStatus : BaseEntity<int>
    {
        public int MessageId { get; set; }
        public ChatMessage Message { get; set; }
        public string UserId { get; set; }
        public MessageStatusType Status { get; set; }
        public DateTime StatusChangedAt { get; set; } = DateTime.UtcNow;
    }

    public enum MessageStatusType
    {
        Sent = 1,
        Delivered = 2,
        Read = 3
    }
}