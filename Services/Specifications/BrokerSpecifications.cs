using Domain.Entities.BrokerEntities;
using Microsoft.EntityFrameworkCore;

namespace Services.Specifications
{
    public class BrokerByUserIdSpecification : Specifications<Broker>
    {
        public BrokerByUserIdSpecification(string userId)
            : base(b => b.UserId == userId)
        {
        }
    }
}
