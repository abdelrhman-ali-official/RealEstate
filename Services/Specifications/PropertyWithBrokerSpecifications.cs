using Domain.Entities.DeveloperEntities;
using Domain.Contracts;

namespace Services.Specifications
{
    public class PropertyWithBrokerSpecifications : Specifications<Property>
    {
        public PropertyWithBrokerSpecifications(int brokerId)
            : base(property => property.BrokerId == brokerId)
        {
            AddInclude(p => p.Broker);
            setOrderByDescending(p => p.CreatedAt);
        }
    }
}