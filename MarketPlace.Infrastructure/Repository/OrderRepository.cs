using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Domain.Entities;
using MarketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;
    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddOrder(Order order, CancellationToken ct)
    {
         await _dbContext.Orders.AddAsync(order, ct);
    }
    public async Task<List<Order>> GetMyOrders(int userId, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Where(o => o.BuyerId == userId)  
            .Include(o => o.OrderItems)              
            .ToListAsync(ct);   
    }
    public async Task<Order?> GetOrderById(int id, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }
    public async Task<Order?> GetMyOrderById(int userId, int id, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.BuyerId == userId && o.Id == id, ct);
    }
    public async Task<(List<Order> Items, int TotalCount)> GetAllOrders(int page, int pageSize, CancellationToken ct)
    {
        var get =  _dbContext.Orders.AsNoTracking();

        var countAsync = await get.CountAsync(ct);

        var paged = await get
            .Include(o => o.OrderItems) 
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (paged, countAsync);
    }
}