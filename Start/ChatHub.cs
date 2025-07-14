using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Services.Abstractions;
using System;

namespace Start
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IChatHubContext _hubContext;

        public ChatHub(IChatService chatService, IChatHubContext hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
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
                return;

            if (int.TryParse(roomName, out int chatRoomId))
            {
                try
                {
                    var chatMessage = await _chatService.SendMessageAsync(chatRoomId, userId, message, repliedToMessageId);
                    await _hubContext.SendNewMessageAsync(
                        roomName,
                        chatMessage.Id,
                        chatMessage.SenderId,
                        chatMessage.Message,
                        chatMessage.SentAt
                    );
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("ErrorMessage", "Failed to send message", ex.Message);
                }
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

            // Broadcast typing indicator to all users in the room
            await _hubContext.SendTypingIndicatorAsync(roomName, userId, isTyping);
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