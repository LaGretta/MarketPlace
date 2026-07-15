using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Interfaces.Repository;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> SearchProducts(
        string? search, int? categoryId, decimal? minPrice, decimal? maxPrice,
        string? sortBy, int page, int pageSize, CancellationToken ct);
    
    Task<Product?> GetProductById(int productId , CancellationToken ct);
    Task<Product?> GetMyProductById(int userId,int productId , CancellationToken ct);
    Task<List<Product>> GetMyProducts(int userId , CancellationToken ct);
    Task CreateProduct(Product product , CancellationToken ct);
    void UpdateProduct(Product product);
    void DeleteProduct(Product product);
}