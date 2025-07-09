using Shared.AppointmentModels;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IAppointmentService
    {
        Task<AppointmentResultDTO> BookAppointmentAsync(AppointmentCreateDTO dto, string customerId);
        Task<IEnumerable<AppointmentResultDTO>> GetAppointmentsAsync(AppointmentSpecificationsParameters parameters);
        Task<AppointmentResultDTO?> GetAppointmentByIdAsync(int appointmentId, string userId, string userRole);
        Task<bool> CancelAppointmentAsync(int appointmentId, string customerId);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status, string ownerId, string userRole);
        Task<bool> UpdateAppointmentAsync(int appointmentId, AppointmentUpdateDTO dto, string userId, string userRole);
    }
} 