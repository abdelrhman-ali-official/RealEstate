using AutoMapper;
using Domain.Entities;
using Shared.AppointmentModels;

namespace Services.MappingProfiles
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<Appointment, AppointmentResultDTO>()
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Property.Title))
                .ForMember(dest => dest.OwnerType, opt => opt.MapFrom(src => src.DeveloperId.HasValue ? "Developer" : "Broker"))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.DeveloperId.HasValue ? src.Developer.User.DisplayName : src.Broker.FullName))
                .ForMember(dest => dest.OwnerContact, opt => opt.MapFrom(src => src.DeveloperId.HasValue ? src.Developer.Phone : src.Broker.Phone));

            CreateMap<AppointmentCreateDTO, Appointment>();
            CreateMap<AppointmentUpdateDTO, Appointment>();
        }
    }
} 