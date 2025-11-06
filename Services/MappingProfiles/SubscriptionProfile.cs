using AutoMapper;
using Domain.Entities.SubscriptionEntities;
using Shared.SubscriptionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MappingProfiles
{
    public class SubscriptionProfile : Profile
    {
        public SubscriptionProfile()
        {
            // Package mappings
            CreateMap<Package, PackageDto>()
                .ReverseMap();

            // Subscription mappings
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.Package, opt => opt.MapFrom(src => src.Package))
                .ReverseMap()
                .ForMember(dest => dest.Package, opt => opt.Ignore());

            // For creating subscriptions
            CreateMap<CreateSubscriptionRequest, Subscription>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BrokerId, opt => opt.Ignore())
                .ForMember(dest => dest.DeveloperId, opt => opt.Ignore())
                .ForMember(dest => dest.Broker, opt => opt.Ignore())
                .ForMember(dest => dest.Developer, opt => opt.Ignore())
                .ForMember(dest => dest.Package, opt => opt.Ignore())
                .ForMember(dest => dest.SubscribedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPropertyCount, opt => opt.Ignore());
        }
    }
}