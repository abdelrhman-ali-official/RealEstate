using System;

namespace Shared.ChatModels
{
    public class ChatMessageReactionDto
    {
        public int ReactionId { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public string ReactionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 