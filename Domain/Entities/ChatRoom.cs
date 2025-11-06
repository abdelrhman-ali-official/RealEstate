using System;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.SecurityEntities;

namespace Domain.Entities
{
    public class ChatRoom : BaseEntity<int>
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        public string User1Id { get; set; } // Customer
        public User User1 { get; set; }
        public string User2Id { get; set; } // Broker/Developer
        public User User2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 