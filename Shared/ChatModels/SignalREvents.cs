namespace Shared.ChatModels
{
    /// <summary>
    /// Documentation for SignalR Chat Events
    /// This file serves as a reference for frontend developers implementing the chat functionality.
    /// </summary>
    public static class SignalREvents
    {
        #region Client to Server Methods (Hub Methods)

        /// <summary>
        /// Join a chat room
        /// </summary>
        /// <param name="roomName">Chat room identifier (usually the room ID as string)</param>
        public const string JoinRoom = "JoinRoom";

        /// <summary>
        /// Leave a chat room
        /// </summary>
        /// <param name="roomName">Chat room identifier</param>
        public const string LeaveRoom = "LeaveRoom";

        /// <summary>
        /// Send a message to a chat room
        /// </summary>
        /// <param name="roomName">Chat room identifier</param>
        /// <param name="message">Message content</param>
        public const string SendMessageToRoom = "SendMessageToRoom";

        /// <summary>
        /// Mark a message as delivered
        /// </summary>
        /// <param name="messageId">ID of the message to mark as delivered</param>
        public const string MarkMessageAsDelivered = "MarkMessageAsDelivered";

        /// <summary>
        /// Mark a message as read
        /// </summary>
        /// <param name="messageId">ID of the message to mark as read</param>
        public const string MarkMessageAsRead = "MarkMessageAsRead";

        /// <summary>
        /// Send typing indicator
        /// </summary>
        /// <param name="roomName">Chat room identifier</param>
        /// <param name="isTyping">True if user is typing, false if stopped typing</param>
        public const string UserTyping = "UserTyping";

        /// <summary>
        /// Get online users in a room
        /// </summary>
        /// <param name="roomName">Chat room identifier</param>
        public const string GetOnlineUsers = "GetOnlineUsers";

        /// <summary>
        /// Update user's last seen timestamp
        /// </summary>
        public const string UpdateLastSeen = "UpdateLastSeen";

        #endregion

        #region Server to Client Events (Events clients should listen for)

        /// <summary>
        /// Receive a new message
        /// Payload: messageId, senderId, message, sentAt
        /// </summary>
        public const string ReceiveMessage = "ReceiveMessage";

        /// <summary>
        /// Message delivery confirmation
        /// Payload: messageId, deliveredToUserId
        /// </summary>
        public const string MessageDelivered = "MessageDelivered";

        /// <summary>
        /// Message read confirmation
        /// Payload: messageId, readByUserId, readAt
        /// </summary>
        public const string MessageRead = "MessageRead";

        /// <summary>
        /// User typing indicator
        /// Payload: userId, isTyping
        /// </summary>
        public const string UserTypingEvent = "UserTyping";

        /// <summary>
        /// User joined room notification
        /// Payload: userId
        /// </summary>
        public const string UserJoined = "UserJoined";

        /// <summary>
        /// User left room notification
        /// Payload: userId
        /// </summary>
        public const string UserLeft = "UserLeft";

        /// <summary>
        /// User online status change
        /// Payload: userId, isOnline
        /// </summary>
        public const string UserOnlineStatus = "UserOnlineStatus";

        /// <summary>
        /// User last seen update
        /// Payload: userId, lastSeen
        /// </summary>
        public const string UserLastSeen = "UserLastSeen";

        /// <summary>
        /// Online users in room
        /// Payload: roomName, onlineUsers[]
        /// </summary>
        public const string OnlineUsers = "OnlineUsers";

        /// <summary>
        /// Push notification
        /// Payload: title, message, type
        /// </summary>
        public const string Notification = "Notification";

        /// <summary>
        /// Error message
        /// Payload: error, details
        /// </summary>
        public const string ErrorMessage = "ErrorMessage";

        #endregion
    }

    /// <summary>
    /// Event parameter models for better type safety
    /// </summary>
    public static class SignalREventModels
    {
        public record NewMessageEvent(int MessageId, string SenderId, string Message, DateTime SentAt);
        public record MessageDeliveredEvent(int MessageId, string DeliveredToUserId);
        public record MessageReadEvent(int MessageId, string ReadByUserId, DateTime ReadAt);
        public record TypingEvent(string UserId, bool IsTyping);
        public record UserJoinedEvent(string UserId);
        public record UserLeftEvent(string UserId);
        public record ErrorEvent(string Title, string Message);
    }
} 