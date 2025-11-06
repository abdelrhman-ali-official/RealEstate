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
        
        // New methods for Pro/Premium property viewers feature
        Task<PropertyViewersAnalyticsDTO?> GetPropertyViewersAsync(int propertyId, string ownerId, PropertyViewAnalyticsRequestDTO? request = null);
        Task<bool> CanUserViewPropertyAnalyticsAsync(string userId, int propertyId);
        Task<List<PropertyViewersAnalyticsDTO>> GetMyPropertiesViewAnalyticsAsync(string ownerId, DateTime? fromDate = null, DateTime? toDate = null);
    }
} 