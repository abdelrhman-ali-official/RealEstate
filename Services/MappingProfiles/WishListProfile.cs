using AutoMapper;
using Domain.Entities;
using Domain.Entities.DeveloperEntities;
using Shared.WishListModels;

namespace Services.MappingProfiles
{
    public class WishListProfile : Profile
    {
        public WishListProfile()
        {
            // Map WishListItem to WishListItemDTO
            CreateMap<WishListItem, WishListItemDTO>()
                .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Property.Title))
                .ForMember(dest => dest.PropertyDescription, opt => opt.MapFrom(src => src.Property.Description))
                .ForMember(dest => dest.PropertyPrice, opt => opt.MapFrom(src => src.Property.Price))
                .ForMember(dest => dest.PropertyGovernment, opt => opt.MapFrom(src => src.Property.Government))
                .ForMember(dest => dest.PropertyCity, opt => opt.MapFrom(src => src.Property.City))
                .ForMember(dest => dest.PropertyFullAddress, opt => opt.MapFrom(src => src.Property.FullAddress))
                .ForMember(dest => dest.PropertyArea, opt => opt.MapFrom(src => src.Property.Area))
                .ForMember(dest => dest.PropertyType, opt => opt.MapFrom(src => src.Property.Type.ToString()))
                .ForMember(dest => dest.PropertyStatus, opt => opt.MapFrom(src => src.Property.Status.ToString()))
                .ForMember(dest => dest.PropertyMainImageUrl, opt => opt.MapFrom(src => src.Property.MainImageUrl))
                .ForMember(dest => dest.DeveloperName, opt => opt.MapFrom(src => src.Property.Developer != null ? src.Property.Developer.CompanyName : null))
                .ForMember(dest => dest.BrokerName, opt => opt.MapFrom(src => src.Property.Broker != null ? src.Property.Broker.FullName : null));

            // Map WishListItem to UserWishListInfoDTO (for admin endpoints)
            CreateMap<WishListItem, UserWishListInfoDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.User.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.AddedToWishListAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
} 