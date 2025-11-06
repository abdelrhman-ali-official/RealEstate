using Shared.SubscriptionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync();
        Task<PackageDto?> GetPackageByIdAsync(int packageId);
        Task<SubscriptionResponse> SubscribeAsync(string userId, CreateSubscriptionRequest request);
        Task<UserSubscriptionInfoDto?> GetCurrentUserSubscriptionAsync(string userId);
        Task<bool> CanCreatePropertyAsync(string userId);
        Task<bool> UpdatePropertyCountAsync(string userId, int propertyCountChange);
        Task<SubscriptionDto?> GetActiveSubscriptionByUserIdAsync(string userId);
        Task<bool> HasFeatureAccessAsync(string userId, string featureName);
        Task<bool> HasPremiumSubscriptionAsync(string userId);
    }
}