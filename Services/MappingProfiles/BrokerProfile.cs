using Domain.Entities.BrokerEntities;
using Shared.BrokerModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MappingProfiles
{
    public class BrokerProfile : Profile
    {
        public BrokerProfile()
        {
            CreateMap<Broker, BrokerResultDTO>()
                .ForMember(d => d.UserName, options => options.MapFrom(s => s.User.DisplayName))
                .ForMember(d => d.UserEmail, options => options.MapFrom(s => s.User.Email));

            CreateMap<BrokerCreateDTO, Broker>();
            CreateMap<BrokerUpdateDTO, Broker>();
        }
    }
} 