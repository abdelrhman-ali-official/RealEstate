using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class UserConnection : BaseEntity<int>
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = true;
        public string? DeviceInfo { get; set; }
        public string? UserAgent { get; set; }
    }
}