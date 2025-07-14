using System.Collections.Generic;

namespace Shared.ChatModels
{
    public class ChatMessageWithSenderDto
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderLogoUrl { get; set; }
        public string Message { get; set; }
        public System.DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public int? RepliedToMessageId { get; set; }
        public List<ChatMessageReactionDto> Reactions { get; set; } = new List<ChatMessageReactionDto>();
    }
} 