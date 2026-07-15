using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Domain.Entities;
using MarketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;
    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchProducts(
        string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, 
        string? sortBy, int page, int pageSize, CancellationToken ct)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);              

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        query = sortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "title"      => query.OrderBy(p => p.Title),
            _            => query.OrderByDescending(p => p.CreatedAt)  
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
    public async Task<Product?> GetProductById(int productId, CancellationToken ct)
    {
         var find = await _dbContext.Products
             .AsNoTracking().FirstOrDefaultAsync(n => n.Id == productId, ct);
         return find;
    }
    public async Task<Product?> GetMyProductById(int userId, int productId, CancellationToken ct)
    {
         var find = await _dbContext.Products.FirstOrDefaultAsync(n => n.SellerId == userId && n.Id == productId, ct);
         return find;
    }
    public async Task<List<Product>> GetMyProducts(int userId, CancellationToken ct)
    {
        var find = await _dbContext.Products.Where(n => n.SellerId == userId).ToListAsync(ct);
        return find;
    }
    public async Task CreateProduct(Product product, CancellationToken ct)
    {
      await _dbContext.Products.AddAsync(product);
    }
    public void UpdateProduct(Product product)
    {
        _dbContext.Products.Update(product);
    }
    public void DeleteProduct(Product product)
    {
        _dbContext.Products.Remove(product);
    }
}