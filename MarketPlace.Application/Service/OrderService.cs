using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Interfaces.Service;
using MarketPlace.Domain.Entities;
using MarketPlace.Domain.Enums;

namespace MarketPlace.Application.Service;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IOrderRepository orderRepository
        , IMapper mapper
        , IUnitOfWork unitOfWork
        , IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
    }

    public async Task<OrderResponseDto> CreateOrder(CreateOrderDto dto, int userId, CancellationToken ct)
    {
       if(dto.Items == null || dto.Items.Count == 0)
           throw new ArgumentException("Order must contain at least one item");
       await _unitOfWork.BeginTransactionAsync(ct);
       try
       {
           var order = new Order
           {
               BuyerId = userId,
               Status = OrderStatus.Pending,
               CreatedAt = DateTime.UtcNow,
               OrderItems = new List<OrderItem>()
           };
           decimal totalPrice = 0;
           foreach (var item in dto.Items)
           {
               var product = await _productRepository.GetProductById(item.ProductId, ct);

               if (product == null)
                   throw new KeyNotFoundException($"Product {item.ProductId} not found");
               if (!product.IsActive)
                   throw new InvalidOperationException($"Product {product.Title} is not available");
               if (product.Stock < item.Quantity)
                   throw new InvalidOperationException($"Not enough stock for {product.Title}");

               product.Stock -= item.Quantity;

               var orderItem = new OrderItem
               {
                   ProductId = item.ProductId,
                   Quantity = item.Quantity,
                   UnitPrice = product.Price,
               };
               order.OrderItems.Add(orderItem);
               totalPrice += product.Price * item.Quantity;
           }

           order.TotalPrice = totalPrice;
           await _orderRepository.AddOrder(order, ct);
           await _unitOfWork.SaveChangesAsync(ct);
           await _unitOfWork.CommitTransactionAsync(ct);
           return _mapper.Map<OrderResponseDto>(order);
       }
       catch
       {
           await _unitOfWork.RollbackTransactionAsync(ct);
           throw;
       }
    }

    public async Task<List<OrderResponseDto>> GetMyOrders(int userId, CancellationToken ct)
    {
         var find = await  _orderRepository.GetMyOrders(userId, ct);
         return _mapper.Map<List<OrderResponseDto>>(find);
    }

    public async Task<OrderResponseDto> GetOrderById(int id, int userId, CancellationToken ct)
    {
         var find = await _orderRepository.GetMyOrderById(id, userId, ct);
         if (find == null)
             throw new KeyNotFoundException("Order not found");
         return _mapper.Map<OrderResponseDto>(find);
    }

    public async Task<OrderResponseDto> CancelOrder(int id, int userId, CancellationToken ct)
    {
        var findorder = await _orderRepository.GetMyOrderById(id, userId, ct);
        if(findorder == null)
            throw new KeyNotFoundException("Order not found");
        if (findorder.Status != OrderStatus.Pending && findorder.Status != OrderStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a shipped or delivered order");
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var item in findorder.OrderItems)
            {
                var product = await _productRepository.GetProductById(item.ProductId, ct);
                if (product != null)
                    product.Stock += item.Quantity;
            }

            findorder.Status = OrderStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return _mapper.Map<OrderResponseDto>(findorder);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<PagedResponseDto<OrderResponseDto>> GetAllOrders(int page, int pageSize, CancellationToken ct)
    {
        var paged = await _orderRepository.GetAllOrders(page, pageSize, ct);
        var map =  _mapper.Map<List<OrderResponseDto>>(paged.Items);

        return new PagedResponseDto<OrderResponseDto>
        {
            Items = map,
            Page = page,
            PageSize = pageSize,
            TotalCount = paged.TotalCount,
        };
    }

    public async Task ChangeOrderStatus(int id, OrderStatus newStatus, CancellationToken ct)
    {
        var findorder = await _orderRepository.GetOrderById(id,ct);
        if (findorder == null)
            throw new KeyNotFoundException("Order not found");
        findorder.Status = newStatus;
        await _unitOfWork.SaveChangesAsync(ct);
    }
}