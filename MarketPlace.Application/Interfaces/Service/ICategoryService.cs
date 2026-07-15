using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Interfaces.Service;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetCategories(CancellationToken ct);  
    Task<CategoryResponseDto> GetCategoryById(int id , CancellationToken ct);
    Task<CategoryResponseDto>  CreateCategory(CreateCategoryDto dto,CancellationToken ct);
    Task UpdateCategory(UpdateCategoryDto dto, CancellationToken ct);
    Task DeleteCategoryById(int id, CancellationToken ct);
}