using Domain.Contracts;
using Domain.Entities.BrokerEntities;
using Domain.Exceptions;
using Services.Abstractions;
using Services.Specifications;
using Shared.BrokerModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Services
{
    public class BrokerService : IBrokerService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public BrokerService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<BrokerResultDTO>> GetAllBrokersAsync(BrokerSpecificationsParameters parameters)
        {
            var brokers = await _unitOfWork.GetRepository<Broker, int>()
                .GetAllAsync(new BrokerWithUserSpecifications(parameters));

            var brokersResult = _mapper.Map<IEnumerable<BrokerResultDTO>>(brokers);

            var count = brokersResult.Count();

            var totalCount = await _unitOfWork.GetRepository<Broker, int>()
                .CountAsync(new BrokerCountSpecifications(parameters));

            var result = new PaginatedResult<BrokerResultDTO>(
                parameters.PageIndex,
                count,
                totalCount,
                brokersResult);

            return result;
        }

        public async Task<BrokerResultDTO?> GetBrokerByIdAsync(int id)
        {
            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                new BrokerWithUserSpecifications(id));

            return broker is null ? null : _mapper.Map<BrokerResultDTO>(broker);
        }

        public async Task<BrokerResultDTO?> GetBrokerByUserIdAsync(string userId)
        {
            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                new BrokerWithUserSpecifications(userId));

            return broker is null ? null : _mapper.Map<BrokerResultDTO>(broker);
        }

        public async Task<BrokerResultDTO> CreateBrokerAsync(BrokerCreateDTO brokerDto, string userId)
        {
            // Check if broker already exists for this user
            var existingBroker = await GetBrokerByUserIdAsync(userId);
            if (existingBroker != null)
            {
                throw new ValidationException(new List<string> { "Broker profile already exists for this user." });
            }

            var broker = _mapper.Map<Broker>(brokerDto);
            broker.UserId = userId;
            broker.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Broker, int>().AddAsync(broker);
            await _unitOfWork.SaveChangesAsync();

            // Get the created broker with user info
            var createdBroker = await GetBrokerByIdAsync(broker.Id);
            return createdBroker!;
        }

        public async Task<BrokerResultDTO> UpdateBrokerAsync(int id, BrokerUpdateDTO brokerDto, string userId)
        {
            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(
                new BrokerWithUserSpecifications(id));

            if (broker is null)
                throw new BrokerNotFoundException(id.ToString());

            // Check if the broker belongs to the user
            if (broker.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own broker profile.");

            _mapper.Map(brokerDto, broker);
            _unitOfWork.GetRepository<Broker, int>().Update(broker);
            await _unitOfWork.SaveChangesAsync();

            var updatedBroker = await GetBrokerByIdAsync(id);
            return updatedBroker!;
        }

        public async Task<bool> DeleteBrokerAsync(int id, string userId)
        {
            var broker = await _unitOfWork.GetRepository<Broker, int>().GetAsync(id);

            if (broker is null)
                return false;

            // Check if the broker belongs to the user
            if (broker.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own broker profile.");

            _unitOfWork.GetRepository<Broker, int>().Delete(broker);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<string>> GetGovernmentsAsync()
        {
            var brokers = await _unitOfWork.GetRepository<Broker, int>().GetAllAsync();
            return brokers.Select(b => b.Government).Distinct().OrderBy(g => g).ToList();
        }

        public async Task<IEnumerable<string>> GetCitiesByGovernmentAsync(string government)
        {
            var brokers = await _unitOfWork.GetRepository<Broker, int>().GetAllAsync();
            return brokers.Where(b => b.Government.ToLower() == government.ToLower())
                         .Select(b => b.City)
                         .Distinct()
                         .OrderBy(c => c)
                         .ToList();
        }

        public async Task<IEnumerable<string>> GetAgencyNamesAsync()
        {
            var brokers = await _unitOfWork.GetRepository<Broker, int>().GetAllAsync();
            return brokers.Where(b => !string.IsNullOrEmpty(b.AgencyName))
                         .Select(b => b.AgencyName!)
                         .Distinct()
                         .OrderBy(a => a)
                         .ToList();
        }
    }
} 