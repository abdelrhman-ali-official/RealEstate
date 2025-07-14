using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Services.Abstractions;
using Shared.ChatModels;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IChatHubContext _hubContext;
        private readonly StoreContext _context;

        public ChatService(IUnitOFWork unitOfWork, IMapper mapper, IChatHubContext hubContext, StoreContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _context = context;
        }

        public async Task<ChatRoomDto> StartOrGetChatRoomAsync(int propertyId, string user1Id, string user2Id)
        {
            var repo = _unitOfWork.GetRepository<ChatRoom, int>();
            var existing = await repo.GetAllAsQueryable()
                .FirstOrDefaultAsync(r => r.PropertyId == propertyId &&
                                         ((r.User1Id == user1Id && r.User2Id == user2Id) ||
                                          (r.User1Id == user2Id && r.User2Id == user1Id)));
            if (existing != null)
                return _mapper.Map<ChatRoomDto>(existing);

            var chatRoom = new ChatRoom
            {
                PropertyId = propertyId,
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow
            };
            await repo.AddAsync(chatRoom);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ChatRoomDto>(chatRoom);
        }

        public async Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<ChatRoom, int>();
            var rooms = await repo.GetAllAsQueryable()
                .Where(r => r.User1Id == userId || r.User2Id == userId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ChatRoomDto>>(rooms);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int chatRoomId, string userId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(chatRoomId);
            if (room == null || (room.User1Id != userId && room.User2Id != userId))
                throw new UnauthorizedAccessException();

            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var messages = await msgRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var messageIds = messages.Select(m => m.Id).ToList();
            var reactionRepo = _unitOfWork.GetRepository<ChatMessageReaction, int>();
            var reactions = await reactionRepo.GetAllAsQueryable()
                .Where(r => messageIds.Contains(r.MessageId))
                .ToListAsync();

            var messageDtos = messages.Select(m => {
                var dto = _mapper.Map<ChatMessageDto>(m);
                dto.Reactions = reactions.Where(r => r.MessageId == m.Id)
                                        .Select(r => _mapper.Map<ChatMessageReactionDto>(r)).ToList();
                return dto;
            }).ToList();

            return messageDtos;
        }

        public async Task<ChatMessageDto> SendMessageAsync(int chatRoomId, string senderId, string message, int? repliedToMessageId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(chatRoomId);
            if (room == null || (room.User1Id != senderId && room.User2Id != senderId))
                throw new UnauthorizedAccessException();

            var msg = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsDelivered = false,
                IsRead = false,
                RepliedToMessageId = repliedToMessageId
            };
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            await msgRepo.AddAsync(msg);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatMessageDto>(msg);
        }

        public async Task MarkMessageAsDeliveredAsync(int messageId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var message = await msgRepo.GetAsync(messageId);
            if (message != null && !message.IsDelivered)
            {
                message.IsDelivered = true;
                await _unitOfWork.SaveChangesAsync();
                
                // Broadcast delivery status via SignalR
                await _hubContext.SendMessageDeliveredAsync(message.ChatRoomId.ToString(), messageId, "recipient");
            }
        }

        public async Task<MessageReadDto> MarkMessageAsReadAsync(int messageId, string userId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var message = await msgRepo.GetAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found");

            // Verify user is a participant in the chat room
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(message.ChatRoomId);
            if (room == null || (room.User1Id != userId && room.User2Id != userId))
                throw new UnauthorizedAccessException();

            // Only mark as read if the user is not the sender
            if (message.SenderId != userId && !message.IsRead)
            {
                message.IsRead = true;
                await _unitOfWork.SaveChangesAsync();

                var result = new MessageReadDto
                {
                    MessageId = messageId,
                    ChatRoomId = message.ChatRoomId,
                    ReadByUserId = userId,
                    ReadAt = DateTime.UtcNow
                };

                // Broadcast read status via SignalR
                await _hubContext.SendMessageReadAsync(message.ChatRoomId.ToString(), messageId, userId, result.ReadAt);

                return result;
            }

            return null;
        }

        public async Task<int> GetTotalUnreadCountAsync(string userId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();

            // Get all chat rooms for the user
            var rooms = await roomRepo.GetAllAsQueryable()
                .Where(r => r.User1Id == userId || r.User2Id == userId)
                .Select(r => r.Id)
                .ToListAsync();

            // Count unread messages in those rooms not sent by the user
            var count = await msgRepo.GetAllAsQueryable()
                .Where(m => rooms.Contains(m.ChatRoomId) && !m.IsRead && m.SenderId != userId)
                .CountAsync();

            return count;
        }

        public async Task<IEnumerable<UnreadCountPerRoomDto>> GetUnreadCountPerRoomAsync(string userId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();

            // Get all chat rooms for the user
            var rooms = await roomRepo.GetAllAsQueryable()
                .Where(r => r.User1Id == userId || r.User2Id == userId)
                .Select(r => r.Id)
                .ToListAsync();

            // Group unread messages by room
            var unreadCounts = await msgRepo.GetAllAsQueryable()
                .Where(m => rooms.Contains(m.ChatRoomId) && !m.IsRead && m.SenderId != userId)
                .GroupBy(m => m.ChatRoomId)
                .Select(g => new UnreadCountPerRoomDto
                {
                    ChatRoomId = g.Key,
                    UnreadCount = g.Count()
                })
                .ToListAsync();

            return unreadCounts;
        }

        public async Task<IEnumerable<ChatRoomSummaryDto>> GetUserChatRoomSummariesAsync(string userId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();

            // Get all rooms where the user is a participant
            var rooms = await roomRepo.GetAllAsQueryable()
                .Where(r => r.User1Id == userId || r.User2Id == userId)
                .ToListAsync();

            var roomIds = rooms.Select(r => r.Id).ToList();

            // Get all last messages for these rooms
            var lastMessages = await msgRepo.GetAllAsQueryable()
                .Where(m => roomIds.Contains(m.ChatRoomId))
                .GroupBy(m => m.ChatRoomId)
                .Select(g => new
                {
                    ChatRoomId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
                })
                .ToListAsync();

            // Get unread counts for these rooms
            var unreadCounts = await msgRepo.GetAllAsQueryable()
                .Where(m => roomIds.Contains(m.ChatRoomId) && !m.IsRead && m.SenderId != userId)
                .GroupBy(m => m.ChatRoomId)
                .Select(g => new
                {
                    ChatRoomId = g.Key,
                    UnreadCount = g.Count()
                })
                .ToListAsync();

            // Get all other user IDs
            var otherUserIds = rooms
                .Select(r => r.User1Id == userId ? r.User2Id : r.User1Id)
                .Distinct()
                .ToList();

            // Get user names in one query using StoreContext
            var users = await _context.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            // Build the summary
            var summaries = rooms.Select(r =>
            {
                var otherUserId = r.User1Id == userId ? r.User2Id : r.User1Id;
                var user = users.FirstOrDefault(u => u.Id == otherUserId);
                var lastMsg = lastMessages.FirstOrDefault(lm => lm.ChatRoomId == r.Id)?.LastMessage;
                var unread = unreadCounts.FirstOrDefault(uc => uc.ChatRoomId == r.Id)?.UnreadCount ?? 0;

                return new ChatRoomSummaryDto
                {
                    RoomId = r.Id,
                    PropertyId = r.PropertyId,
                    OtherUserId = otherUserId,
                    OtherUserName = user?.UserName ?? "Unknown",
                    LastMessage = lastMsg?.Message,
                    LastMessageSentAt = lastMsg?.SentAt,
                    UnreadMessagesCount = unread
                };
            })
            .OrderByDescending(s => s.LastMessageSentAt)
            .ToList();

            return summaries;
        }

        public async Task<IEnumerable<ChatMessageWithSenderDto>> GetChatHistoryWithSenderAsync(int chatRoomId, string userId)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var brokerRepo = _unitOfWork.GetRepository<Domain.Entities.BrokerEntities.Broker, int>();
            var developerRepo = _unitOfWork.GetRepository<Domain.Entities.DeveloperEntities.Developer, int>();

            // Validate user is a participant
            var room = await roomRepo.GetAsync(chatRoomId);
            if (room == null || (room.User1Id != userId && room.User2Id != userId))
                throw new UnauthorizedAccessException();

            // Get all messages in the room
            var messages = await msgRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var messageIds = messages.Select(m => m.Id).ToList();
            var reactionRepo = _unitOfWork.GetRepository<ChatMessageReaction, int>();
            var reactions = await reactionRepo.GetAllAsQueryable()
                .Where(r => messageIds.Contains(r.MessageId))
                .ToListAsync();

            // Get all sender IDs
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();

            // Get user info using StoreContext
            var users = await _context.Users
                .Where(u => senderIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            // Get broker logos
            var brokers = await brokerRepo.GetAllAsQueryable()
                .Where(b => senderIds.Contains(b.UserId))
                .Select(b => new { b.UserId, b.FullName, b.LogoUrl })
                .ToListAsync();

            // Get developer logos
            var developers = await developerRepo.GetAllAsQueryable()
                .Where(d => senderIds.Contains(d.UserId))
                .Select(d => new { d.UserId, d.CompanyName, d.LogoUrl })
                .ToListAsync();

            // Build DTOs
            var result = messages.Select(m =>
            {
                var user = users.FirstOrDefault(u => u.Id == m.SenderId);
                var broker = brokers.FirstOrDefault(b => b.UserId == m.SenderId);
                var developer = developers.FirstOrDefault(d => d.UserId == m.SenderId);

                string senderName = user?.UserName ?? broker?.FullName ?? developer?.CompanyName ?? "Unknown";
                string logoUrl = broker?.LogoUrl ?? developer?.LogoUrl ?? null;

                var dto = new ChatMessageWithSenderDto
                {
                    Id = m.Id,
                    ChatRoomId = m.ChatRoomId,
                    SenderId = m.SenderId,
                    SenderName = senderName,
                    SenderLogoUrl = logoUrl,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsDelivered = m.IsDelivered,
                    IsRead = m.IsRead,
                    RepliedToMessageId = m.RepliedToMessageId,
                    Reactions = reactions.Where(r => r.MessageId == m.Id)
                                        .Select(r => _mapper.Map<ChatMessageReactionDto>(r)).ToList()
                };
                return dto;
            }).ToList();

            return result;
        }

        public async Task<ChatMessageReactionDto> AddReactionAsync(int messageId, string userId, string reactionType)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var message = await msgRepo.GetAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found");

            // Only one reaction per user per message
            var reactionRepo = _unitOfWork.GetRepository<ChatMessageReaction, int>();
            var existing = await reactionRepo.GetAllAsQueryable()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);
            if (existing != null)
            {
                existing.ReactionType = reactionType;
                existing.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<ChatMessageReactionDto>(existing);
            }

            var reaction = new ChatMessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow
            };
            await reactionRepo.AddAsync(reaction);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ChatMessageReactionDto>(reaction);
        }

        public async Task RemoveReactionAsync(int messageId, string userId)
        {
            var reactionRepo = _unitOfWork.GetRepository<ChatMessageReaction, int>();
            var existing = await reactionRepo.GetAllAsQueryable()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);
            if (existing != null)
            {
                reactionRepo.Delete(existing);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
} 