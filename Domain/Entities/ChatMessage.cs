using System;
using Domain.Entities;

namespace Domain.Entities
{
    public class ChatMessage : BaseEntity<int>
    {
        public int ChatRoomId { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDelivered { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public int? RepliedToMessageId { get; set; } // nullable FK to ChatMessage
    }
} 