namespace Shared.ChatModels
{
    public class TypingIndicatorDto
    {
        public int ChatRoomId { get; set; }
        public string UserId { get; set; }
        public bool IsTyping { get; set; }
    }
} 