using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Shared.ChatModels
{
    /// <summary>
    /// Comprehensive status information for a message including delivery and read status
    /// </summary>
    public class MessageStatusSummaryDto
    {
        public int MessageId { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public List<MessageStatusDetailDto> DetailedStatuses { get; set; } = new List<MessageStatusDetailDto>();
    }

    /// <summary>
    /// Detailed status information for a specific user's interaction with a message
    /// </summary>
    public class MessageStatusDetailDto
    {
        public string UserId { get; set; }
        public string Status { get; set; } // "Sent", "Delivered", "Read"
        public DateTime StatusChangedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for bulk operations on message status
    /// </summary>
    public class BulkMessageStatusRequest
    {
        public List<int> MessageIds { get; set; } = new List<int>();
        public string Action { get; set; } // "delivered", "read"
    }

    /// <summary>
    /// Response DTO for bulk message status operations
    /// </summary>
    public class BulkMessageStatusResponse
    {
        public int ProcessedCount { get; set; }
        public List<int> UpdatedMessageIds { get; set; } = new List<int>();
        public DateTime ProcessedAt { get; set; }
        public string Status { get; set; } = "Success";
        public string Message { get; set; }
    }

    /// <summary>
    /// Enhanced message DTO with comprehensive status information
    /// </summary>
    public class EnhancedChatMessageDto : ChatMessageDto
    {
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public MessageStatusType CurrentStatus { get; set; }
        public List<MessageStatusDetailDto> StatusHistory { get; set; } = new List<MessageStatusDetailDto>();
    }

    /// <summary>
    /// Real-time status update notification
    /// </summary>
    public class MessageStatusUpdateDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string UserId { get; set; }
        public MessageStatusType Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Message delivery confirmation
    /// </summary>
    public class MessageDeliveryConfirmationDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string DeliveredToUserId { get; set; }
        public DateTime DeliveredAt { get; set; }
        public bool IsDelivered { get; set; } = true;
    }
}