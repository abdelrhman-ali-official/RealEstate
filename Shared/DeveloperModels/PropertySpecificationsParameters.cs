using Domain.Entities.DeveloperEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DeveloperModels
{
    public class PropertySpecificationsParameters
    {
        private const int MAXPAGESIZE = 188;
        private const int DefaultPageSize = 10;

        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public string? Government { get; set; }
        public string? City { get; set; }
        public PropertyType? Type { get; set; }
        public PropertyStatus? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public PropertySortingOptions? Sort { get; set; }
        public int PageIndex { get; set; } = 1;
        public string? Search { get; set; }

        private int _pageSize = DefaultPageSize;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MAXPAGESIZE ? MAXPAGESIZE : value;
        }
    }

    public enum PropertySortingOptions
    {
        TitleAsc,
        TitleDesc,
        PriceAsc,
        PriceDesc,
        AreaAsc,
        AreaDesc,
        CreatedAtAsc,
        CreatedAtDesc
    }
} 