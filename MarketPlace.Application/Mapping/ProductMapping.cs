using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Mapping;

public class ProductMapping : Profile
{
    public ProductMapping()
    {
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<Product, ProductResponseDto>();
    }
}