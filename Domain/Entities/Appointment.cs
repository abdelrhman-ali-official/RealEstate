using Domain.Entities.BrokerEntities;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.SecurityEntities;
using System;

namespace Domain.Entities
{
    public class Appointment : BaseEntity<int>
    {
        public string CustomerId { get; set; }
        public User Customer { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        public int? DeveloperId { get; set; }
        public Developer? Developer { get; set; }
        public int? BrokerId { get; set; }
        public Broker? Broker { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AppointmentStatus : byte
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Completed = 4
    }
} 