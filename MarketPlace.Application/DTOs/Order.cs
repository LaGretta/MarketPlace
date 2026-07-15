using MarketPlace.Domain.Enums;

namespace MarketPlace.Application.DTOs;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderResponseDto
{
    public int Id { get; set; }
    public int BuyerId { get; set; }            
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();  
}
public class OrderItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } 
}