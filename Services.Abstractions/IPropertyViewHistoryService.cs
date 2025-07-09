using Shared.PropertyViewHistoryModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IPropertyViewHistoryService
    {
        Task AddPropertyViewAsync(int propertyId, string userId);
        Task<List<PropertyViewHistoryDTO>> GetRecentViewedPropertiesAsync(string userId, int count = 10);
        Task<int> GetPropertyViewCountAsync(int propertyId);
        Task<List<MostViewedPropertyDTO>> GetMostViewedPropertiesAsync(int top = 1);
    }
} 