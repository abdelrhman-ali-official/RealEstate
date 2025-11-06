using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.ChatModels;

namespace Services.Abstractions
{
    public interface IChatService
    {
        Task<ChatRoomDto> StartOrGetChatRoomAsync(int propertyId, string user1Id, string user2Id);
        Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId);
        Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int chatRoomId, string userId);
        Task<ChatMessageDto> SendMessageAsync(int chatRoomId, string senderId, string message, int? repliedToMessageId);
        
        // Enhanced Message Status Management
        Task MarkMessageAsDeliveredAsync(int messageId);
        Task<MessageReadDto> MarkMessageAsReadAsync(int messageId, string userId);
        Task MarkMessagesAsDeliveredAsync(List<int> messageIds, string userId);
        Task MarkAllMessagesAsReadAsync(int chatRoomId, string userId);
        Task<MessageStatusSummaryDto> GetMessageStatusAsync(int messageId, string requestingUserId);
        
        Task<int> GetTotalUnreadCountAsync(string userId);
        Task<IEnumerable<UnreadCountPerRoomDto>> GetUnreadCountPerRoomAsync(string userId);
        Task<IEnumerable<ChatRoomSummaryDto>> GetUserChatRoomSummariesAsync(string userId);
        Task<IEnumerable<ChatMessageWithSenderDto>> GetChatHistoryWithSenderAsync(int chatRoomId, string userId);
        Task<ChatMessageReactionDto> AddReactionAsync(int messageId, string userId, string reactionType);
        Task RemoveReactionAsync(int messageId, string userId);
        Task<ChatRoomDto?> StartChatWithPropertyOwnerAsync(int propertyId, string userId);
        Task<bool> CanPropertyOwnerContactViewerAsync(int propertyId, string propertyOwnerId, string viewerUserId);
        Task<bool> IsUserPropertyOwnerAsync(int propertyId, string userId);
        Task<ChatRoomDto> StartChatWithViewerAndSendMessageAsync(int propertyId, string senderId, string receiverId, string message);
        
        // Enhanced real-time features
        Task<bool> CanUserAccessChatRoomAsync(int chatRoomId, string userId);
        Task UpdateUserConnectionAsync(string userId, string connectionId, bool isOnline);
        Task UpdateUserLastSeenAsync(string userId);
        Task<IEnumerable<string>> GetOnlineUsersInRoomAsync(int chatRoomId);
        Task MarkMessageAsDeliveredForOnlineUsersAsync(int messageId, int chatRoomId);
        Task<PaginatedChatHistoryDto> GetChatHistoryWithPaginationAsync(int chatRoomId, string userId, int pageNumber = 1, int pageSize = 50);
        Task<ChatRoomInfoDto?> GetChatRoomInfoAsync(int chatRoomId, string userId);
    }
} 