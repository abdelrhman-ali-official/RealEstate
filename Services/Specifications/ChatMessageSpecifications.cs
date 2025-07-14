using Domain.Contracts;
using Domain.Entities;
using System;

namespace Services.Specifications
{
    public class ChatMessagesByRoomSpecification : Specifications<ChatMessage>
    {
        public ChatMessagesByRoomSpecification(int chatRoomId)
            : base(m => m.ChatRoomId == chatRoomId)
        {
            setOrderBy(m => m.SentAt);
        }
    }
} 