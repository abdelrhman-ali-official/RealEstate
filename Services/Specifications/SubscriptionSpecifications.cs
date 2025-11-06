using Domain.Entities.SubscriptionEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class SubscriptionSpecifications : Specifications<Subscription>
    {
        public SubscriptionSpecifications(int? brokerId = null, int? developerId = null, bool? isActive = null)
            : base(s => 
                (brokerId == null || s.BrokerId == brokerId) &&
                (developerId == null || s.DeveloperId == developerId) &&
                (isActive == null || s.IsActive == isActive))
        {
            AddInclude(s => s.Package);
        }

        public SubscriptionSpecifications(int subscriptionId) : base(s => s.Id == subscriptionId)
        {
            AddInclude(s => s.Package);
        }
    }
}