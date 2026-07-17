using AutoMapper;
using FluentAssertions;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Service;
using MarketPlace.Domain.Entities;
using Moq;

namespace MarketPlace.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository>  _categoryRepositoryMock;
    private readonly Mock<IMapper>  _mapperMock;
    private readonly Mock<IUnitOfWork>  _unitOfWorkMock;
    private readonly CategoryService _categoryService;
    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryService = new CategoryService(_categoryRepositoryMock.Object
            ,_mapperMock.Object
            , _unitOfWorkMock.Object);
    }
    [Fact]
    public async Task GetCategories_WhenNotFound_KeyNotFoundException()
    {
        _categoryRepositoryMock.Setup(n => n.GetCategoryById(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync((Category)null!);
        
        Func<Task> act = async () => await _categoryService.GetCategoryById(1,CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task GetCategoryById_WhenCategoryExists_ReturnsCategory()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetCategoryById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category());
        
        _mapperMock
            .Setup(m => m.Map<CategoryResponseDto>(It.IsAny<Category>()))
            .Returns(new CategoryResponseDto());

        var result = await _categoryService.GetCategoryById(1, CancellationToken.None);
        result.Should().NotBeNull();
    }
[Fact]
    public async Task CreateCategory_WhenValidData_CreatesAndSaves()
    {
        _mapperMock
            .Setup(m => m.Map<Category>(It.IsAny<CreateCategoryDto>()))
            .Returns(new Category());

        _mapperMock
            .Setup(m => m.Map<CategoryResponseDto>(It.IsAny<Category>()))
            .Returns(new CategoryResponseDto());

        var dto = new CreateCategoryDto();

        var result = await _categoryService.CreateCategory(dto, CancellationToken.None);

        result.Should().NotBeNull();
        _categoryRepositoryMock.Verify(r =>
            r.CreateCategory(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u =>
            u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task UpdateCategory_WhenNotFound_ThrowsKeyNotFound()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetCategoryById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);

        Func<Task> act = async () =>
            await _categoryService.UpdateCategory(new UpdateCategoryDto(), 1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task UpdateCategory_WhenValidData_UpdatesAndSaves()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetCategoryById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category());

        await _categoryService.UpdateCategory(new UpdateCategoryDto(), 1, CancellationToken.None);

        _categoryRepositoryMock.Verify(r =>
            r.UpdateCategory(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u =>
            u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task DeleteCategoryById_WhenNotFound_ThrowsKeyNotFound()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetCategoryById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);

        Func<Task> act = async () =>
            await _categoryService.DeleteCategoryById(1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    [Fact]
    public async Task DeleteCategoryById_WhenValidData_DeletesAndSaves()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetCategoryById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category());

        await _categoryService.DeleteCategoryById(1, CancellationToken.None);

        _categoryRepositoryMock.Verify(r =>
            r.DeleteCategory(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u =>
            u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}