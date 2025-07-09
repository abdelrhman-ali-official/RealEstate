using System;
using System.Collections.Generic;

namespace Shared.PropertyViewHistoryModels
{
    public record AddPropertyViewDTO(int PropertyId);

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