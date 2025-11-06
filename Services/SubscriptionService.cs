using AutoMapper;
using Domain.Contracts;
using Domain.Entities.BrokerEntities;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.SecurityEntities;
using Domain.Entities.SubscriptionEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Services.Specifications;
using Shared.SubscriptionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public SubscriptionService(IUnitOFWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            var packageRepository = _unitOfWork.GetRepository<Package, int>();
            var packages = await packageRepository.GetAllAsync(new PackageSpecifications());
            return _mapper.Map<IEnumerable<PackageDto>>(packages);
        }

        public async Task<PackageDto?> GetPackageByIdAsync(int packageId)
        {
            var packageRepository = _unitOfWork.GetRepository<Package, int>();
            var package = await packageRepository.GetAsync(packageId);
            return package != null ? _mapper.Map<PackageDto>(package) : null;
        }

        public async Task<SubscriptionResponse> SubscribeAsync(string userId, CreateSubscriptionRequest request)
        {
            try
            {
                var packageRepository = _unitOfWork.GetRepository<Package, int>();
                var subscriptionRepository = _unitOfWork.GetRepository<Subscription, int>();
                var brokerRepository = _unitOfWork.GetRepository<Broker, int>();
                var developerRepository = _unitOfWork.GetRepository<Developer, int>();

                // Check if package exists and is active
                var package = await packageRepository.GetAsync(request.PackageId);
                if (package == null || !package.IsActive)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Package not found or inactive."
                    };
                }

                // Find user type (Broker or Developer)
                var broker = await brokerRepository.GetAsync(new BrokerWithUserSpecifications(userId));
                var developer = await developerRepository.GetAsync(new DeveloperWithUserSpecifications(userId));

                if (broker == null && developer == null)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User must be a registered Broker or Developer."
                    };
                }

                // Check for existing active subscription
                var existingSubscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(
                    brokerId: broker?.Id, 
                    developerId: developer?.Id, 
                    isActive: true));

                if (existingSubscription != null)
                {
                    // Deactivate existing subscription
                    existingSubscription.IsActive = false;
                    existingSubscription.UpdatedAt = DateTime.UtcNow;
                    subscriptionRepository.Update(existingSubscription);
                }

                // Validate PlanType
                if (request.PlanType != "Monthly" && request.PlanType != "Yearly")
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Invalid plan type. Must be 'Monthly' or 'Yearly'."
                    };
                }

                // Calculate expiry date based on plan type
                DateTime expiryDate;
                if (request.PlanType == "Monthly")
                {
                    expiryDate = DateTime.UtcNow.AddMonths(1);
                }
                else // Yearly
                {
                    expiryDate = DateTime.UtcNow.AddYears(1);
                }

                // Create new subscription
                var newSubscription = new Subscription
                {
                    PackageId = request.PackageId,
                    BrokerId = broker?.Id,
                    DeveloperId = developer?.Id,
                    SubscribedAt = DateTime.UtcNow,
                    ExpiresAt = expiryDate,
                    PlanType = request.PlanType,
                    IsActive = true,
                    CurrentPropertyCount = 0
                };

                await subscriptionRepository.AddAsync(newSubscription);
                await _unitOfWork.SaveChangesAsync();

                // Get the complete subscription with package
                var completeSubscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(newSubscription.Id));

                return new SubscriptionResponse
                {
                    Success = true,
                    Message = $"Successfully subscribed to {package.Name} package.",
                    Subscription = _mapper.Map<SubscriptionDto>(completeSubscription)
                };
            }
            catch (Exception ex)
            {
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = $"An error occurred while processing subscription: {ex.Message}"
                };
            }
        }

        public async Task<UserSubscriptionInfoDto?> GetCurrentUserSubscriptionAsync(string userId)
        {
            var brokerRepository = _unitOfWork.GetRepository<Broker, int>();
            var developerRepository = _unitOfWork.GetRepository<Developer, int>();
            var subscriptionRepository = _unitOfWork.GetRepository<Subscription, int>();
            var propertyRepository = _unitOfWork.GetRepository<Property, int>();

            // Find user type
            var broker = await brokerRepository.GetAsync(new BrokerWithUserSpecifications(userId));
            var developer = await developerRepository.GetAsync(new DeveloperWithUserSpecifications(userId));

            if (broker == null && developer == null)
                return null;

            // Get active subscription
            var subscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(
                brokerId: broker?.Id, 
                developerId: developer?.Id, 
                isActive: true));

            if (subscription == null)
                return null;

            // Count current properties
            var propertiesUsed = await propertyRepository.CountAsync(new PropertyWithDeveloperSpecifications(
                new Shared.DeveloperModels.PropertySpecificationsParameters
                {
                    BrokerId = broker?.Id,
                    DeveloperId = developer?.Id
                }));

            var propertiesRemaining = subscription.Package.PropertyLimit == -1 ? 
                int.MaxValue : 
                Math.Max(0, subscription.Package.PropertyLimit - propertiesUsed);

            return new UserSubscriptionInfoDto
            {
                CurrentSubscription = _mapper.Map<SubscriptionDto>(subscription),
                PropertiesUsed = propertiesUsed,
                PropertiesRemaining = propertiesRemaining,
                CanCreateProperty = subscription.Package.PropertyLimit == -1 || propertiesUsed < subscription.Package.PropertyLimit,
                Package = _mapper.Map<PackageDto>(subscription.Package)
            };
        }

        public async Task<bool> CanCreatePropertyAsync(string userId)
        {
            var userSubscription = await GetCurrentUserSubscriptionAsync(userId);
            return userSubscription?.CanCreateProperty ?? false;
        }

        public async Task<bool> UpdatePropertyCountAsync(string userId, int propertyCountChange)
        {
            var subscriptionRepository = _unitOfWork.GetRepository<Subscription, int>();
            var brokerRepository = _unitOfWork.GetRepository<Broker, int>();
            var developerRepository = _unitOfWork.GetRepository<Developer, int>();

            // Find user type
            var broker = await brokerRepository.GetAsync(new BrokerWithUserSpecifications(userId));
            var developer = await developerRepository.GetAsync(new DeveloperWithUserSpecifications(userId));

            if (broker == null && developer == null)
                return false;

            // Get active subscription
            var subscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(
                brokerId: broker?.Id, 
                developerId: developer?.Id, 
                isActive: true));

            if (subscription == null)
                return false;

            subscription.CurrentPropertyCount += propertyCountChange;
            subscription.UpdatedAt = DateTime.UtcNow;

            subscriptionRepository.Update(subscription);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<SubscriptionDto?> GetActiveSubscriptionByUserIdAsync(string userId)
        {
            var userSubscription = await GetCurrentUserSubscriptionAsync(userId);
            return userSubscription?.CurrentSubscription;
        }

        public async Task<bool> HasFeatureAccessAsync(string userId, string featureName)
        {
            var userSubscription = await GetCurrentUserSubscriptionAsync(userId);
            if (userSubscription?.Package == null)
                return false;

            var package = userSubscription.Package;

            return featureName.ToLower() switch
            {
                "propertyviews" => package.ShowPropertyViews,
                "wishlistnotifications" => package.ShowWishlistNotifications,
                "wishlistuserdetails" => package.ShowWishlistUserDetails,
                "funneltracking" => package.FunnelTracking,
                "exportleads" => package.ExportLeads,
                "directcontact" => package.DirectContactSystem,
                "whatsappintegration" => package.WhatsAppIntegration,
                _ => false
            };
        }

        public async Task<bool> HasPremiumSubscriptionAsync(string userId)
        {
            var userSubscription = await GetCurrentUserSubscriptionAsync(userId);
            if (userSubscription?.Package == null)
                return false;

            // Premium users are those with Pro or Premium packages
            return userSubscription.Package.Name.Equals("Pro", StringComparison.OrdinalIgnoreCase) ||
                   userSubscription.Package.Name.Equals("Premium", StringComparison.OrdinalIgnoreCase);
        }
    }
}