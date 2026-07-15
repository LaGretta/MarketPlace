using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Domain.Entities;
using MarketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Infrastructure.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;
    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<Category>> GetCategories(CancellationToken ct)
    {
        return await _dbContext.Categories.ToListAsync(ct);
    }
    public async Task<Category?> GetCategoryById(int id, CancellationToken ct)
    {
        var  category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        return category;
    }
    public async Task CreateCategory(Category category, CancellationToken ct)
    {
         await  _dbContext.Categories.AddAsync(category, ct);
    }
    public void UpdateCategory(Category category)
    {
        _dbContext.Categories.Update(category);
    }
    public void DeleteCategory(Category category)
    {
         _dbContext.Categories.Remove(category);
    }
}