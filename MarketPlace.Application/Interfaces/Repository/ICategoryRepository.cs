using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Interfaces.Repository;

public interface ICategoryRepository
{
   Task<List<Category>> GetCategories(CancellationToken ct);  
   Task<Category?> GetCategoryById(int id , CancellationToken ct);
   Task CreateCategory(Category category , CancellationToken ct);
   void UpdateCategory(Category category);
   void DeleteCategory(Category category);
}