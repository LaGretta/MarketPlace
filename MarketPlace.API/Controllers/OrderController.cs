using System.Security.Claims;
using MarketPlace.API.Auth;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces.Service;
using MarketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.API.Controllers;

[Authorize]   
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    private int GetUserId()
        => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    
    [HttpPost]
    [Authorize(Roles = Roles.Buyer)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _orderService.CreateOrder(dto, userId, ct);
        return Ok(result);
    }
    [HttpGet("my")]
    [Authorize(Roles = Roles.Buyer)]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _orderService.GetMyOrders(userId, ct);
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Buyer)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrderById(id, userId, ct);
        return Ok(result);
    }
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = Roles.Buyer)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrderById(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _orderService.CancelOrder(id, userId, ct);
        return Ok(result);
    }
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(PagedResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _orderService.GetAllOrders(page, pageSize, ct);
        return Ok(result);
    }
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] OrderStatus newStatus,
        CancellationToken ct)
    {
        await _orderService.ChangeOrderStatus(id, newStatus, ct);
        return NoContent();
    }
}