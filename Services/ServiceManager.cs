using Domain.Contracts;
//using Domain.Contracts.NewModule;
using Domain.Entities.SecurityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Services.Abstractions;
//using Services.Services;
using Shared.SecurityModels;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using AutoMapper;
using Domain.Contracts;
using Persistence.Data;
using Core.Services;

namespace Services
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<IPropertyService> _propertyService;
        private readonly Lazy<IDeveloperService> _developerService;
        private readonly Lazy<IBrokerService> _brokerService;
        private readonly Lazy<IAppointmentService> _appointmentService;
        private readonly Lazy<IDashboardService> _dashboardService;
        private readonly Lazy<IWishListService> _wishListService;
        private readonly Lazy<IPropertyViewHistoryService> _propertyViewHistoryService;

        private readonly AutoMapper.IMapper _mapper;
    

        //private readonly Lazy<IClinicSearchService> _clinicSearchService;

        public ServiceManager(
            IUnitOFWork unitOfWork,
            AutoMapper.IMapper mapper,
            UserManager<User> userManager,
            IOptions<JwtOptions> jwtOptions,
            IOptions<DomainSettings> domainSettings,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _productService = new Lazy<IProductService>(() => new ProductService(unitOfWork, mapper));
            _propertyService = new Lazy<IPropertyService>(() => new PropertyService(unitOfWork, mapper));
            _developerService = new Lazy<IDeveloperService>(() => new DeveloperService(unitOfWork, mapper));
            _brokerService = new Lazy<IBrokerService>(() => new BrokerService(unitOfWork, mapper));
            _appointmentService = new Lazy<IAppointmentService>(() => new AppointmentService(unitOfWork, mapper));
            _dashboardService = new Lazy<IDashboardService>(() => new DashboardService(unitOfWork, mapper, userManager));
            _wishListService = new Lazy<IWishListService>(() => new WishListService(unitOfWork, mapper));
            _propertyViewHistoryService = new Lazy<IPropertyViewHistoryService>(() => new PropertyViewHistoryService(unitOfWork, mapper));

            // Initialize authenticationService
            _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(
                userManager,
                jwtOptions,
                domainSettings,
                mapper,
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>()
            ));
/*
            _paymentService = new Lazy<IPaymentService>(() => new PaymentService(basketRepository, unitOfWork, mapper, configuration));
            _petService = new Lazy<IPetService>(() => new PetService(
                serviceProvider.GetRequiredService<IPetRepository>(),
                mapper,
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PetService>()));

            _clinicService = new Lazy<IClinicService>(() => new ClinicService(unitOfWork, mapper, userManager));
            _doctorScheduleService = new Lazy<IDoctorScheduleService>(() => new DoctorScheduleService(unitOfWork, mapper));

            // Use the AppointmentService from Core/Services
            _appointmentService = new Lazy<IAppointmentService>(() => new AppointmentService(unitOfWork, mapper));

            // Use the MedicalRecordService from Core/Services
            _medicalRecordService = new Lazy<IMedicalRecordService>(() => new MedicalRecordService(
                unitOfWork,
                mapper,
                _appointmentService.Value));
*/


         
        }

        public IProductService ProductService => _productService.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public IPropertyService PropertyService => _propertyService.Value;
        public IDeveloperService DeveloperService => _developerService.Value;
        public IBrokerService BrokerService => _brokerService.Value;
        public IAppointmentService AppointmentService => _appointmentService.Value;
        public IDashboardService DashboardService => _dashboardService.Value;
        public IWishListService WishListService => _wishListService.Value;
        public IPropertyViewHistoryService PropertyViewHistoryService => _propertyViewHistoryService.Value;
    }
}