using Shared;
using Shared.DeveloperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IPropertyService
    {
        Task<PaginatedResult<PropertyResultDTO>> GetAllPropertiesAsync(PropertySpecificationsParameters parameters);
        Task<PaginatedResult<PropertyResultDTO>> GetPropertiesForSaleAsync(PropertySpecificationsParameters parameters);
        Task<PaginatedResult<PropertyResultDTO>> GetPropertiesForRentAsync(PropertySpecificationsParameters parameters);
        Task<PropertyResultDTO?> GetPropertyByIdAsync(int id);
        Task<IEnumerable<PropertyResultDTO>> GetPropertiesByDeveloperAsync(int developerId, string userId);
        Task<IEnumerable<PropertyResultDTO>> GetPropertiesByBrokerAsync(int brokerId, string userId);
        Task<PropertyResultDTO> CreatePropertyAsync(PropertyCreateDTO propertyDto, string userId, ISubscriptionService subscriptionService);
        Task<PropertyResultDTO> UpdatePropertyAsync(int id, PropertyUpdateDTO propertyDto, string userId);
        Task<bool> DeletePropertyAsync(int id, string userId, ISubscriptionService subscriptionService);
        Task<IEnumerable<string>> GetGovernmentsAsync();
        Task<IEnumerable<string>> GetCitiesByGovernmentAsync(string government);
    }
} 