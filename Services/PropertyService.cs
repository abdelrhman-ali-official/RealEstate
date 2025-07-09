using Domain.Contracts;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.BrokerEntities;
using Domain.Exceptions;
using Services.Abstractions;
using Services.Specifications;
using Shared.DeveloperModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public PropertyService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PropertyResultDTO>> GetAllPropertiesAsync(PropertySpecificationsParameters parameters)
        {
            var properties = await _unitOfWork.GetRepository<Property, int>()
                .GetAllAsync(new PropertyWithDeveloperSpecifications(parameters));

            var propertiesResult = _mapper.Map<IEnumerable<PropertyResultDTO>>(properties);

            var count = propertiesResult.Count();

            var totalCount = await _unitOfWork.GetRepository<Property, int>()
                .CountAsync(new PropertyCountSpecifications(parameters));

            var result = new PaginatedResult<PropertyResultDTO>(
                parameters.PageIndex,
                count,
                totalCount,
                propertiesResult);

            return result;
        }

        public async Task<PropertyResultDTO?> GetPropertyByIdAsync(int id)
        {
            var property = await _unitOfWork.GetRepository<Property, int>().GetAsync(
                new PropertyWithDeveloperSpecifications(id));

            return property is null ? null : _mapper.Map<PropertyResultDTO>(property);
        }

        public async Task<IEnumerable<PropertyResultDTO>> GetPropertiesByDeveloperAsync(int developerId, string userId)
        {
            // First verify that the developer belongs to the user
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                new DeveloperWithUserSpecifications(developerId));

            if (developer is null)
                throw new DeveloperNotFoundException(developerId.ToString());

            if (developer.UserId != userId)
                throw new UnauthorizedAccessException("You can only view your own properties.");

            var parameters = new PropertySpecificationsParameters
            {
                DeveloperId = developerId
            };

            var properties = await _unitOfWork.GetRepository<Property, int>()
                .GetAllAsync(new PropertyWithDeveloperSpecifications(parameters));

            return _mapper.Map<IEnumerable<PropertyResultDTO>>(properties);
        }

        public async Task<IEnumerable<PropertyResultDTO>> GetPropertiesByBrokerAsync(int brokerId, string userId)
        {
            // First verify that the broker belongs to the user
            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                new BrokerWithUserSpecifications(brokerId));

            if (broker is null)
                throw new BrokerNotFoundException(brokerId.ToString());

            if (broker.UserId != userId)
                throw new UnauthorizedAccessException("You can only view your own properties.");

            var parameters = new PropertySpecificationsParameters
            {
                BrokerId = brokerId
            };

            var properties = await _unitOfWork.GetRepository<Property, int>()
                .GetAllAsync(new PropertyWithDeveloperSpecifications(parameters));

            return _mapper.Map<IEnumerable<PropertyResultDTO>>(properties);
        }

        public async Task<PropertyResultDTO> CreatePropertyAsync(PropertyCreateDTO propertyDto, string userId)
        {
            // Check if user is a developer or broker
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                new DeveloperWithUserSpecifications(userId));

            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                new BrokerWithUserSpecifications(userId));

            if (developer == null && broker == null)
                throw new UnauthorizedAccessException("You must be a developer or broker to create properties.");

            var property = _mapper.Map<Property>(propertyDto);
            
            if (developer != null)
            {
                property.DeveloperId = developer.Id;
            }
            else if (broker != null)
            {
                property.BrokerId = broker.Id;
            }

            property.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Property, int>().AddAsync(property);
            await _unitOfWork.SaveChangesAsync();

            // Get the created property with owner info
            var createdProperty = await GetPropertyByIdAsync(property.Id);
            return createdProperty!;
        }

        public async Task<PropertyResultDTO> UpdatePropertyAsync(int id, PropertyUpdateDTO propertyDto, string userId)
        {
            var property = await _unitOfWork.GetRepository<Property, int>().GetAsync(
                new PropertyWithDeveloperSpecifications(id));

            if (property is null)
                throw new PropertyNotFoundException(id.ToString());

            // Check if the property belongs to the user
            bool isOwner = false;
            if (property.DeveloperId.HasValue)
            {
                var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                    new DeveloperWithUserSpecifications(property.DeveloperId.Value));
                isOwner = developer?.UserId == userId;
            }
            else if (property.BrokerId.HasValue)
            {
                var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                    new BrokerWithUserSpecifications(property.BrokerId.Value));
                isOwner = broker?.UserId == userId;
            }

            if (!isOwner)
                throw new UnauthorizedAccessException("You can only update your own properties.");

            _mapper.Map(propertyDto, property);
            _unitOfWork.GetRepository<Property, int>().Update(property);
            await _unitOfWork.SaveChangesAsync();

            var updatedProperty = await GetPropertyByIdAsync(id);
            return updatedProperty!;
        }

        public async Task<bool> DeletePropertyAsync(int id, string userId)
        {
            var property = await _unitOfWork.GetRepository<Property, int>().GetAsync(
                new PropertyWithDeveloperSpecifications(id));

            if (property is null)
                return false;

            // Check if the property belongs to the user
            bool isOwner = false;
            if (property.DeveloperId.HasValue)
            {
                var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                    new DeveloperWithUserSpecifications(property.DeveloperId.Value));
                isOwner = developer?.UserId == userId;
            }
            else if (property.BrokerId.HasValue)
            {
                var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                    new BrokerWithUserSpecifications(property.BrokerId.Value));
                isOwner = broker?.UserId == userId;
            }

            if (!isOwner)
                throw new UnauthorizedAccessException("You can only delete your own properties.");

            _unitOfWork.GetRepository<Property, int>().Delete(property);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<string>> GetGovernmentsAsync()
        {
            var properties = await _unitOfWork.GetRepository<Property, int>().GetAllAsync();
            return properties.Select(p => p.Government).Distinct().OrderBy(g => g).ToList();
        }

        public async Task<IEnumerable<string>> GetCitiesByGovernmentAsync(string government)
        {
            var properties = await _unitOfWork.GetRepository<Property, int>().GetAllAsync();
            return properties
                .Where(p => p.Government.ToLower() == government.ToLower())
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
} 