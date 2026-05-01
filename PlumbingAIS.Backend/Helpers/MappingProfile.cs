using AutoMapper;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductReadDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Не вказано"))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : "Не вказано"))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : "Не вказано"));

            CreateMap<ProductCreateDto, Product>();

            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : "Гість"))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : "Гість"));

            CreateMap<UserRegisterDto, User>();
        }
    }
}