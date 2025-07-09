using Shared.WishListModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IWishListService
    {
        Task<bool> AddToWishListAsync(int propertyId, string userId);
        Task<IEnumerable<WishListItemDTO>> GetUserWishListAsync(string userId);
        Task<bool> RemoveFromWishListAsync(int propertyId, string userId);
        Task<WishListCountDTO> GetWishListCountAsync(string userId);
        Task<IEnumerable<MostWishedPropertyDTO>> GetMostWishedPropertiesAsync(WishListFilterDTO filter);
        Task<PropertyWishListUsersDTO> GetPropertyWishListUsersAsync(int propertyId);
        Task<bool> IsPropertyInWishListAsync(int propertyId, string userId);
    }
} 