using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.ChatModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Start.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ChatController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Mark a specific message as delivered
        /// </summary>
        [HttpPost("message/{messageId}/delivered")]
        public async Task<ActionResult> MarkMessageAsDelivered(int messageId)
        {
            try
            {
                await _serviceManager.ChatService.MarkMessageAsDeliveredAsync(messageId);
                return Ok(new { Success = true, Message = "Message marked as delivered", MessageId = messageId });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to mark message as delivered", Details = ex.Message });
            }
        }

        /// <summary>
        /// Mark a specific message as read by the current user
        /// </summary>
        [HttpPost("message/{messageId}/read")]
        public async Task<ActionResult<MessageReadDto>> MarkMessageAsRead(int messageId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
                
                if (result == null)
                {
                    return BadRequest("Message already read or you are the sender");
                }
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = "Access denied", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to mark message as read", Details = ex.Message });
            }
        }

        /// <summary>
        /// Mark multiple messages as delivered (bulk operation)
        /// </summary>
        [HttpPost("messages/bulk/delivered")]
        public async Task<ActionResult<BulkMessageStatusResponse>> MarkMessagesAsDelivered([FromBody] BulkMessageStatusRequest request)
        {
            try
            {
                var userId = GetUserId();
                await _serviceManager.ChatService.MarkMessagesAsDeliveredAsync(request.MessageIds, userId);
                
                return Ok(new BulkMessageStatusResponse
                {
                    ProcessedCount = request.MessageIds.Count,
                    UpdatedMessageIds = request.MessageIds,
                    ProcessedAt = DateTime.UtcNow,
                    Status = "Success",
                    Message = $"Successfully marked {request.MessageIds.Count} messages as delivered"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to mark messages as delivered", Details = ex.Message });
            }
        }

        /// <summary>
        /// Mark all messages in a chat room as read
        /// </summary>
        [HttpPost("room/{chatRoomId}/mark-all-read")]
        public async Task<ActionResult> MarkAllMessagesAsRead(int chatRoomId)
        {
            try
            {
                var userId = GetUserId();
                await _serviceManager.ChatService.MarkAllMessagesAsReadAsync(chatRoomId, userId);
                
                return Ok(new { 
                    Success = true, 
                    Message = "All messages marked as read", 
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    ProcessedAt = DateTime.UtcNow 
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = "Access denied", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to mark messages as read", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive status information for a specific message
        /// </summary>
        [HttpGet("message/{messageId}/status")]
        public async Task<ActionResult<MessageStatusSummaryDto>> GetMessageStatus(int messageId)
        {
            try
            {
                var userId = GetUserId();
                var status = await _serviceManager.ChatService.GetMessageStatusAsync(messageId, userId);
                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = "Access denied", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to retrieve message status", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get delivery and read statistics for a chat room
        /// </summary>
        [HttpGet("room/{chatRoomId}/message-statistics")]
        public async Task<ActionResult<object>> GetChatRoomMessageStatistics(int chatRoomId)
        {
            try
            {
                var userId = GetUserId();
                
                // Verify user access
                if (!await _serviceManager.ChatService.CanUserAccessChatRoomAsync(chatRoomId, userId))
                {
                    return StatusCode(403, new { Error = "Access denied", Message = "Access denied to this chat room" });
                }

                var unreadCounts = await _serviceManager.ChatService.GetUnreadCountPerRoomAsync(userId);
                var roomUnreadCount = unreadCounts.FirstOrDefault(u => u.ChatRoomId == chatRoomId)?.UnreadCount ?? 0;

                return Ok(new
                {
                    ChatRoomId = chatRoomId,
                    UnreadMessageCount = roomUnreadCount,
                    LastUpdated = DateTime.UtcNow,
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to retrieve chat room statistics", Details = ex.Message });
            }
        }

        /// <summary>
        /// Start or get a chat room for a property
        /// Customers: Only need to provide PropertyId - system auto-detects property owner
        /// Brokers/Developers (Premium): Can optionally specify OtherUserId to contact specific viewers
        /// </summary>
        [HttpPost("room")]
        public async Task<ActionResult<ChatRoomDto>> StartOrGetRoom([FromBody] StartChatRoomRequest request)
        {
            var userId = GetUserId();
            
            try
            {
                // Scenario 1: OtherUserId is not provided - Auto-detect property owner
                if (string.IsNullOrEmpty(request.OtherUserId))
                {
                    // Use the existing method that automatically finds the property owner
                    var room = await _serviceManager.ChatService.StartChatWithPropertyOwnerAsync(request.PropertyId, userId);
                    
                    if (room == null)
                    {
                        return BadRequest(new 
                        { 
                            Error = "Unable to start chat",
                            Message = "Property not found, has no owner, or you cannot chat with yourself." 
                        });
                    }
                    
                    return Ok(room);
                }
                
                // Scenario 2: OtherUserId is provided - Direct contact (may require Premium for property owners)
                // Check if the current user is the property owner trying to contact a viewer
                var isPropertyOwner = await _serviceManager.ChatService.IsUserPropertyOwnerAsync(request.PropertyId, userId);
                
                if (isPropertyOwner)
                {
                    // Property owner trying to contact someone - check Premium subscription
                    var hasPremium = await _serviceManager.SubscriptionService.HasPremiumSubscriptionAsync(userId);
                    
                    if (!hasPremium)
                    {
                        return StatusCode(402, new 
                        { 
                            Error = "Premium subscription required",
                            Message = "Property owners need a Premium subscription to initiate contact with property viewers.",
                            Feature = "contact_viewers"
                        });
                    }
                    
                    // Verify the viewer has actually viewed the property
                    var canContact = await _serviceManager.ChatService.CanPropertyOwnerContactViewerAsync(
                        request.PropertyId, userId, request.OtherUserId);
                    
                    if (!canContact)
                    {
                        return StatusCode(403, new 
                        { 
                            Error = "Access denied", 
                            Message = "You can only contact users who have viewed your property." 
                        });
                    }
                }
                
                // Create or get the chat room
                var chatRoom = await _serviceManager.ChatService.StartOrGetChatRoomAsync(
                    request.PropertyId, userId, request.OtherUserId);
                
                return Ok(chatRoom);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "Invalid request", Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = "Access denied", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    Error = "Failed to create chat room", 
                    Message = "An unexpected error occurred. Please try again later.",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Start a chat with the property owner (Customer-friendly endpoint)
        /// Anyone can use this to contact property owners - No Premium required
        /// </summary>
        [HttpPost("property/{propertyId}/start-chat")]
        public async Task<ActionResult<ChatRoomDto>> StartChatWithPropertyOwner(int propertyId)
        {
            var userId = GetUserId();
            
            try
            {
                var room = await _serviceManager.ChatService.StartChatWithPropertyOwnerAsync(propertyId, userId);
                
                if (room == null)
                {
                    return BadRequest(new 
                    { 
                        Error = "Unable to start chat",
                        Message = "Property not found, has no owner, or you cannot start a chat with yourself." 
                    });
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    Error = "Failed to start chat", 
                    Message = "An error occurred while starting the chat.",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Contact a property viewer (Premium feature for property owners)
        /// Requires: Premium subscription, property ownership, viewer must have viewed property
        /// </summary>
        [HttpPost("property/{propertyId}/contact-viewer/{viewerUserId}")]
        public async Task<ActionResult<ChatRoomDto>> StartChatWithPropertyViewer(int propertyId, string viewerUserId, [FromBody] ContactViewerWithMessageRequest request)
        {
            var userId = GetUserId();
            
            try
            {
                // Check if user has Premium subscription and owns the property
                var canContact = await _serviceManager.ChatService.CanPropertyOwnerContactViewerAsync(propertyId, userId, viewerUserId);
                
                if (!canContact)
                {
                    return StatusCode(403, new 
                    { 
                        Error = "Access denied",
                        Message = "You don't have permission to contact this user. This feature requires a Premium subscription, property ownership, and the user must have viewed your property.",
                        Feature = "contact_viewers"
                    });
                }

                var room = await _serviceManager.ChatService.StartChatWithViewerAndSendMessageAsync(propertyId, userId, viewerUserId, request.Message);
                
                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    Error = "Failed to contact viewer", 
                    Message = "An error occurred while contacting the viewer.",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Contact a property viewer (Premium feature for property owners)
        /// Alternative endpoint - Requires: Premium subscription, property ownership, viewer must have viewed property
        /// </summary>
        [HttpPost("contact-property-viewer")]
        public async Task<ActionResult<ChatRoomDto>> ContactPropertyViewer([FromBody] ContactPropertyViewerRequest request)
        {
            var userId = GetUserId();
            
            try
            {
                // Check if user has Premium subscription and owns the property
                var canContact = await _serviceManager.ChatService.CanPropertyOwnerContactViewerAsync(request.PropertyId, userId, request.ViewerUserId);
                
                if (!canContact)
                {
                    return StatusCode(403, new 
                    { 
                        Error = "Access denied",
                        Message = "You don't have permission to contact this user. This feature requires a Premium subscription, property ownership, and the user must have viewed your property.",
                        Feature = "contact_viewers"
                    });
                }

                var room = await _serviceManager.ChatService.StartChatWithViewerAndSendMessageAsync(request.PropertyId, userId, request.ViewerUserId, request.Message);
                
                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    Error = "Failed to contact viewer", 
                    Message = "An error occurred while contacting the viewer.",
                    Details = ex.Message 
                });
            }
        }

        [HttpGet("rooms")]
        public async Task<ActionResult<IEnumerable<ChatRoomSummaryDto>>> GetUserRoomSummaries()
        {
            var userId = GetUserId();
            var summaries = await _serviceManager.ChatService.GetUserChatRoomSummariesAsync(userId);
            return Ok(summaries);
        }

        [HttpGet("room/{roomId}/info")]
        public async Task<ActionResult<ChatRoomInfoDto>> GetChatRoomInfo(int roomId)
        {
            var userId = GetUserId();
            var roomInfo = await _serviceManager.ChatService.GetChatRoomInfoAsync(roomId, userId);
            
            if (roomInfo == null)
            {
                return NotFound("Chat room not found or you don't have access to it.");
            }
            
            return Ok(roomInfo);
        }

        [HttpGet("room/{roomId}/messages")]
        public async Task<ActionResult<PaginatedChatHistoryDto>> GetChatHistoryPaginated(int roomId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var userId = GetUserId();
            var history = await _serviceManager.ChatService.GetChatHistoryWithPaginationAsync(roomId, userId, pageNumber, pageSize);
            return Ok(history);
        }

        [HttpGet("room/{roomId}/messages/all")]
        public async Task<ActionResult<IEnumerable<ChatMessageWithSenderDto>>> GetChatHistory(int roomId)
        {
            var userId = GetUserId();
            var messages = await _serviceManager.ChatService.GetChatHistoryWithSenderAsync(roomId, userId);
            return Ok(messages);
        }

        [HttpPost("message/{messageId}/reaction")]
        public async Task<ActionResult<ChatMessageReactionDto>> AddReaction(int messageId, [FromBody] string reactionType)
        {
            var userId = GetUserId();
            var reaction = await _serviceManager.ChatService.AddReactionAsync(messageId, userId, reactionType);
            return Ok(reaction);
        }

        [HttpDelete("message/{messageId}/reaction")]
        public async Task<ActionResult> RemoveReaction(int messageId)
        {
            var userId = GetUserId();
            await _serviceManager.ChatService.RemoveReactionAsync(messageId, userId);
            return Ok();
        }

        [HttpPost("room/{roomId}/message")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage(int roomId, [FromBody] SendMessageRequest request)
        {
            var userId = GetUserId();
            var message = await _serviceManager.ChatService.SendMessageAsync(roomId, userId, request.Message, request.RepliedToMessageId);
            return Ok(message);
        }

        [HttpGet("unread/count")]
        public async Task<ActionResult<int>> GetTotalUnreadCount()
        {
            var userId = GetUserId();
            var count = await _serviceManager.ChatService.GetTotalUnreadCountAsync(userId);
            return Ok(count);
        }

        [HttpGet("rooms/unread")]
        public async Task<ActionResult<IEnumerable<UnreadCountPerRoomDto>>> GetUnreadCountPerRoom()
        {
            var userId = GetUserId();
            var counts = await _serviceManager.ChatService.GetUnreadCountPerRoomAsync(userId);
            return Ok(counts);
        }
    }

    // Note: StartChatRoomRequest has been moved to Shared.ChatModels namespace
    // Keep SendMessageRequest here as it's only used in this controller
    public class SendMessageRequest
    {
        public string Message { get; set; }
        public int? RepliedToMessageId { get; set; }
    }
} 