using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.BrokerEntities;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.SubscriptionEntities;
using Services.Abstractions;
using Shared.DeveloperModels;
using Shared.PropertyViewHistoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Specifications;

namespace Services
{
    public class PropertyViewHistoryService : IPropertyViewHistoryService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public PropertyViewHistoryService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddPropertyViewAsync(int propertyId, string userId)
        {
            // For testing - always add view (we can add duplicate prevention later)
            var view = new PropertyViewHistory
            {
                UserId = userId,
                PropertyId = propertyId,
                ViewedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<PropertyViewHistory, int>().AddAsync(view);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<PropertyViewHistoryDTO>> GetRecentViewedPropertiesAsync(string userId, int count = 10)
        {
            var views = await _unitOfWork.GetRepository<PropertyViewHistory, int>()
                .GetAllAsync(new PropertyViewHistoryByUserSpecifications(userId));
            // Only the most recent view per property
            var recent = views
                .GroupBy(v => v.PropertyId)
                .Select(g => g.OrderByDescending(v => v.ViewedAt).First())
                .OrderByDescending(v => v.ViewedAt)
                .Take(count)
                .ToList();
            return _mapper.Map<List<PropertyViewHistoryDTO>>(recent);
        }

        public async Task<int> GetPropertyViewCountAsync(int propertyId)
        {
            var count = await _unitOfWork.GetRepository<PropertyViewHistory, int>()
                .CountAsync(new PropertyViewHistoryByPropertySpecifications(propertyId));
            return count;
        }

        public async Task<List<MostViewedPropertyDTO>> GetMostViewedPropertiesAsync(int top = 1)
        {
            var views = await _unitOfWork.GetRepository<PropertyViewHistory, int>()
                .GetAllAsync(new PropertyViewHistoryWithPropertySpecifications());

            var grouped = views
                .GroupBy(v => v.PropertyId)
                .Select(g => new MostViewedPropertyDTO
                {
                    PropertyId = g.Key,
                    Title = g.First().Property.Title,
                    Description = g.First().Property.Description,
                    Price = g.First().Property.Price,
                    Government = g.First().Property.Government,
                    City = g.First().Property.City,
                    FullAddress = g.First().Property.FullAddress,
                    Area = g.First().Property.Area,
                    MainImageUrl = g.First().Property.MainImageUrl,
                    AdditionalImages = g.First().Property.AdditionalImages,
                    DeveloperId = g.First().Property.DeveloperId,
                    BrokerId = g.First().Property.BrokerId,
                    DeveloperName = g.First().Property.Developer?.User?.DisplayName,
                    BrokerName = g.First().Property.Broker?.FullName,
                    OwnerType = g.First().Property.DeveloperId.HasValue ? "Developer" : "Broker",
                    ViewCount = g.Count()
                })
                .OrderByDescending(x => x.ViewCount)
                .ThenByDescending(x => x.PropertyId)
                .Take(top)
                .ToList();

            return grouped;
        }

        // New methods for Pro/Premium property viewers feature
        public async Task<PropertyViewersAnalyticsDTO?> GetPropertyViewersAsync(int propertyId, string ownerId, PropertyViewAnalyticsRequestDTO? request = null)
        {
            // First, check if the user can view analytics for this property
            if (!await CanUserViewPropertyAnalyticsAsync(ownerId, propertyId))
            {
                return null;
            }

            var propertyRepository = _unitOfWork.GetRepository<Property, int>();
            var property = await propertyRepository.GetAsync(propertyId);
            
            if (property == null)
                return null;

            // Get property views with user details
            var viewRepository = _unitOfWork.GetRepository<PropertyViewHistory, int>();
            var views = await viewRepository.GetAllAsync(new PropertyViewHistoryWithUserSpecifications(propertyId));

            // Apply date filters if provided
            if (request?.FromDate.HasValue == true)
            {
                views = views.Where(v => v.ViewedAt >= request.FromDate.Value).ToList();
            }
            if (request?.ToDate.HasValue == true)
            {
                views = views.Where(v => v.ViewedAt <= request.ToDate.Value).ToList();
            }

            // Group by user and create viewer analytics
            var viewerGroups = views
                .GroupBy(v => v.UserId)
                .Select(g => new PropertyViewerDTO
                {
                    UserId = g.Key,
                    UserName = g.First().User?.UserName ?? "Unknown",
                    Email = g.First().User?.Email ?? "Unknown",
                    DisplayName = g.First().User?.DisplayName ?? "Unknown",
                    PhoneNumber = g.First().User?.PhoneNumber ?? "Unknown",
                    ViewedAt = g.OrderByDescending(v => v.ViewedAt).First().ViewedAt,
                    ViewCount = g.Count()
                })
                .OrderByDescending(v => v.ViewedAt)
                .ToList();

            // Apply limit if provided
            if (request?.Limit.HasValue == true && request.Limit > 0)
            {
                viewerGroups = viewerGroups.Take(request.Limit.Value).ToList();
            }

            return new PropertyViewersAnalyticsDTO
            {
                PropertyId = propertyId,
                PropertyTitle = property.Title,
                TotalViews = views.Count(),
                UniqueViewers = viewerGroups.Count,
                Viewers = viewerGroups,
                LastViewedAt = views.Any() ? views.Max(v => v.ViewedAt) : null,
                FirstViewedAt = views.Any() ? views.Min(v => v.ViewedAt) : null
            };
        }

        public async Task<bool> CanUserViewPropertyAnalyticsAsync(string userId, int propertyId)
        {
            // Check if user is the property owner
            var propertyRepository = _unitOfWork.GetRepository<Property, int>();
            var property = await propertyRepository.GetAsync(propertyId);
            
            if (property == null)
                return false;

            // Get user type (Broker or Developer)
            var brokerRepository = _unitOfWork.GetRepository<Broker, int>();
            var developerRepository = _unitOfWork.GetRepository<Developer, int>();
            
            var broker = await brokerRepository.GetAsync(new BrokerWithUserSpecifications(userId));
            var developer = await developerRepository.GetAsync(new DeveloperWithUserSpecifications(userId));

            // Check if user owns the property
            bool isOwner = false;
            if (broker != null && property.BrokerId == broker.Id)
                isOwner = true;
            else if (developer != null && property.DeveloperId == developer.Id)
                isOwner = true;

            if (!isOwner)
                return false;

            // Check if user has Pro or Premium subscription
            var subscriptionRepository = _unitOfWork.GetRepository<Subscription, int>();
            var subscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(
                brokerId: broker?.Id,
                developerId: developer?.Id,
                isActive: true));

            if (subscription?.Package == null)
                return false;

            // Only Pro and Premium packages can view property analytics
            return subscription.Package.Name == "Pro" || subscription.Package.Name == "Premium";
        }

