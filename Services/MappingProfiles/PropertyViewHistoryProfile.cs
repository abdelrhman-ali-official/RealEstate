using AutoMapper;
using Domain.Entities;
using Shared.PropertyViewHistoryModels;

namespace Services.MappingProfiles
{
    public class PropertyViewHistoryProfile : Profile
    {
        public PropertyViewHistoryProfile()
        {
            CreateMap<PropertyViewHistory, PropertyViewHistoryDTO>()
                .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Property.Title))
                .ForMember(dest => dest.PropertyDescription, opt => opt.MapFrom(src => src.Property.Description))
                .ForMember(dest => dest.PropertyPrice, opt => opt.MapFrom(src => src.Property.Price))
                .ForMember(dest => dest.PropertyMainImageUrl, opt => opt.MapFrom(src => src.Property.MainImageUrl));
        }
    }
} 