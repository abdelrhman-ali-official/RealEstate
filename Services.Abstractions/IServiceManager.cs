using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IServiceManager
    {
        public IProductService ProductService { get; }
        public IAuthenticationService AuthenticationService { get; }
        public IPropertyService PropertyService { get; }
        public IDeveloperService DeveloperService { get; }
        public IBrokerService BrokerService { get; }
        public IAppointmentService AppointmentService { get; }
        public IDashboardService DashboardService { get; }
        public IWishListService WishListService { get; }
        public IPropertyViewHistoryService PropertyViewHistoryService { get; }
    }
}