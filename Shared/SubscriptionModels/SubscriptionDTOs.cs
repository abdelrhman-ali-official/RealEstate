using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.SubscriptionModels
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int PropertyLimit { get; set; }
        public bool ShowPropertyViews { get; set; }
        public bool ShowWishlistNotifications { get; set; }
        public bool ShowWishlistUserDetails { get; set; }
        public bool FunnelTracking { get; set; }
        public bool ExportLeads { get; set; }
        public bool DirectContactSystem { get; set; }
        public bool WhatsAppIntegration { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubscriptionDto
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public PackageDto Package { get; set; }
        public int? BrokerId { get; set; }
        public int? DeveloperId { get; set; }
        public DateTime SubscribedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string PlanType { get; set; } // "Monthly" or "Yearly"
        public int CurrentPropertyCount { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public int PackageId { get; set; }
        public string PlanType { get; set; } = "Monthly"; // "Monthly" or "Yearly"
    }

    public class SubscriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SubscriptionDto Subscription { get; set; }
    }

    public class UserSubscriptionInfoDto
    {
        public SubscriptionDto CurrentSubscription { get; set; }
        public int PropertiesUsed { get; set; }
        public int PropertiesRemaining { get; set; }
        public bool CanCreateProperty { get; set; }
        public PackageDto Package { get; set; }
    }
}