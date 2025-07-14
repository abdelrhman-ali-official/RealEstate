using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.ChatModels;
using System.Collections.Generic;
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

        [HttpPost("room")]
        public async Task<ActionResult<ChatRoomDto>> StartOrGetRoom([FromBody] StartChatRoomRequest request)
        {
            var userId = GetUserId();
            var room = await _serviceManager.ChatService.StartOrGetChatRoomAsync(request.PropertyId, userId, request.OtherUserId);
            return Ok(room);
        }

        [HttpGet("rooms")]
        public async Task<ActionResult<IEnumerable<ChatRoomSummaryDto>>> GetUserRoomSummaries()
        {
            var userId = GetUserId();
            var summaries = await _serviceManager.ChatService.GetUserChatRoomSummariesAsync(userId);
            return Ok(summaries);
        }

        [HttpGet("room/{roomId}/messages")]
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

        [HttpPost("message/{messageId}/read")]
        public async Task<ActionResult<MessageReadDto>> MarkMessageAsRead(int messageId)
        {
            var userId = GetUserId();
            var result = await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
            return Ok(result);
        }

        [HttpPost("message/{messageId}/delivered")]
        public async Task<ActionResult> MarkMessageAsDelivered(int messageId)
        {
            await _serviceManager.ChatService.MarkMessageAsDeliveredAsync(messageId);
            return Ok();
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

    public class StartChatRoomRequest
    {
        public int PropertyId { get; set; }
        public string OtherUserId { get; set; }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
        public int? RepliedToMessageId { get; set; }
    }
} 