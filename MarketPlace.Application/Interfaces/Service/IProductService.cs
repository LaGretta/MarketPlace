using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Interfaces.Service;

public interface IProductService
{
    Task<PagedResponseDto<ProductResponseDto>> SearchProducts(
        string? search, int? categoryId, decimal? minPrice, decimal? maxPrice,
        string? sortBy, int page, int pageSize, CancellationToken ct);

    Task<ProductResponseDto> GetProductById(int id, CancellationToken ct);
    Task<List<ProductResponseDto>> GetMyProducts(int userId, CancellationToken ct);
    Task<ProductResponseDto> CreateProduct(CreateProductDto dto, int userId, CancellationToken ct);
    Task UpdateProduct(UpdateProductDto dto, int id, int userId, CancellationToken ct);
    Task DeleteProduct(int id, int userId, CancellationToken ct);
}