using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.BrokerEntities;

namespace Domain.Entities.DeveloperEntities
{
    public class Property : BaseEntity<int>
    {
        public int? DeveloperId { get; set; }
        public Developer? Developer { get; set; }
        public int? BrokerId { get; set; }
        public Broker? Broker { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
        public decimal Area { get; set; }
        public PropertyType Type { get; set; }
        public PropertyStatus Status { get; set; }
        public string MainImageUrl { get; set; }
        public string AdditionalImages { get; set; } // JSON string for multiple images
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PropertyType
    {
        Apartment = 1,
        Villa = 2,
        Shop = 3,
        Land = 4,
        Office = 5,
        Warehouse = 6
    }

    public enum PropertyStatus
    {
        Available = 1,
        Sold = 2,
        Pending = 3,
        Reserved = 4
    }
} 