using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.BrokerEntities;
using Services.Abstractions;
using Shared.ChatModels;
using Shared.PropertyViewHistoryModels;
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
                RepliedToMessageId = repliedToMessageId > 0 ? repliedToMessageId : null
            };
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            await msgRepo.AddAsync(msg);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatMessageDto>(msg);
        }

        public async Task MarkMessageAsDeliveredAsync(int messageId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var statusRepo = _unitOfWork.GetRepository<ChatMessageStatus, int>();
            
            var message = await msgRepo.GetAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found", nameof(messageId));

            if (!message.IsDelivered)
            {
                message.IsDelivered = true;
                message.DeliveredAt = DateTime.UtcNow;
                
                // Create delivery status record
                var deliveryStatus = new ChatMessageStatus
                {
                    MessageId = messageId,
                    UserId = await GetRecipientUserIdAsync(message),
                    Status = MessageStatusType.Delivered,
                    StatusChangedAt = DateTime.UtcNow
                };
                await statusRepo.AddAsync(deliveryStatus);
                
                await _unitOfWork.SaveChangesAsync();
                
                // Broadcast delivery status via SignalR
                await _hubContext.SendMessageDeliveredAsync(message.ChatRoomId.ToString(), messageId, deliveryStatus.UserId);
            }
        }

        public async Task<MessageReadDto> MarkMessageAsReadAsync(int messageId, string userId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var statusRepo = _unitOfWork.GetRepository<ChatMessageStatus, int>();
            
            var message = await msgRepo.GetAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found", nameof(messageId));

            // Verify user is a participant in the chat room
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(message.ChatRoomId);
            if (room == null || (room.User1Id != userId && room.User2Id != userId))
                throw new UnauthorizedAccessException("User is not a participant in this chat room");

            // Only mark as read if the user is not the sender
            if (message.SenderId != userId)
            {
                // Check if already marked as read by this user
                var existingReadStatus = await statusRepo.GetAllAsQueryable()
                    .FirstOrDefaultAsync(s => s.MessageId == messageId && s.UserId == userId && s.Status == MessageStatusType.Read);

                if (existingReadStatus == null)
                {
                    // Mark message as read globally
                    if (!message.IsRead)
                    {
                        message.IsRead = true;
                        message.ReadAt = DateTime.UtcNow;
                    }

                    // Create read status record
                    var readStatus = new ChatMessageStatus
                    {
                        MessageId = messageId,
                        UserId = userId,
                        Status = MessageStatusType.Read,
                        StatusChangedAt = DateTime.UtcNow
                    };
                    await statusRepo.AddAsync(readStatus);
                    
                    await _unitOfWork.SaveChangesAsync();

                    var result = new MessageReadDto
                    {
                        MessageId = messageId,
                        ChatRoomId = message.ChatRoomId,
                        ReadByUserId = userId,
                        ReadAt = readStatus.StatusChangedAt
                    };

                    // Broadcast read status via SignalR
                    await _hubContext.SendMessageReadAsync(message.ChatRoomId.ToString(), messageId, userId, result.ReadAt);

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Marks multiple messages as delivered for a specific user (bulk operation)
        /// </summary>
        public async Task MarkMessagesAsDeliveredAsync(List<int> messageIds, string userId)
        {
            if (messageIds == null || !messageIds.Any())
                return;

            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var statusRepo = _unitOfWork.GetRepository<ChatMessageStatus, int>();

            var messages = await msgRepo.GetAllAsQueryable()
                .Where(m => messageIds.Contains(m.Id) && !m.IsDelivered && m.SenderId != userId)
                .ToListAsync();

            if (messages.Any())
            {
                var statusesToAdd = new List<ChatMessageStatus>();
                var now = DateTime.UtcNow;

                foreach (var message in messages)
                {
                    message.IsDelivered = true;
                    message.DeliveredAt = now;
                    
                    statusesToAdd.Add(new ChatMessageStatus
                    {
                        MessageId = message.Id,
                        UserId = userId,
                        Status = MessageStatusType.Delivered,
                        StatusChangedAt = now
                    });
                }

                // Add each status individually since AddRangeAsync is not available
                foreach (var status in statusesToAdd)
                {
                    await statusRepo.AddAsync(status);
                }
                await _unitOfWork.SaveChangesAsync();

                // Broadcast bulk delivery status
                foreach (var message in messages)
                {
                    await _hubContext.SendMessageDeliveredAsync(message.ChatRoomId.ToString(), message.Id, userId);
                }
            }
        }

        /// <summary>
        /// Marks all messages in a chat room as read for a specific user
        /// </summary>
        public async Task MarkAllMessagesAsReadAsync(int chatRoomId, string userId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var statusRepo = _unitOfWork.GetRepository<ChatMessageStatus, int>();

            // Verify user access to chat room
            if (!await CanUserAccessChatRoomAsync(chatRoomId, userId))
                throw new UnauthorizedAccessException("User does not have access to this chat room");

            var unreadMessages = await msgRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId && 
                           m.SenderId != userId && 
                           !statusRepo.GetAllAsQueryable().Any(s => s.MessageId == m.Id && s.UserId == userId && s.Status == MessageStatusType.Read))
                .ToListAsync();

            if (unreadMessages.Any())
            {
                var statusesToAdd = new List<ChatMessageStatus>();
                var now = DateTime.UtcNow;

                foreach (var message in unreadMessages)
                {
                    if (!message.IsRead)
                    {
                        message.IsRead = true;
                        message.ReadAt = now;
                    }

                    statusesToAdd.Add(new ChatMessageStatus
                    {
                        MessageId = message.Id,
                        UserId = userId,
                        Status = MessageStatusType.Read,
                        StatusChangedAt = now
                    });
                }

                // Add each status individually since AddRangeAsync is not available
                foreach (var status in statusesToAdd)
                {
                    await statusRepo.AddAsync(status);
                }
                await _unitOfWork.SaveChangesAsync();

                // Broadcast read status for all messages
                foreach (var message in unreadMessages)
                {
                    await _hubContext.SendMessageReadAsync(chatRoomId.ToString(), message.Id, userId, now);
                }
            }
        }

        /// <summary>
        /// Gets the delivery and read status for a specific message
        /// </summary>
        public async Task<MessageStatusSummaryDto> GetMessageStatusAsync(int messageId, string requestingUserId)
        {
            var msgRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var statusRepo = _unitOfWork.GetRepository<ChatMessageStatus, int>();

            var message = await msgRepo.GetAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found", nameof(messageId));

            // Verify requesting user has access
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(message.ChatRoomId);
            if (room == null || (room.User1Id != requestingUserId && room.User2Id != requestingUserId))
                throw new UnauthorizedAccessException("User does not have access to this message");

            var statuses = await statusRepo.GetAllAsQueryable()
                .Where(s => s.MessageId == messageId)
                .ToListAsync();

            return new MessageStatusSummaryDto
            {
                MessageId = messageId,
                IsDelivered = message.IsDelivered,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                DeliveredAt = message.DeliveredAt,
                ReadAt = message.ReadAt,
                DetailedStatuses = statuses.Select(s => new MessageStatusDetailDto
                {
                    UserId = s.UserId,
                    Status = s.Status.ToString(),
                    StatusChangedAt = s.StatusChangedAt
                }).ToList()
            };
        }

        /// <summary>
        /// Helper method to get recipient user ID for a message
        /// </summary>
        private async Task<string> GetRecipientUserIdAsync(ChatMessage message)
        {
            var roomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var room = await roomRepo.GetAsync(message.ChatRoomId);
            
            if (room == null)
                throw new InvalidOperationException("Chat room not found for message");

            return room.User1Id == message.SenderId ? room.User2Id : room.User1Id;
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

        public async Task<ChatRoomDto?> StartChatWithPropertyOwnerAsync(int propertyId, string userId)
        {
            // Get property details to find the owner
            var propertyRepo = _unitOfWork.GetRepository<Property, int>();
            var property = await propertyRepo.GetAllAsQueryable()
                .Include(p => p.Broker)
                .Include(p => p.Developer)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return null;

            // Determine the property owner's user ID
            string? ownerUserId = null;
            if (property.BrokerId.HasValue && property.Broker != null)
            {
                ownerUserId = property.Broker.UserId;
            }
            else if (property.DeveloperId.HasValue && property.Developer != null)
            {
                ownerUserId = property.Developer.UserId;
            }

            if (string.IsNullOrEmpty(ownerUserId))
                return null;

            // Don't allow users to chat with themselves
            if (ownerUserId == userId)
                return null;

            // Use the existing method to start or get the chat room
            return await StartOrGetChatRoomAsync(propertyId, userId, ownerUserId);
        }

        public async Task<bool> CanPropertyOwnerContactViewerAsync(int propertyId, string propertyOwnerId, string viewerUserId)
        {
            // Check if the user owns the property and has Premium subscription
            var propertyRepo = _unitOfWork.GetRepository<Property, int>();
            var property = await propertyRepo.GetAllAsQueryable()
                .Include(p => p.Broker)
                .Include(p => p.Developer)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return false;

            // Check if the user is the property owner
            bool isOwner = false;
            if (property.BrokerId.HasValue && property.Broker != null && property.Broker.UserId == propertyOwnerId)
            {
                isOwner = true;
            }
            else if (property.DeveloperId.HasValue && property.Developer != null && property.Developer.UserId == propertyOwnerId)
            {
                isOwner = true;
            }

            if (!isOwner)
                return false;

            // Check if the viewer actually viewed this property
            var viewHistoryRepo = _unitOfWork.GetRepository<PropertyViewHistory, int>();
            var hasViewed = await viewHistoryRepo.GetAllAsQueryable()
                .AnyAsync(v => v.PropertyId == propertyId && v.UserId == viewerUserId);

            if (!hasViewed)
                return false;

            // Use the existing PropertyViewHistoryService to check Premium subscription and property ownership
            // This reuses the same validation logic as the analytics endpoint
            var propertyViewHistoryService = new PropertyViewHistoryService(_unitOfWork, _mapper);
            return await propertyViewHistoryService.CanUserViewPropertyAnalyticsAsync(propertyOwnerId, propertyId);
        }

        /// <summary>
        /// Check if the current user is the owner of the specified property
        /// </summary>
        public async Task<bool> IsUserPropertyOwnerAsync(int propertyId, string userId)
        {
            var propertyRepo = _unitOfWork.GetRepository<Property, int>();
            var property = await propertyRepo.GetAllAsQueryable()
                .Include(p => p.Broker)
                .Include(p => p.Developer)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return false;

            // Check if user is the broker owner
            if (property.BrokerId.HasValue && property.Broker != null && property.Broker.UserId == userId)
            {
                return true;
            }

            // Check if user is the developer owner
            if (property.DeveloperId.HasValue && property.Developer != null && property.Developer.UserId == userId)
            {
                return true;
            }

            return false;
        }

        public async Task<ChatRoomDto> StartChatWithViewerAndSendMessageAsync(int propertyId, string senderId, string receiverId, string message)
        {
            // First, create or get the chat room
            var chatRoom = await StartOrGetChatRoomAsync(propertyId, senderId, receiverId);
            
            // If a message is provided, send it immediately
            if (!string.IsNullOrWhiteSpace(message))
            {
                await SendMessageAsync(chatRoom.Id, senderId, message, null);
            }
            
            return chatRoom;
        }

        public async Task<bool> CanUserAccessChatRoomAsync(int chatRoomId, string userId)
        {
            var chatRoomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var chatRoom = await chatRoomRepo.GetAsync(chatRoomId);
            
            if (chatRoom == null)
                return false;
                
            return chatRoom.User1Id == userId || chatRoom.User2Id == userId;
        }

        public async Task UpdateUserConnectionAsync(string userId, string connectionId, bool isOnline)
        {
            var connectionRepo = _unitOfWork.GetRepository<UserConnection, int>();
            var existingConnection = await connectionRepo.GetAllAsQueryable()
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ConnectionId == connectionId);

            if (existingConnection != null)
            {
                existingConnection.IsOnline = isOnline;
                existingConnection.LastSeenAt = DateTime.UtcNow;
                connectionRepo.Update(existingConnection);
            }
            else if (isOnline)
            {
                var newConnection = new UserConnection
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    IsOnline = true,
                    ConnectedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                await connectionRepo.AddAsync(newConnection);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserLastSeenAsync(string userId)
        {
            var connectionRepo = _unitOfWork.GetRepository<UserConnection, int>();
            var userConnections = await connectionRepo.GetAllAsQueryable()
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            foreach (var connection in userConnections)
            {
                connection.LastSeenAt = DateTime.UtcNow;
                connectionRepo.Update(connection);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetOnlineUsersInRoomAsync(int chatRoomId)
        {
            var chatRoomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var chatRoom = await chatRoomRepo.GetAsync(chatRoomId);
            
            if (chatRoom == null)
                return new List<string>();

            var connectionRepo = _unitOfWork.GetRepository<UserConnection, int>();
            var onlineUsers = await connectionRepo.GetAllAsQueryable()
                .Where(uc => (uc.UserId == chatRoom.User1Id || uc.UserId == chatRoom.User2Id) && uc.IsOnline)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToListAsync();

            return onlineUsers;
        }

        public async Task MarkMessageAsDeliveredForOnlineUsersAsync(int messageId, int chatRoomId)
        {
            var onlineUsers = await GetOnlineUsersInRoomAsync(chatRoomId);
            var messageRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var message = await messageRepo.GetAsync(messageId);
            
            if (message == null)
                return;

            foreach (var userId in onlineUsers)
            {
                if (userId != message.SenderId) // Don't mark as delivered for the sender
                {
                    await MarkMessageAsDeliveredAsync(messageId);
                }
            }
        }

        public async Task<PaginatedChatHistoryDto> GetChatHistoryWithPaginationAsync(int chatRoomId, string userId, int pageNumber = 1, int pageSize = 50)
        {
            // Validate user can access this chat room
            var canAccess = await CanUserAccessChatRoomAsync(chatRoomId, userId);
            if (!canAccess)
                throw new UnauthorizedAccessException("You don't have access to this chat room");

            var messageRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var query = messageRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentAt);

            var totalMessages = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalMessages / pageSize);

            var messages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SentAt) // Order chronologically for display
                .ToListAsync();

            var messageDtos = _mapper.Map<List<ChatMessageWithSenderDto>>(messages);

            return new PaginatedChatHistoryDto
            {
                Messages = messageDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalMessages = totalMessages,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        public async Task<ChatRoomInfoDto?> GetChatRoomInfoAsync(int chatRoomId, string userId)
        {
            // Validate user can access this chat room
            var canAccess = await CanUserAccessChatRoomAsync(chatRoomId, userId);
            if (!canAccess)
                return null;

            var chatRoomRepo = _unitOfWork.GetRepository<ChatRoom, int>();
            var chatRoom = await chatRoomRepo.GetAllAsQueryable()
                .Include(cr => cr.Property)
                .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

            if (chatRoom == null)
                return null;

            // Get last message timestamp
            var messageRepo = _unitOfWork.GetRepository<ChatMessage, int>();
            var lastMessage = await messageRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            // Get unread count for current user
            var unreadCount = await messageRepo.GetAllAsQueryable()
                .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != userId && !m.IsRead)
                .CountAsync();

            // Get online users
            var onlineUserIds = await GetOnlineUsersInRoomAsync(chatRoomId);
            var onlineUsers = onlineUserIds.Select(id => new OnlineUserDto
            {
                UserId = id,
                IsOnline = true,
                LastSeen = DateTime.UtcNow
            }).ToList();

            return new ChatRoomInfoDto
            {
                Id = chatRoom.Id,
                PropertyId = chatRoom.PropertyId,
                PropertyTitle = chatRoom.Property?.Title ?? "Property",
                User1Id = chatRoom.User1Id,
                User1Name = "User 1", // You might want to include user names in the query
                User2Id = chatRoom.User2Id,
                User2Name = "User 2", // You might want to include user names in the query
                CreatedAt = chatRoom.CreatedAt,
                LastMessageAt = lastMessage?.SentAt ?? chatRoom.CreatedAt,
                UnreadCount = unreadCount,
                OnlineUsers = onlineUsers
            };
        }
    }
} 