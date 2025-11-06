using Domain.Entities.DeveloperEntities;
using Shared.DeveloperModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MappingProfiles
{
    public class PropertyProfile : Profile
    {
        public PropertyProfile()
        {
            CreateMap<Property, PropertyResultDTO>()
                .ForMember(d => d.DeveloperName, options => options.MapFrom(s => s.Developer != null ? s.Developer.User.DisplayName : null))
                .ForMember(d => d.DeveloperCompanyName, options => options.MapFrom(s => s.Developer != null ? s.Developer.CompanyName : null))
                .ForMember(d => d.BrokerName, options => options.MapFrom(s => s.Broker != null ? s.Broker.FullName : null))
                .ForMember(d => d.BrokerAgencyName, options => options.MapFrom(s => s.Broker != null ? s.Broker.AgencyName : null))
                .ForMember(d => d.OwnerType, options => options.MapFrom(s => s.DeveloperId.HasValue ? "Developer" : "Broker"))
                .ForMember(d => d.TotalViews, options => options.Ignore())
                .ForMember(d => d.UniqueViewers, options => options.Ignore())
                .ForMember(d => d.LastViewedAt, options => options.Ignore());

            CreateMap<PropertyCreateDTO, Property>();
            CreateMap<PropertyUpdateDTO, Property>();
        }
    }
} 