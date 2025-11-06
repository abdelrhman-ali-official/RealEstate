using Domain.Entities.DeveloperEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DeveloperModels
{
    public record PropertyResultDTO
    {
        public int Id { get; set; }
        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public decimal Area { get; set; }
        public int? Rooms { get; set; }
        public int? Bathrooms { get; set; }
        public PropertyType Type { get; set; }
        public PropertyStatus Status { get; set; }
        public PropertyPurpose Purpose { get; set; }
        public string MainImageUrl { get; set; }
        public string AdditionalImages { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? DeveloperName { get; set; }
        public string? DeveloperCompanyName { get; set; }
        public string? BrokerName { get; set; }
        public string? BrokerAgencyName { get; set; }
        public string OwnerType { get; set; } // "Developer" or "Broker"
        
        // View statistics
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public DateTime? LastViewedAt { get; set; }
    }

    public record PropertyCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public decimal Area { get; set; }
        public int? Rooms { get; set; }
        public int? Bathrooms { get; set; }
        public PropertyType Type { get; set; }
        public PropertyStatus Status { get; set; }
        public PropertyPurpose Purpose { get; set; }
        public string MainImageUrl { get; set; }
        public string AdditionalImages { get; set; }
    }

    public record PropertyUpdateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public decimal Area { get; set; }
        public int? Rooms { get; set; }
        public int? Bathrooms { get; set; }
        public PropertyType Type { get; set; }
        public PropertyStatus Status { get; set; }
        public PropertyPurpose Purpose { get; set; }
        public string MainImageUrl { get; set; }
        public string AdditionalImages { get; set; }
    }
} 