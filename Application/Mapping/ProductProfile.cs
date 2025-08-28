using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductReadDto>()
             .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<ProductCreateDto, Product>()
            .ForMember(d => d.CreatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<ProductUpdateDto, Product>()
            .ForMember(d => d.ModifiedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}