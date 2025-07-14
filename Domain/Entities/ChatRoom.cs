using System;
using Domain.Entities;

namespace Domain.Entities
{
    public class ChatRoom : BaseEntity<int>
    {
        public int PropertyId { get; set; }
        public string User1Id { get; set; } // Customer
        public string User2Id { get; set; } // Broker/Developer
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 