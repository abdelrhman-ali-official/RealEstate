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

        #endregion

        #region Server to Client Events

        /// <summary>
        /// New message received
        /// Parameters: (int messageId, string senderId, string message, DateTime sentAt)
        /// </summary>
        public const string ReceiveMessage = "ReceiveMessage";

        /// <summary>
        /// Message delivered to recipient
        /// Parameters: (int messageId, string deliveredToUserId)
        /// </summary>
        public const string MessageDelivered = "MessageDelivered";

        /// <summary>
        /// Message read by recipient
        /// Parameters: (int messageId, string readByUserId, DateTime readAt)
        /// </summary>
        public const string MessageRead = "MessageRead";

        /// <summary>
        /// User typing indicator
        /// Parameters: (string userId, bool isTyping)
        /// </summary>
        public const string UserTypingEvent = "UserTyping";

        /// <summary>
        /// User joined the room
        /// Parameters: (string userId)
        /// </summary>
        public const string UserJoined = "UserJoined";

        /// <summary>
        /// User left the room
        /// Parameters: (string userId)
        /// </summary>
        public const string UserLeft = "UserLeft";

        /// <summary>
        /// Error occurred during operation
        /// Parameters: (string title, string message)
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