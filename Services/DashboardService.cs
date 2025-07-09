using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.SecurityEntities;
using Services.Abstractions;
using Shared.DashboardModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOFWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public DashboardService(IUnitOFWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<AdminDashboardDTO> GetAdminDashboardAsync(DashboardFilterDTO filter)
        {
            // Date range
            var fromDate = filter.FromDate ?? DateTime.UtcNow.AddMonths(-12);
            var toDate = filter.ToDate ?? DateTime.UtcNow;

            // Users
            var usersQuery = _userManager.Users.Include(u => u.Address).AsQueryable();
            if (!string.IsNullOrEmpty(filter.City))
                usersQuery = usersQuery.Where(u => u.Address.City.ToLower() == filter.City.ToLower());
            // Government filter removed (not present in Address)
            usersQuery = usersQuery.Where(u => u.RegisteredAt >= fromDate && u.RegisteredAt <= toDate);

            var users = await usersQuery.ToListAsync();
            var userBreakdown = new UserRoleBreakdownDTO
            {
                Customers = users.Count(u => u.UserRole == Role.Customer),
                Brokers = users.Count(u => u.UserRole == Role.Broker),
                Developers = users.Count(u => u.UserRole == Role.Developer),
                Admins = users.Count(u => u.UserRole == Role.Admin)
            };

            // Properties
            var propertiesQuery = _unitOfWork.GetRepository<Property, int>().GetAllAsQueryable();
            if (!string.IsNullOrEmpty(filter.City))
                propertiesQuery = propertiesQuery.Where(p => p.City.ToLower() == filter.City.ToLower());
            if (!string.IsNullOrEmpty(filter.Government))
                propertiesQuery = propertiesQuery.Where(p => p.Government.ToLower() == filter.Government.ToLower());
            if (filter.PropertyType.HasValue)
                propertiesQuery = propertiesQuery.Where(p => (int)p.Type == filter.PropertyType.Value);
            propertiesQuery = propertiesQuery.Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate);

            var properties = await propertiesQuery.ToListAsync();

            // Appointments
            var appointmentsQuery = _unitOfWork.GetRepository<Appointment, int>().GetAllAsQueryable();
            if (!string.IsNullOrEmpty(filter.City))
                appointmentsQuery = appointmentsQuery.Where(a => a.Property.City.ToLower() == filter.City.ToLower());
            if (!string.IsNullOrEmpty(filter.Government))
                appointmentsQuery = appointmentsQuery.Where(a => a.Property.Government.ToLower() == filter.Government.ToLower());
            appointmentsQuery = appointmentsQuery.Where(a => a.CreatedAt >= fromDate && a.CreatedAt <= toDate);

            var appointments = await appointmentsQuery.Include(a => a.Property).ToListAsync();

            // Top 5 Cities
            var topCities = properties
                .GroupBy(p => p.City)
                .Select(g => new CityPropertyCountDTO { City = g.Key, PropertyCount = g.Count() })
                .OrderByDescending(x => x.PropertyCount)
                .Take(5)
                .ToList();

            // Appointment status breakdown
            var appointmentStatusBreakdown = new AppointmentStatusBreakdownDTO
            {
                Pending = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                Confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                Completed = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
            };

            // Monthly stats (based on filtered date range)
            var startDate = fromDate.Date;
            var endDate = toDate.Date;
            var months = new List<(int Year, int Month, string Label)>();
            
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            while (currentDate <= endDate)
            {
                months.Add((currentDate.Year, currentDate.Month, currentDate.ToString("yyyy-MM")));
                currentDate = currentDate.AddMonths(1);
            }

            var monthlyStats = months.Select(m => new MonthlyStatsDTO
            {
                Month = m.Label,
                NewUsers = users.Count(u => u.RegisteredAt.Year == m.Year && u.RegisteredAt.Month == m.Month),
                NewProperties = properties.Count(p => p.CreatedAt.Year == m.Year && p.CreatedAt.Month == m.Month),
                NewAppointments = new MonthlyAppointmentStatusDTO
                {
                    Pending = appointments.Count(a => a.CreatedAt.Year == m.Year && a.CreatedAt.Month == m.Month && a.Status == AppointmentStatus.Pending),
                    Confirmed = appointments.Count(a => a.CreatedAt.Year == m.Year && a.CreatedAt.Month == m.Month && a.Status == AppointmentStatus.Confirmed),
                    Completed = appointments.Count(a => a.CreatedAt.Year == m.Year && a.CreatedAt.Month == m.Month && a.Status == AppointmentStatus.Completed),
                    Cancelled = appointments.Count(a => a.CreatedAt.Year == m.Year && a.CreatedAt.Month == m.Month && a.Status == AppointmentStatus.Cancelled)
                }
            }).ToList();

            return new AdminDashboardDTO
            {
                TotalUsers = users.Count,
                UserBreakdown = userBreakdown,
                TotalProperties = properties.Count,
                TotalAppointments = appointments.Count,
                AppointmentStatusBreakdown = appointmentStatusBreakdown,
                TopCities = topCities,
                MonthlyStats = monthlyStats
            };
        }
    }
} 