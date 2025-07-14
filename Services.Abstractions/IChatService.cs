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
        Task MarkMessageAsDeliveredAsync(int messageId);
        Task<MessageReadDto> MarkMessageAsReadAsync(int messageId, string userId);
        Task<int> GetTotalUnreadCountAsync(string userId);
        Task<IEnumerable<UnreadCountPerRoomDto>> GetUnreadCountPerRoomAsync(string userId);
        Task<IEnumerable<ChatRoomSummaryDto>> GetUserChatRoomSummariesAsync(string userId);
        Task<IEnumerable<ChatMessageWithSenderDto>> GetChatHistoryWithSenderAsync(int chatRoomId, string userId);
        Task<ChatMessageReactionDto> AddReactionAsync(int messageId, string userId, string reactionType);
        Task RemoveReactionAsync(int messageId, string userId);
    }
} 