global using Shared;
using Shared.ProductModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Abstractions
{
    public interface IProductService
    {
        public Task<PaginatedResult<ProductResultDTO>> GetAllProductsAsync(ProductSpecificationsParameters parameters);
        public Task<IEnumerable<BrandResultDTO>> GetAllBrandsAsync();
        public Task<IEnumerable<TypeResultDTO>> GetAllTypesAsync();
        public Task<ProductResultDTO?> GetProductByIdAsync(int id);
        public Task<IEnumerable<ProductResultDTO>> GetProductsByIdsAsync(IEnumerable<int> ids);
    }
}
