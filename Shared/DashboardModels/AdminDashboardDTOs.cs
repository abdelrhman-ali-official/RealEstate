using System;
using System.Collections.Generic;

namespace Shared.DashboardModels
{
    public class AdminDashboardDTO
    {
        public int TotalUsers { get; set; }
        public UserRoleBreakdownDTO UserBreakdown { get; set; }
        public int TotalProperties { get; set; }
        public int TotalAppointments { get; set; }
        public AppointmentStatusBreakdownDTO AppointmentStatusBreakdown { get; set; }
        public List<CityPropertyCountDTO> TopCities { get; set; }
        public List<MonthlyStatsDTO> MonthlyStats { get; set; }
    }

    public class UserRoleBreakdownDTO
    {
        public int Customers { get; set; }
        public int Brokers { get; set; }
        public int Developers { get; set; }
        public int Admins { get; set; }
    }

    public class AppointmentStatusBreakdownDTO
    {
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class CityPropertyCountDTO
    {
        public string City { get; set; }
        public int PropertyCount { get; set; }
    }

    public class MonthlyStatsDTO
    {
        public string Month { get; set; } // e.g. "2024-07"
        public int NewUsers { get; set; }
        public int NewProperties { get; set; }
        public MonthlyAppointmentStatusDTO NewAppointments { get; set; }
    }

    public class MonthlyAppointmentStatusDTO
    {
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
} 