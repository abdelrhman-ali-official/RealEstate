using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shared.SubscriptionModels
{
    public class SubscriptionSpecificationsParameters
    {
        public int? PackageId { get; set; }
        public int? BrokerId { get; set; }
        public int? DeveloperId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? SubscribedAfter { get; set; }
        public DateTime? SubscribedBefore { get; set; }
    }

    public class PackageSpecificationsParameters
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public int? MinPropertyLimit { get; set; }
        public int? MaxPropertyLimit { get; set; }
        public bool? DirectContactSystem { get; set; }
    }
}