using Shared;
using Shared.DeveloperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IDeveloperService
    {
        Task<DeveloperResultDTO?> GetDeveloperByIdAsync(int id);
        Task<DeveloperResultDTO?> GetDeveloperByUserIdAsync(string userId);
        Task<DeveloperResultDTO> CreateDeveloperAsync(DeveloperCreateDTO developerDto, string userId);
        Task<DeveloperResultDTO> UpdateDeveloperAsync(int id, DeveloperUpdateDTO developerDto, string userId);
        Task<bool> DeleteDeveloperAsync(int id, string userId);
        Task<bool> DeveloperExistsAsync(string userId);
    }
} 