using System;
using Domain.Entities;

namespace Shared.AppointmentModels
{
    public class AppointmentCreateDTO
    {
        public int PropertyId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }

    public class AppointmentUpdateDTO
    {
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
    }

    public class AppointmentResultDTO
    {
        public int AppointmentId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string OwnerType { get; set; } // "Developer" or "Broker"
        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerContact { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 