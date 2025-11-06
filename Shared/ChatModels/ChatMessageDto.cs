using System;
using System.Collections.Generic;

namespace Shared.ChatModels
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RepliedToMessageId { get; set; }
        public List<ChatMessageReactionDto> Reactions { get; set; } = new List<ChatMessageReactionDto>();
        
        // Enhanced status information
        public string DeliveryStatus => GetDeliveryStatusText();
        public bool CanMarkAsRead => !IsRead && !string.IsNullOrEmpty(SenderId);
        
        private string GetDeliveryStatusText()
        {
            if (IsRead) return "Read";
            if (IsDelivered) return "Delivered";
            return "Sent";
        }
    }
} 