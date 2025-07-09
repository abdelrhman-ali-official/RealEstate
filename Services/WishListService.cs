using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.DeveloperEntities;
using Domain.Exceptions;
using Services.Abstractions;
using Services.Specifications;
using Shared.WishListModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class WishListService : IWishListService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishListService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> AddToWishListAsync(int propertyId, string userId)
        {
            // Check if property exists
            var property = await _unitOfWork.GetRepository<Property, int>().GetAsync(new Services.Specifications.PropertyWithDeveloperSpecifications(propertyId));
            if (property == null)
                throw new PropertyNotFoundException(propertyId.ToString());

            // Check if already in wishlist
            var existingItem = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAsync(new WishListWithDetailsSpecifications(propertyId, userId));

            if (existingItem != null)
                return false; // Already in wishlist

            // Add to wishlist
            var wishListItem = new WishListItem
            {
                UserId = userId,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<WishListItem, int>().AddAsync(wishListItem);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<WishListItemDTO>> GetUserWishListAsync(string userId)
        {
            var wishListItems = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAllAsync(new WishListWithDetailsSpecifications(userId));

            return _mapper.Map<IEnumerable<WishListItemDTO>>(wishListItems);
        }

        public async Task<bool> RemoveFromWishListAsync(int propertyId, string userId)
        {
            var wishListItem = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAsync(new WishListWithDetailsSpecifications(propertyId, userId));

            if (wishListItem == null)
                return false;

            _unitOfWork.GetRepository<WishListItem, int>().Delete(wishListItem);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<WishListCountDTO> GetWishListCountAsync(string userId)
        {
            var count = await _unitOfWork.GetRepository<WishListItem, int>()
                .CountAsync(new WishListCountSpecifications(userId));

            return new WishListCountDTO
            {
                TotalItems = count,
                UserId = userId
            };
        }

        public async Task<IEnumerable<MostWishedPropertyDTO>> GetMostWishedPropertiesAsync(WishListFilterDTO filter)
        {
            var wishListItems = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAllAsync(new MostWishedPropertiesSpecifications(
                    filter.DeveloperId,
                    filter.BrokerId,
                    filter.PropertyType,
                    filter.Government,
                    filter.City,
                    filter.FromDate,
                    filter.ToDate));

            // Group by property and count
            var groupedProperties = wishListItems
                .GroupBy(w => w.PropertyId)
                .Select(g => new MostWishedPropertyDTO
                {
                    PropertyId = g.Key,
                    PropertyTitle = g.First().Property.Title,
                    PropertyPrice = g.First().Property.Price,
                    PropertyType = g.First().Property.Type.ToString(),
                    PropertyStatus = g.First().Property.Status.ToString(),
                    WishListCount = g.Count(),
                    DeveloperName = g.First().Property.Developer?.CompanyName,
                    BrokerName = g.First().Property.Broker?.FullName
                })
                .OrderByDescending(p => p.WishListCount)
                .ToList();

            return groupedProperties;
        }

        public async Task<PropertyWishListUsersDTO> GetPropertyWishListUsersAsync(int propertyId)
        {
            // Check if property exists
            var property = await _unitOfWork.GetRepository<Property, int>().GetAsync(new Services.Specifications.PropertyWithDeveloperSpecifications(propertyId));
            if (property == null)
                throw new PropertyNotFoundException(propertyId.ToString());

            var wishListItems = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAllAsync(new PropertyWishListUsersSpecifications(propertyId));

            var users = _mapper.Map<List<UserWishListInfoDTO>>(wishListItems);

            return new PropertyWishListUsersDTO
            {
                PropertyId = propertyId,
                PropertyTitle = property.Title,
                TotalWishListCount = users.Count,
                Users = users
            };
        }

        public async Task<bool> IsPropertyInWishListAsync(int propertyId, string userId)
        {
            var wishListItem = await _unitOfWork.GetRepository<WishListItem, int>()
                .GetAsync(new WishListWithDetailsSpecifications(propertyId, userId));

            return wishListItem != null;
        }
    }
} 