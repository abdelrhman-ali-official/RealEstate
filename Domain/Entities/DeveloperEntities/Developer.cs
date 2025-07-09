using Domain.Entities.SecurityEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.DeveloperEntities
{
    public class Developer : BaseEntity<int>
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
} 