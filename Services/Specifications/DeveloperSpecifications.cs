using Domain.Entities.DeveloperEntities;
using Microsoft.EntityFrameworkCore;

namespace Services.Specifications
{
    public class DeveloperByUserIdSpecification : Specifications<Developer>
    {
        public DeveloperByUserIdSpecification(string userId)
            : base(d => d.UserId == userId)
        {
        }
    }
}
