using Domain.Entities.SecurityEntities;
using Domain.Entities.SubscriptionEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.BrokerEntities
{
    public class Broker : BaseEntity<int>
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? AgencyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
} 