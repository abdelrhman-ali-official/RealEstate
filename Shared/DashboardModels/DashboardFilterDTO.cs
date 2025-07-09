using System;

namespace Shared.DashboardModels
{
    public class DashboardFilterDTO
    {
        public string? City { get; set; }
        public string? Government { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PropertyType { get; set; }
    }
} 