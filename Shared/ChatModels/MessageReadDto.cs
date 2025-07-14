namespace Shared.ChatModels
{
    public class MessageReadDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string ReadByUserId { get; set; }
        public System.DateTime ReadAt { get; set; }
    }
} 