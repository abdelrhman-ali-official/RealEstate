using Domain.Contracts;
using Domain.Entities;
using System;
using System.Linq.Expressions;

namespace Services.Specifications
{
    public class ChatRoomByPropertyAndUsersSpecification : Specifications<ChatRoom>
    {
        public ChatRoomByPropertyAndUsersSpecification(int propertyId, string user1Id, string user2Id)
            : base(r => r.PropertyId == propertyId &&
                        ((r.User1Id == user1Id && r.User2Id == user2Id) ||
                         (r.User1Id == user2Id && r.User2Id == user1Id)))
        {
        }
    }

    public class ChatRoomsByUserSpecification : Specifications<ChatRoom>
    {
        public ChatRoomsByUserSpecification(string userId)
            : base(r => r.User1Id == userId || r.User2Id == userId)
        {
        }
    }
} 