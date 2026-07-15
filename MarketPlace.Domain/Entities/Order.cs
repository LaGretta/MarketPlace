using MarketPlace.Domain.Enums;

namespace MarketPlace.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}