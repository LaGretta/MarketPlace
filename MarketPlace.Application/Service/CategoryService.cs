using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Interfaces.Service;
using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Service;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository
        , IMapper mapper
        , IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryResponseDto>> GetCategories(CancellationToken ct)
    {
        var  categories = await _categoryRepository.GetCategories(ct);
        return _mapper.Map<List<CategoryResponseDto>>(categories);
    }

    public async Task<CategoryResponseDto> GetCategoryById(int id, CancellationToken ct)
    {
        var  category = await _categoryRepository.GetCategoryById(id, ct);
        if(category == null)
            throw new KeyNotFoundException("Category not found");
        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> CreateCategory(CreateCategoryDto dto, CancellationToken ct)
    {
        var map = _mapper.Map<Category>(dto);
        await _categoryRepository.CreateCategory(map, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return _mapper.Map<CategoryResponseDto>(map);
    }

    public async Task UpdateCategory(UpdateCategoryDto dto, int id, CancellationToken ct)
    {
        var category = await _categoryRepository.GetCategoryById(id, ct);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        _mapper.Map(dto, category);                   
        _categoryRepository.UpdateCategory(category);  
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteCategoryById(int id, CancellationToken ct)
    {
        var category = await _categoryRepository.GetCategoryById(id, ct);
        if(category == null)
            throw new KeyNotFoundException("Category not found");
         _categoryRepository.DeleteCategory(category);
         await _unitOfWork.SaveChangesAsync(ct);
    }
}