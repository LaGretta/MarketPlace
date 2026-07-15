using MarketPlace.Domain.Entities;
using MarketPlace.Domain.Enums;

namespace MarketPlace.Application.Interfaces.Repository;

public interface IOrderRepository
{
    Task AddOrder(Order order, CancellationToken ct);
    Task<List<Order>> GetMyOrders(int userId, CancellationToken ct);
    Task<Order?> GetOrderById(int id, CancellationToken ct);

    Task<Order?> GetMyOrderById(int userId ,int id, CancellationToken ct);
    Task<(List<Order> Items, int TotalCount)> GetAllOrders(int page, int pageSize, CancellationToken ct);
}