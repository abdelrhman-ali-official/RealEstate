using Domain.Entities.DeveloperEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class DeveloperWithUserSpecifications : Specifications<Developer>
    {
        public DeveloperWithUserSpecifications(int id) : base(developer => developer.Id == id)
        {
            AddInclude(developer => developer.User);
        }

        public DeveloperWithUserSpecifications(string userId) : base(developer => developer.UserId == userId)
        {
            AddInclude(developer => developer.User);
        }
    }
} 