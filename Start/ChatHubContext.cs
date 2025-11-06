using Microsoft.AspNetCore.SignalR;
using Services.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Start
{
    public class ChatHubContext : IChatHubContext
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHubContext(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Message Events
        public async Task SendNewMessageAsync(string groupName, int messageId, string senderId, string message, System.DateTime sentAt)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageId, senderId, message, sentAt);
        }

        public async Task SendMessageDeliveredAsync(string groupName, int messageId, string deliveredToUserId)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("MessageDelivered", messageId, deliveredToUserId);
        }

        public async Task SendMessageReadAsync(string groupName, int messageId, string readByUserId, System.DateTime readAt)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("MessageRead", messageId, readByUserId, readAt);
        }

        public async Task SendReactionAddedAsync(string groupName, int messageId, string userId, string reactionType)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReactionAdded", messageId, userId, reactionType);
        }

        public async Task SendReactionRemovedAsync(string groupName, int messageId, string userId)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReactionRemoved", messageId, userId);
        }

        // Typing Events
        public async Task SendTypingIndicatorAsync(string groupName, string userId, bool isTyping)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("UserTyping", userId, isTyping);
        }

        // Room Events
        public async Task SendUserJoinedRoomAsync(string groupName, string userId)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("UserJoined", userId);
        }

        public async Task SendUserLeftRoomAsync(string groupName, string userId)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("UserLeft", userId);
        }

        // Presence Events
        public async Task SendUserOnlineStatusAsync(string userId, bool isOnline)
        {
            await _hubContext.Clients.All.SendAsync("UserOnlineStatus", userId, isOnline);
        }

        public async Task SendUserLastSeenAsync(string userId, System.DateTime lastSeen)
        {
            await _hubContext.Clients.All.SendAsync("UserLastSeen", userId, lastSeen);
        }

        // Notification Events
        public async Task SendNotificationAsync(string userId, string title, string message, string type)
        {
            await _hubContext.Clients.User(userId).SendAsync("Notification", title, message, type);
        }

        public async Task SendBulkNotificationAsync(IEnumerable<string> userIds, string title, string message, string type)
        {
            await _hubContext.Clients.Users(userIds).SendAsync("Notification", title, message, type);
        }
    }
} 