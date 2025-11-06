using System;
using System.Collections.Generic;

namespace Shared.PropertyViewHistoryModels
{
    public record PropertyViewHistoryDTO
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string PropertyDescription { get; set; }
        public decimal PropertyPrice { get; set; }
        public string PropertyMainImageUrl { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    public record RecentViewedPropertiesDTO
    {
        public List<PropertyViewHistoryDTO> RecentViews { get; set; } = new();
    }

    // New DTOs for property viewers feature (Pro/Premium only)
    public record PropertyViewerDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime ViewedAt { get; set; }
        public int ViewCount { get; set; } // How many times this user viewed the property
    }

    public record PropertyViewersAnalyticsDTO
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public List<PropertyViewerDTO> Viewers { get; set; } = new();
        public DateTime? LastViewedAt { get; set; }
        public DateTime? FirstViewedAt { get; set; }
    }

    public record PropertyViewAnalyticsRequestDTO
    {
        public int PropertyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Limit { get; set; } = 50; // Default limit for viewers
    }

    public record MostViewedPropertyDTO
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public decimal Area { get; set; }
        public string MainImageUrl { get; set; }
        public string AdditionalImages { get; set; }
        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public string? DeveloperName { get; set; }
        public string? BrokerName { get; set; }
        public string OwnerType { get; set; }
        public int ViewCount { get; set; }
    }
} 