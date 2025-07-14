using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IChatHubContext
    {
        // Message Events
        Task SendNewMessageAsync(string groupName, int messageId, string senderId, string message, System.DateTime sentAt);
        Task SendMessageDeliveredAsync(string groupName, int messageId, string deliveredToUserId);
        Task SendMessageReadAsync(string groupName, int messageId, string readByUserId, System.DateTime readAt);
        Task SendReactionAddedAsync(string groupName, int messageId, string userId, string reactionType);
        Task SendReactionRemovedAsync(string groupName, int messageId, string userId);
        
        // Typing Events
        Task SendTypingIndicatorAsync(string groupName, string userId, bool isTyping);
        
        // Room Events
        Task SendUserJoinedRoomAsync(string groupName, string userId);
        Task SendUserLeftRoomAsync(string groupName, string userId);
    }
} 