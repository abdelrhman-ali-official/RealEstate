using Domain.Entities.SubscriptionEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class PackageSpecifications : Specifications<Package>
    {
        public PackageSpecifications() : base(p => p.IsActive)
        {
        }

        public PackageSpecifications(int id) : base(p => p.Id == id)
        {
        }

        public PackageSpecifications(string name) : base(p => p.Name == name && p.IsActive)
        {
        }
    }
}