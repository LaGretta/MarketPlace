using AutoMapper;
using FluentAssertions;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Service;
using MarketPlace.Domain.Entities;
using MarketPlace.Domain.Enums;
using Moq;

namespace MarketPlace.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository;
    private readonly Mock<IProductRepository> _productRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _productRepository = new Mock<IProductRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();

        _sut = new OrderService(
            _orderRepository.Object,  
            _mapper.Object,        
            _unitOfWork.Object,          
            _productRepository.Object); 
    }
    [Fact]
    public async Task CreateOrder_WhenProductNotFound_ThrowsKeyNotFound()
    {
        _productRepository
            .Setup(r => r.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>()));
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };
        Func<Task> act = async () => await _sut.CreateOrder(dto,userId:1,CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task CreateOrder_WhenNotEnoughStock_ThrowsInvalidOperation()
    {
        _productRepository.Setup(n => n.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Product
            {
                Id = 1,
                IsActive =  true,
                Stock = 3,
                Price = 100
            }
        );
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = 1,
                    Quantity = 5
                }
            }
        };
        Func<Task> act = async () => await _sut.CreateOrder(dto,userId:1,CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateOrder_WhenNotIsActive_InvalidOperationException()
    {
        _productRepository.Setup(n => n.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Product
            {
                Id = 1,
                IsActive = false,
                Stock = 3,
                Price = 100
            });
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };
        Func<Task> act = async () => await _sut.CreateOrder(dto,userId:1,CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateOrder_WhenValidData_CreatesOrderAndCommits()
    {
        _productRepository.Setup(n => n.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = 1,
                IsActive = true,
                Stock = 10,
                Price = 100
            });
        _mapper.Setup(n => n.Map<OrderResponseDto>(It.IsAny<Order>()))
            .Returns(new OrderResponseDto());
            
                var dto = new CreateOrderDto
                {
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductId = 1, Quantity = 2 }
                    }
                };
            
       var result = await _sut.CreateOrder(dto,userId:1,CancellationToken.None);
       result.Should().NotBeNull();
       
       _unitOfWork.Verify(n => n.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
       _orderRepository.Verify(n => n.GetOrderById(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderNotFound_ThrowsKeyNotFound()
    {
      _orderRepository
          .Setup(n => n.GetMyOrderById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((Order?)null);
        
      Func<Task> act = async () => await _sut.CancelOrder(id: 1, userId: 1, CancellationToken.None);

      await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task CancelOrder_WhenStatusIsShippedOrDelivered_ThrowsInvalidOperation()
    {
        _orderRepository
            .Setup(n => n.GetMyOrderById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                Id = 1,
                Status = OrderStatus.Shipped
            });
        Func<Task> act = async () => await _sut.CancelOrder(id: 1, userId: 1, CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelOrder_WhenValidPendingOrder_CancelsAndReturnsStock()
    {
        _orderRepository
            .Setup(n => n.GetMyOrderById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                Id = 1,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            });
        
        _mapper.Setup(n => n.Map<OrderResponseDto>(It.IsAny<Order>()))
            .Returns(new OrderResponseDto());
        
        var result = await _sut.CancelOrder(id:1,userId:1,CancellationToken.None);
        result.Should().NotBeNull();
        
        _unitOfWork.Verify(n => n.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetOrderById_WhenOrderNotFound_ThrowsKeyNotFound()
    {
        _orderRepository.Setup(n => n.GetMyOrderById(It.IsAny<int>(),It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        
        Func<Task> act = async () => await _sut.GetOrderById(id: 1, userId: 1, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task ChangeOrderStatus_WhenOrderNotFound_ThrowsKeyNotFound()
    {
        _orderRepository
            .Setup(n => n.GetOrderById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);
        
        Func<Task> act = async () => await _sut.GetOrderById(id:1,userId:1,CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }












}