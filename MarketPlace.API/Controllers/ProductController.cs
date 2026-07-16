using System.Security.Claims;
using MarketPlace.API.Auth;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly  IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    private int GetUserId()
        => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _productService.SearchProducts(
            search, categoryId, minPrice, maxPrice, sortBy, page, pageSize, ct);
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var get = await _productService.GetProductById(id, ct);
        return Ok(get);
    }
    [HttpGet("my")]
    [Authorize(Roles = Roles.Seller)]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var userId = GetUserId();
        var getmyProduct = await _productService.GetMyProducts(userId, ct);
        return Ok(getmyProduct);
    }
    [HttpPost]
    [Authorize(Roles = Roles.Seller)]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productService.CreateProduct(dto, userId, ct);
        return Ok(result);
    }
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Seller)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        await _productService.UpdateProduct(dto, id, userId, ct);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Seller)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _productService.DeleteProduct(id, userId, ct);
        return NoContent();
    }
}