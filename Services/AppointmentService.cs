using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Services.Abstractions;
using Services.Specifications;
using Shared.AppointmentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppointmentService(IUnitOFWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppointmentResultDTO> BookAppointmentAsync(AppointmentCreateDTO dto, string customerId)
        {
            // Get property and determine owner
            var property = await _unitOfWork.GetRepository<Domain.Entities.DeveloperEntities.Property, int>().GetAsync(dto.PropertyId);
            if (property == null)
                throw new Exception("Property not found");

            var appointment = _mapper.Map<Appointment>(dto);
            appointment.CustomerId = customerId;
            appointment.PropertyId = property.Id;
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.Status = AppointmentStatus.Pending;

            if (property.DeveloperId.HasValue)
            {
                appointment.DeveloperId = property.DeveloperId;
                appointment.BrokerId = null;
            }
            else if (property.BrokerId.HasValue)
            {
                appointment.BrokerId = property.BrokerId;
                appointment.DeveloperId = null;
            }
            else
            {
                throw new Exception("Property owner not found");
            }

            await _unitOfWork.GetRepository<Appointment, int>().AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            // Get with details
            var result = await GetAppointmentByIdAsync(appointment.Id, customerId, "Customer");
            return result!;
        }

        public async Task<IEnumerable<AppointmentResultDTO>> GetAppointmentsAsync(AppointmentSpecificationsParameters parameters)
        {
            var appointments = await _unitOfWork.GetRepository<Appointment, int>().GetAllAsync(new AppointmentWithDetailsSpecifications(parameters));
            return _mapper.Map<IEnumerable<AppointmentResultDTO>>(appointments);
        }

        public async Task<AppointmentResultDTO?> GetAppointmentByIdAsync(int appointmentId, string userId, string userRole)
        {
            var appointment = await _unitOfWork.GetRepository<Appointment, int>().GetAsync(new AppointmentWithDetailsSpecifications(appointmentId));
            if (appointment == null)
                return null;

            // Authorization: Customer can view own, Broker/Developer can view if owner, Admin can view all
            if (userRole == "Customer" && appointment.CustomerId != userId)
                throw new UnauthorizedAccessException();
            if (userRole == "Developer" && appointment.Developer?.UserId != userId)
                throw new UnauthorizedAccessException();
            if (userRole == "Broker" && appointment.Broker?.UserId != userId)
                throw new UnauthorizedAccessException();
            // Admin can view all

            return _mapper.Map<AppointmentResultDTO>(appointment);
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string customerId)
        {
            var appointment = await _unitOfWork.GetRepository<Appointment, int>().GetAsync(new AppointmentWithDetailsSpecifications(appointmentId));
            if (appointment == null)
                return false;
            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException();
            if (appointment.Status != AppointmentStatus.Pending)
                throw new InvalidOperationException("Only pending appointments can be cancelled");
            appointment.Status = AppointmentStatus.Cancelled;
            _unitOfWork.GetRepository<Appointment, int>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status, string ownerId, string userRole)
        {
            var appointment = await _unitOfWork.GetRepository<Appointment, int>().GetAsync(new AppointmentWithDetailsSpecifications(appointmentId));
            if (appointment == null)
                return false;
            if (userRole == "Developer" && appointment.Developer?.UserId != ownerId)
                throw new UnauthorizedAccessException();
            if (userRole == "Broker" && appointment.Broker?.UserId != ownerId)
                throw new UnauthorizedAccessException();
            if (userRole == "Admin")
            {
                // Admin can update any
            }
            else if (userRole != "Developer" && userRole != "Broker")
            {
                throw new UnauthorizedAccessException();
            }
            appointment.Status = status;
            _unitOfWork.GetRepository<Appointment, int>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAppointmentAsync(int appointmentId, AppointmentUpdateDTO dto, string userId, string userRole)
        {
            var appointment = await _unitOfWork.GetRepository<Appointment, int>().GetAsync(new AppointmentWithDetailsSpecifications(appointmentId));
            if (appointment == null)
                return false;
            // Only customer can update their own appointment if pending
            if (userRole == "Customer")
            {
                if (appointment.CustomerId != userId)
                    throw new UnauthorizedAccessException();
                if (appointment.Status != AppointmentStatus.Pending)
                    throw new InvalidOperationException("Only pending appointments can be updated");
                appointment.AppointmentDate = dto.AppointmentDate;
                appointment.Notes = dto.Notes;
            }
            else if (userRole == "Developer" && appointment.Developer?.UserId == userId)
            {
                appointment.Status = dto.Status;
            }
            else if (userRole == "Broker" && appointment.Broker?.UserId == userId)
            {
                appointment.Status = dto.Status;
            }
            else if (userRole == "Admin")
            {
                appointment.Status = dto.Status;
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
            _unitOfWork.GetRepository<Appointment, int>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AppointmentResultDTO>> GetBrokerAppointmentsAsync(string userId, AppointmentSpecificationsParameters parameters)
        {
            // Get broker by userId
            var broker = await _unitOfWork.GetRepository<Domain.Entities.BrokerEntities.Broker, int>()
                .GetAsync(new BrokerByUserIdSpecification(userId));
            
            if (broker == null)
                throw new UnauthorizedAccessException("Broker not found");

            // Set broker ID in parameters
            parameters.BrokerId = broker.Id;
            
            var appointments = await _unitOfWork.GetRepository<Appointment, int>()
                .GetAllAsync(new AppointmentWithDetailsSpecifications(parameters));
            
            return _mapper.Map<IEnumerable<AppointmentResultDTO>>(appointments);
        }

        public async Task<IEnumerable<AppointmentResultDTO>> GetDeveloperAppointmentsAsync(string userId, AppointmentSpecificationsParameters parameters)
        {
            // Get developer by userId
            var developer = await _unitOfWork.GetRepository<Domain.Entities.DeveloperEntities.Developer, int>()
                .GetAsync(new DeveloperByUserIdSpecification(userId));
            
            if (developer == null)
                throw new UnauthorizedAccessException("Developer not found");

            // Set developer ID in parameters
            parameters.DeveloperId = developer.Id;
            
            var appointments = await _unitOfWork.GetRepository<Appointment, int>()
                .GetAllAsync(new AppointmentWithDetailsSpecifications(parameters));
            
            return _mapper.Map<IEnumerable<AppointmentResultDTO>>(appointments);
        }

       
    }
} 