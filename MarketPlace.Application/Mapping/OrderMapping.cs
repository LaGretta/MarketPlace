using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Mapping;

public class OrderMapping : Profile
{
    public OrderMapping()
    {
        CreateMap<CreateOrderDto,Order>();
        CreateMap<OrderItemDto,OrderItem>();
        CreateMap<OrderItem, OrderItemResponseDto>();
        CreateMap<Order, OrderResponseDto>();
    }
}