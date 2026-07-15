namespace MarketPlace.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public int SellerId { get; set; }
    public User Seller { get; set; } = null!;  
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}