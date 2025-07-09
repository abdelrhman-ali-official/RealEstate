using System;
using Domain.Entities;

namespace Shared.AppointmentModels
{
    public class AppointmentSpecificationsParameters
    {
        public int? CustomerId { get; set; }
        public int? PropertyId { get; set; }
        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        private int _pageSize = 10;
        private const int MaxPageSize = 100;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
} 