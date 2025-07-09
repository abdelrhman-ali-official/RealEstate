using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.BrokerModels
{
    public class BrokerSpecificationsParameters
    {
        private const int MAXPAGESIZE = 188;
        private const int DefaultPageSize = 10;

        public string? City { get; set; }
        public string? Government { get; set; }
        public string? AgencyName { get; set; }
        public string? FullName { get; set; }
        public BrokerSortingOptions? Sort { get; set; }
        public int PageIndex { get; set; } = 1;
        public string? Search { get; set; }

        private int _pageSize = DefaultPageSize;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MAXPAGESIZE ? MAXPAGESIZE : value;
        }
    }

    public enum BrokerSortingOptions
    {
        FullNameAsc,
        FullNameDesc,
        AgencyNameAsc,
        AgencyNameDesc,
        CityAsc,
        CityDesc,
        GovernmentAsc,
        GovernmentDesc,
        CreatedAtAsc,
        CreatedAtDesc
    }
} 