using Shared;
using Shared.BrokerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IBrokerService
    {
        Task<PaginatedResult<BrokerResultDTO>> GetAllBrokersAsync(BrokerSpecificationsParameters parameters);
        Task<BrokerResultDTO?> GetBrokerByIdAsync(int id);
        Task<BrokerResultDTO?> GetBrokerByUserIdAsync(string userId);
        Task<BrokerResultDTO> CreateBrokerAsync(BrokerCreateDTO brokerDto, string userId);
        Task<BrokerResultDTO> UpdateBrokerAsync(int id, BrokerUpdateDTO brokerDto, string userId);
        Task<bool> DeleteBrokerAsync(int id, string userId);
        Task<IEnumerable<string>> GetGovernmentsAsync();
        Task<IEnumerable<string>> GetCitiesByGovernmentAsync(string government);
        Task<IEnumerable<string>> GetAgencyNamesAsync();
    }
} 