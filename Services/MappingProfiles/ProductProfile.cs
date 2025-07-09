using Domain.Entities.ProductEntities;
using Shared.ProductModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Services.MappingProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductBrand, BrandResultDTO>();
            CreateMap<ProductType,TypeResultDTO>();


            CreateMap<Product, ProductResultDTO>()
                .ForMember(d => d.BrandName, options => options.MapFrom(s => s.ProductBrand.Name))
                .ForMember(d => d.TypeName, options => options.MapFrom(s => s.ProductType.Name));
        }
    }
}
