using Domain.Entities.ProductEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class ProductsWithBrandAndTypeByIdsSpecifications : Specifications<Product>
    {
        public ProductsWithBrandAndTypeByIdsSpecifications(IEnumerable<int> ids)
            : base(product => ids.Contains(product.Id))
        {
            AddInclude(product => product.ProductBrand);
            AddInclude(product => product.ProductType);
        }
    }
} 

