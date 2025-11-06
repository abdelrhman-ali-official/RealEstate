using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Services.Abstractions;
using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Start
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IChatHubContext _hubContext;
        private readonly ILogger<ChatHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private static readonly ConcurrentDictionary<string, DateTime> _userLastSeen = new();

        public ChatHub(IChatService chatService, IChatHubContext hubContext, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryAdd(userId, Context.ConnectionId);
                _userLastSeen.AddOrUpdate(userId, DateTime.UtcNow, (key, value) => DateTime.UtcNow);
                
                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
                
                // Notify contacts that user is online
                await _hubContext.SendUserOnlineStatusAsync(userId, true);
                
                // Update user connection in database
                await _chatService.UpdateUserConnectionAsync(userId, Context.ConnectionId, true);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                _userLastSeen.AddOrUpdate(userId, DateTime.UtcNow, (key, value) => DateTime.UtcNow);
                
                _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, Context.ConnectionId);
                
                // Notify contacts that user is offline
                await _hubContext.SendUserOnlineStatusAsync(userId, false);
                
                // Update user connection in database
                await _chatService.UpdateUserConnectionAsync(userId, Context.ConnectionId, false);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        #region Room Management Methods

        public async Task JoinRoom(string roomName)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            
            // Notify other users in the room that this user joined
            await _hubContext.SendUserJoinedRoomAsync(roomName, userId);
        }

        public async Task LeaveRoom(string roomName)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            
            // Notify other users in the room that this user left
            await _hubContext.SendUserLeftRoomAsync(roomName, userId);
        }

        #endregion

        #region Message Methods

        public async Task AddReaction(int messageId, string reactionType)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            // Get message to find room
            var message = await _chatService.GetChatHistoryAsync(0, userId); // Placeholder, replace with actual fetch
            // You should implement a method to get message by ID and its room
            // For now, assume you have chatRoomId
            // int chatRoomId = ...;
            // string groupName = chatRoomId.ToString();
            // await _chatService.AddReactionAsync(messageId, userId, reactionType);
            // await _hubContext.SendReactionAddedAsync(groupName, messageId, userId, reactionType);
        }

        public async Task RemoveReaction(int messageId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;
            // int chatRoomId = ...;
            // string groupName = chatRoomId.ToString();
            // await _chatService.RemoveReactionAsync(messageId, userId);
            // await _hubContext.SendReactionRemovedAsync(groupName, messageId, userId);
        }

        public async Task SendMessageToRoom(string roomName, string message, int? repliedToMessageId = null)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Authentication required");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Message cannot be empty");
                return;
            }

            if (!int.TryParse(roomName, out int chatRoomId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Invalid room ID");
                return;
            }

            try
            {
                // Validate user can send message to this room
                var canSendMessage = await _chatService.CanUserAccessChatRoomAsync(chatRoomId, userId);
                if (!canSendMessage)
                {
                    await Clients.Caller.SendAsync("ErrorMessage", "You don't have permission to send messages to this room");
                    return;
                }

                var chatMessage = await _chatService.SendMessageAsync(chatRoomId, userId, message, repliedToMessageId);
                
                // Send message to all users in the room
                await _hubContext.SendNewMessageAsync(
                    roomName,
                    chatMessage.Id,
                    chatMessage.SenderId,
                    chatMessage.Message,
                    chatMessage.SentAt
                );

                // Mark as delivered for online users in the room
                await _chatService.MarkMessageAsDeliveredForOnlineUsersAsync(chatMessage.Id, chatRoomId);

                _logger.LogInformation("Message {MessageId} sent by user {UserId} to room {RoomId}", 
                    chatMessage.Id, userId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message from user {UserId} to room {RoomId}", userId, chatRoomId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to send message", ex.Message);
            }
        }

        #endregion

        #region Message Status Methods

        public async Task MarkMessageAsDelivered(int messageId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                await _chatService.MarkMessageAsDeliveredAsync(messageId);
                
                // Get the chat room ID for the message to broadcast to the correct room
                // This would require a method to get message details or pass roomId as parameter
                // For now, we'll use a simplified approach
                await Clients.Caller.SendAsync("MessageDelivered", messageId, userId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark message as delivered", ex.Message);
            }
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);
                
                if (result != null)
                {
                    // Broadcast read status to all users in the room
                    await _hubContext.SendMessageReadAsync(
                        result.ChatRoomId.ToString(), 
                        result.MessageId, 
                        result.ReadByUserId, 
                        result.ReadAt
                    );
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark message as read", ex.Message);
            }
        }

        #endregion

        #region Typing Indicator Methods

        public async Task UserTyping(string roomName, bool isTyping)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            if (!int.TryParse(roomName, out int chatRoomId))
                return;

            // Validate user can access this room
            var canAccess = await _chatService.CanUserAccessChatRoomAsync(chatRoomId, userId);
            if (!canAccess)
                return;

            // Broadcast typing indicator to other users in the room (not the sender)
            await Clients.GroupExcept(roomName, Context.ConnectionId).SendAsync("UserTyping", userId, isTyping);
            
            _logger.LogDebug("User {UserId} typing status: {IsTyping} in room {RoomId}", userId, isTyping, chatRoomId);
        }

        #endregion

        #region Message Status Methods

        /// <summary>
        /// Mark message as delivered when user receives it
        /// </summary>
        public async Task MarkMessageDelivered(int messageId, int chatRoomId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                await _chatService.MarkMessageAsDeliveredAsync(messageId);
                
                // Notify sender about delivery
                var roomName = $"room_{chatRoomId}";
                await Clients.GroupExcept(roomName, Context.ConnectionId)
                    .SendAsync("MessageDelivered", messageId, userId, DateTime.UtcNow);
                
                _logger.LogDebug("Message {MessageId} marked as delivered by user {UserId}", messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark message {MessageId} as delivered for user {UserId}", messageId, userId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark message as delivered");
            }
        }

        /// <summary>
        /// Mark message as read when user reads it
        /// </summary>
        public async Task MarkMessageRead(int messageId, int chatRoomId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);
                
                if (result != null)
                {
                    // Notify sender about read status
                    var roomName = $"room_{chatRoomId}";
                    await Clients.GroupExcept(roomName, Context.ConnectionId)
                        .SendAsync("MessageRead", messageId, userId, result.ReadAt);
                    
                    _logger.LogDebug("Message {MessageId} marked as read by user {UserId}", messageId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark message {MessageId} as read for user {UserId}", messageId, userId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark message as read");
            }
        }

        /// <summary>
        /// Mark all messages in a chat room as read
        /// </summary>
        public async Task MarkAllMessagesRead(int chatRoomId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                await _chatService.MarkAllMessagesAsReadAsync(chatRoomId, userId);
                
                // Notify other participants
                var roomName = $"room_{chatRoomId}";
                await Clients.GroupExcept(roomName, Context.ConnectionId)
                    .SendAsync("AllMessagesRead", chatRoomId, userId, DateTime.UtcNow);
                
                _logger.LogDebug("All messages in room {ChatRoomId} marked as read by user {UserId}", chatRoomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all messages as read in room {ChatRoomId} for user {UserId}", chatRoomId, userId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark messages as read");
            }
        }

        /// <summary>
        /// Bulk mark messages as delivered
        /// </summary>
        public async Task BulkMarkMessagesDelivered(int[] messageIds, int chatRoomId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                await _chatService.MarkMessagesAsDeliveredAsync(messageIds.ToList(), userId);
                
                // Notify sender about bulk delivery
                var roomName = $"room_{chatRoomId}";
                await Clients.GroupExcept(roomName, Context.ConnectionId)
                    .SendAsync("MessagesDeliveredBulk", messageIds, userId, DateTime.UtcNow);
                
                _logger.LogDebug("Bulk marked {Count} messages as delivered by user {UserId}", messageIds.Length, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk mark messages as delivered for user {UserId}", userId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to mark messages as delivered");
            }
        }

        /// <summary>
        /// Request message status information
        /// </summary>
        public async Task GetMessageStatus(int messageId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                var status = await _chatService.GetMessageStatusAsync(messageId, userId);
                await Clients.Caller.SendAsync("MessageStatus", messageId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status for message {MessageId} for user {UserId}", messageId, userId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to get message status");
            }
        }

        #endregion

        #region Presence Methods

        public async Task GetOnlineUsers(string roomName)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return;

            if (!int.TryParse(roomName, out int chatRoomId))
                return;

            try
            {
                var onlineUsers = await _chatService.GetOnlineUsersInRoomAsync(chatRoomId);
                await Clients.Caller.SendAsync("OnlineUsers", roomName, onlineUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get online users for room {RoomId}", chatRoomId);
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to get online users");
            }
        }

        public async Task UpdateLastSeen()
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _userLastSeen.AddOrUpdate(userId, DateTime.UtcNow, (key, value) => DateTime.UtcNow);
                await _chatService.UpdateUserLastSeenAsync(userId);
            }
        }

        #endregion

        #region Helper Methods

        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        #endregion
    }
} 