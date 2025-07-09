using Domain.Entities;
using Domain.Contracts;

namespace Services.Specifications
{
    public class PropertyViewHistoryByUserSpecifications : Specifications<PropertyViewHistory>
    {
        public PropertyViewHistoryByUserSpecifications(string userId)
            : base(v => v.UserId == userId)
        {
            AddInclude(v => v.Property);
            setOrderByDescending(v => v.ViewedAt);
        }
    }

    public class PropertyViewHistoryByPropertySpecifications : Specifications<PropertyViewHistory>
    {
        public PropertyViewHistoryByPropertySpecifications(int propertyId)
            : base(v => v.PropertyId == propertyId)
        {
        }
    }

    public class PropertyViewHistoryWithPropertySpecifications : Specifications<PropertyViewHistory>
    {
        public PropertyViewHistoryWithPropertySpecifications()
            : base(_ => true)
        {
            AddInclude(v => v.Property);
            AddInclude(v => v.Property.Developer);
            AddInclude(v => v.Property.Broker);
        }
    }
} 