namespace Shared.ChatModels
{
    public class ChatRoomSummaryDto
    {
        public int RoomId { get; set; }
        public int PropertyId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string LastMessage { get; set; }
        public System.DateTime? LastMessageSentAt { get; set; }
        public int UnreadMessagesCount { get; set; }
    }
} 