using System;
using System.Collections.Generic;
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
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RepliedToMessageId { get; set; } // nullable FK to ChatMessage

        // Navigation Properties
        public virtual ChatRoom ChatRoom { get; set; }
        public virtual ChatMessage RepliedToMessage { get; set; }
        public virtual ICollection<ChatMessageStatus> MessageStatuses { get; set; } = new List<ChatMessageStatus>();
        public virtual ICollection<ChatMessageReaction> Reactions { get; set; } = new List<ChatMessageReaction>();
    }
} 