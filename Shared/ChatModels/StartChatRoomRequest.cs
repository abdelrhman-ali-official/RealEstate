namespace Shared.ChatModels
{
    /// <summary>
    /// Request model for starting or getting a chat room
    /// </summary>
    public class StartChatRoomRequest
    {
        /// <summary>
        /// The ID of the property being discussed
        /// </summary>
        public int PropertyId { get; set; }
        
        /// <summary>
        /// Optional: The ID of the other user to chat with.
        /// If not provided, the system will automatically use the property owner's ID.
        /// This is useful for customers who just want to contact the property owner.
        /// </summary>
        public string? OtherUserId { get; set; }
    }
}