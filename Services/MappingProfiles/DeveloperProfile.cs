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
    public class DeveloperProfile : Profile
    {
        public DeveloperProfile()
        {
            CreateMap<Developer, DeveloperResultDTO>()
                .ForMember(d => d.UserName, options => options.MapFrom(s => s.User.UserName))
                .ForMember(d => d.UserEmail, options => options.MapFrom(s => s.User.Email));

            CreateMap<DeveloperCreateDTO, Developer>();
            CreateMap<DeveloperUpdateDTO, Developer>();
        }
    }
} 