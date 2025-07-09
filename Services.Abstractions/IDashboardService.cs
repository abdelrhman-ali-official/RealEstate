using Shared.DashboardModels;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IDashboardService
    {
        Task<AdminDashboardDTO> GetAdminDashboardAsync(DashboardFilterDTO filter);
    }
} 