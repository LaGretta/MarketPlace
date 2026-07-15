using MarketPlace.Application.DTOs;
using MarketPlace.Domain.Enums;

namespace MarketPlace.Application.Interfaces.Service;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrder(CreateOrderDto dto, int userId, CancellationToken ct);
    Task<List<OrderResponseDto>> GetMyOrders(int userId, CancellationToken ct);
    Task<OrderResponseDto> GetOrderById(int id, int userId, CancellationToken ct);
    Task<OrderResponseDto> CancelOrder(int id, int userId, CancellationToken ct);
    Task<PagedResponseDto<OrderResponseDto>> GetAllOrders(int page, int pageSize, CancellationToken ct);
    Task ChangeOrderStatus(int id, OrderStatus newStatus, CancellationToken ct);
}
