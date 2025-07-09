using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Services.Abstractions;
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
    }
} 