using System;
using System.Collections.Generic;

namespace Shared.ChatModels
{
    public class OnlineUserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class PaginatedChatHistoryDto
    {
        public List<ChatMessageWithSenderDto> Messages { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalMessages { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class ChatRoomInfoDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string User1Id { get; set; }
        public string User1Name { get; set; }
        public string User2Id { get; set; }
        public string User2Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public List<OnlineUserDto> OnlineUsers { get; set; } = new();
    }
}