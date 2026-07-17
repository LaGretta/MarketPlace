using AutoMapper;
using FluentAssertions;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Service;
using MarketPlace.Domain.Entities;
using Moq;

namespace MarketPlace.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepository = new Mock<IProductRepository>();
        _mapper = new Mock<IMapper>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _sut = new ProductService(
            _productRepository.Object,
            _mapper.Object,
            _unitOfWork.Object);
    }
    
    [Fact]
    public async Task GetProductById_WhenNotFound_ThrowsKeyNotFound()
    {
        _productRepository
            .Setup(r => r.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _sut.GetProductById(1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task GetProductById_WhenExists_ReturnsProduct()
    {
        // товар є
        _productRepository
            .Setup(r => r.GetProductById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = 1, Title = "Test" });

        _mapper
            .Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns(new ProductResponseDto());

        var result = await _sut.GetProductById(1, CancellationToken.None);

        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateProduct_WhenValidData_CreatesProduct()
    {
        var dto = new CreateProductDto
        {
            Title = "Test",
            Description = "Desc",
            Price = 100,
            Stock = 10,
            CategoryId = 1
        };

        _mapper
            .Setup(m => m.Map<Product>(It.IsAny<CreateProductDto>()))
            .Returns(new Product { Id = 1 });

        _mapper
            .Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns(new ProductResponseDto());

        var result = await _sut.CreateProduct(dto, userId: 5, CancellationToken.None);

        result.Should().NotBeNull();
        _productRepository.Verify(r => r.CreateProduct(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateProduct_WhenNotFoundOrNotOwner_ThrowsKeyNotFound()
    {

        _productRepository
            .Setup(r => r.GetMyProductById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var dto = new UpdateProductDto { Title = "New", Price = 200, Stock = 5 };

        Func<Task> act = async () => await _sut.UpdateProduct(dto, id: 1, userId: 5, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task UpdateProduct_WhenValidOwner_UpdatesProduct()
    {
        _productRepository
            .Setup(r => r.GetMyProductById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = 1, SellerId = 5 });

        var dto = new UpdateProductDto { Title = "New", Price = 200, Stock = 5 };

        await _sut.UpdateProduct(dto, id: 1, userId: 5, CancellationToken.None);

        _productRepository.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task DeleteProduct_WhenNotFoundOrNotOwner_ThrowsKeyNotFound()
    {
        _productRepository
            .Setup(r => r.GetMyProductById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _sut.DeleteProduct(id: 1, userId: 5, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}