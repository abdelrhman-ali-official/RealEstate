using Domain.Entities.BrokerEntities;
using Domain.Entities.DeveloperEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SubscriptionEntities
{
    public class Subscription : BaseEntity<int>
    {
        public int PackageId { get; set; }
        public Package Package { get; set; }
        
        // User identification - either Broker or Developer
        public int? BrokerId { get; set; }
        public Broker? Broker { get; set; }
        
        public int? DeveloperId { get; set; }
        public Developer? Developer { get; set; }
        
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } // For future use if you want to add expiration
        public bool IsActive { get; set; } = true;
        public string PlanType { get; set; } = "Monthly"; // "Monthly" or "Yearly"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Property count tracking
        public int CurrentPropertyCount { get; set; } = 0;
    }
}