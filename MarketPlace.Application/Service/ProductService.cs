using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Interfaces.Service;
using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Service;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;


    public ProductService(IProductRepository productRepository
        , IMapper mapper
        , IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponseDto<ProductResponseDto>> SearchProducts
    (string? search, int? categoryId, decimal? minPrice, decimal? 
        maxPrice, string? sortBy, int page, int pageSize, CancellationToken ct)
    {
        var paged = await _productRepository.SearchProducts
            (search, categoryId, minPrice, maxPrice, sortBy, page, pageSize, ct);
        
        var map =  _mapper.Map<List<ProductResponseDto>>(paged.Items);

        return new PagedResponseDto<ProductResponseDto>
        {
            Items = map,
            Page = page,
            PageSize = pageSize,
            TotalCount = paged.TotalCount,
        };
    }

    public async Task<ProductResponseDto> GetProductById(int id, CancellationToken ct)
    {
        var find = await _productRepository.GetProductById(id, ct);
        if(find == null)
            throw new KeyNotFoundException("Product not found");
        return _mapper.Map<ProductResponseDto>(find);
    }

    public async Task<List<ProductResponseDto>> GetMyProducts(int userId, CancellationToken ct)
    {
        var myproduct = await _productRepository.GetMyProducts(userId, ct);
        return _mapper.Map<List<ProductResponseDto>>(myproduct);
    }

    public async Task<ProductResponseDto> CreateProduct(CreateProductDto dto, int userId, CancellationToken ct)
    {
        var product = _mapper.Map<Product>(dto);
        product.SellerId = userId;
        product.IsActive = true;
        product.CreatedAt = DateTime.UtcNow;
        
        await _productRepository.CreateProduct(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task UpdateProduct(UpdateProductDto dto, int id, int userId, CancellationToken ct)
    {
        var  find = await _productRepository.GetMyProductById(userId,id, ct);
        if (find == null)
            throw new KeyNotFoundException("Product not found");
        _mapper.Map(dto, find);
        _productRepository.UpdateProduct(find);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteProduct(int id, int userId, CancellationToken ct)
    {
        var find = await _productRepository.GetMyProductById(userId,id, ct);
        if (find == null)
            throw new KeyNotFoundException("Product not found");
        _productRepository.DeleteProduct(find);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}