        public async Task<List<PropertyViewersAnalyticsDTO>> GetMyPropertiesViewAnalyticsAsync(string ownerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var brokerRepository = _unitOfWork.GetRepository<Broker, int>();
            var developerRepository = _unitOfWork.GetRepository<Developer, int>();
            var propertyRepository = _unitOfWork.GetRepository<Property, int>();
            var subscriptionRepository = _unitOfWork.GetRepository<Subscription, int>();

            var broker = await brokerRepository.GetAsync(new BrokerWithUserSpecifications(ownerId));
            var developer = await developerRepository.GetAsync(new DeveloperWithUserSpecifications(ownerId));

            if (broker == null && developer == null)
                return new List<PropertyViewersAnalyticsDTO>();

            // Check subscription level - user must have Pro or Premium
            var subscription = await subscriptionRepository.GetAsync(new SubscriptionSpecifications(
                brokerId: broker?.Id,
                developerId: developer?.Id,
                isActive: true));

            if (subscription?.Package == null || (subscription.Package.Name != "Pro" && subscription.Package.Name != "Premium"))
                return new List<PropertyViewersAnalyticsDTO>();

            // Get user's properties
            var properties = new List<Property>();
            if (broker != null)
            {
                properties = (await propertyRepository.GetAllAsync(new PropertyWithBrokerSpecifications(broker.Id))).ToList();
            }
            else if (developer != null)
            {
                var devParams = new PropertySpecificationsParameters { DeveloperId = developer.Id };
                properties = (await propertyRepository.GetAllAsync(new PropertyWithDeveloperSpecifications(devParams))).ToList();
            }

            var analyticsResults = new List<PropertyViewersAnalyticsDTO>();

            foreach (var property in properties)
            {
                var analytics = await GetPropertyViewersAsync(property.Id, ownerId, new PropertyViewAnalyticsRequestDTO
                {
                    PropertyId = property.Id,
                    FromDate = fromDate,
                    ToDate = toDate
                });

                if (analytics != null)
                {
                    analyticsResults.Add(analytics);
                }
            }

            return analyticsResults.OrderByDescending(a => a.TotalViews).ToList();
        }
    }
} 