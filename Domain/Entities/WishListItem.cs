using Domain.Entities.DeveloperEntities;
using Domain.Entities.SecurityEntities;
using System;

namespace Domain.Entities
{
    public class WishListItem : BaseEntity<int>
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 