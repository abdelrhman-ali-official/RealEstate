using Domain.Contracts;
using Domain.Entities.DeveloperEntities;
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

namespace Services
{
    public class DeveloperService : IDeveloperService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeveloperService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DeveloperResultDTO?> GetDeveloperByIdAsync(int id)
        {
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                new DeveloperWithUserSpecifications(id));

            return developer is null ? null : _mapper.Map<DeveloperResultDTO>(developer);
        }

        public async Task<DeveloperResultDTO?> GetDeveloperByUserIdAsync(string userId)
        {
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                new DeveloperWithUserSpecifications(userId));

            return developer is null ? null : _mapper.Map<DeveloperResultDTO>(developer);
        }

        public async Task<DeveloperResultDTO> CreateDeveloperAsync(DeveloperCreateDTO developerDto, string userId)
        {
            // Check if developer already exists for this user
            var existingDeveloper = await GetDeveloperByUserIdAsync(userId);
            if (existingDeveloper != null)
            {
                throw new ValidationException(new List<string> { "Developer profile already exists for this user." });
            }

            var developer = _mapper.Map<Developer>(developerDto);
            developer.UserId = userId;
            developer.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<Developer, int>().AddAsync(developer);
            await _unitOfWork.SaveChangesAsync();

            // Get the created developer with user info
            var createdDeveloper = await GetDeveloperByIdAsync(developer.Id);
            return createdDeveloper!;
        }

        public async Task<DeveloperResultDTO> UpdateDeveloperAsync(int id, DeveloperUpdateDTO developerDto, string userId)
        {
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(
                new DeveloperWithUserSpecifications(id));

            if (developer is null)
                throw new DeveloperNotFoundException(id.ToString());

            // Check if the developer belongs to the user
            if (developer.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own developer profile.");

            _mapper.Map(developerDto, developer);
            _unitOfWork.GetRepository<Developer, int>().Update(developer);
            await _unitOfWork.SaveChangesAsync();

            var updatedDeveloper = await GetDeveloperByIdAsync(id);
            return updatedDeveloper!;
        }

        public async Task<bool> DeleteDeveloperAsync(int id, string userId)
        {
            var developer = await _unitOfWork.GetRepository<Developer, int>().GetAsync(id);

            if (developer is null)
                return false;

            // Check if the developer belongs to the user
            if (developer.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own developer profile.");

            _unitOfWork.GetRepository<Developer, int>().Delete(developer);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeveloperExistsAsync(string userId)
        {
            var developer = await GetDeveloperByUserIdAsync(userId);
            return developer != null;
        }
    }
} 