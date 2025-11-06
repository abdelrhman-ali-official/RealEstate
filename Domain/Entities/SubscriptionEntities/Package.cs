using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SubscriptionEntities
{
    public class Package : BaseEntity<int>
    {
        public string Name { get; set; } // Basic, Pro, Premium
        public string Description { get; set; }
        public decimal Price { get; set; } = 0; // All packages are free for monthly
        public decimal MonthlyPrice { get; set; } = 0; // Monthly price (free for all)
        public decimal YearlyPrice { get; set; } = 10000; // Yearly price (10000 L.E for all packages)
        public int PropertyLimit { get; set; } // 10 for Basic, 50 for Pro, -1 for Premium (unlimited)
        public bool ShowPropertyViews { get; set; } = true; // Available in all packages
        public bool ShowWishlistNotifications { get; set; } = true; // Available in all packages
        public bool ShowWishlistUserDetails { get; set; } = false; // Pro and Premium only
        public bool FunnelTracking { get; set; } = false; // Pro and Premium only
        public bool ExportLeads { get; set; } = false; // Pro and Premium only
        public bool DirectContactSystem { get; set; } = false; // Premium only
        public bool WhatsAppIntegration { get; set; } = false; // Premium only
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}