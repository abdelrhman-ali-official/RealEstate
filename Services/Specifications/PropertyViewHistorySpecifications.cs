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

    public class PropertyViewHistoryByUserAndPropertySpecifications : Specifications<PropertyViewHistory>
    {
        public PropertyViewHistoryByUserAndPropertySpecifications(string userId, int propertyId)
            : base(v => v.UserId == userId && v.PropertyId == propertyId)
        {
            setOrderByDescending(v => v.ViewedAt);
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

    // New specification for property viewers analytics (includes user details)
    public class PropertyViewHistoryWithUserSpecifications : Specifications<PropertyViewHistory>
    {
        public PropertyViewHistoryWithUserSpecifications(int propertyId)
            : base(v => v.PropertyId == propertyId)
        {
            AddInclude(v => v.User);
            setOrderByDescending(v => v.ViewedAt);
        }
    }
} 