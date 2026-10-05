using InventoryService.Api.Models;
using InventoryService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<InventoryItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await _inventoryService.GetAllAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _inventoryService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(List<InventoryItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock() => Ok(await _inventoryService.GetLowStockAsync());

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var item = await _inventoryService.GetByProductIdAsync(productId);
        return item is null ? NotFound(Problem404(productId)) : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest request)
    {
        try
        {
            var created = await _inventoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetByProduct), new { productId = created.ProductId }, created);
        }
        catch (DuplicateInventoryException ex)
        {
            return Conflict(new ProblemDetails { Title = "Duplicate inventory record", Detail = ex.Message, Status = 409 });
        }
    }

    [HttpPut("product/{productId:int}")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateInventoryItemRequest request)
    {
        try
        {
            return Ok(await _inventoryService.UpdateAsync(productId, request));
        }
        catch (InventoryNotFoundException)
        {
            return NotFound(Problem404(productId));
        }
    }

    [HttpPost("product/{productId:int}/restock")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restock(int productId, [FromBody] QuantityRequest request)
    {
        try
        {
            return Ok(await _inventoryService.RestockAsync(productId, request.Quantity));
        }
        catch (InventoryNotFoundException)
        {
            return NotFound(Problem404(productId));
        }
    }

    [HttpPost("product/{productId:int}/deduct")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deduct(int productId, [FromBody] QuantityRequest request)
    {
        try
        {
            return Ok(await _inventoryService.DeductAsync(productId, request.Quantity));
        }
        catch (InventoryNotFoundException)
        {
            return NotFound(Problem404(productId));
        }
        catch (InsufficientStockException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Insufficient stock",
                Detail = ex.Message,
                Status = 409,
                Extensions = { ["productId"] = ex.ProductId, ["requested"] = ex.Requested, ["available"] = ex.Available }
            });
        }
    }

    [HttpPost("check")]
    [ProducesResponseType(typeof(StockCheckResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Check([FromBody] StockCheckRequest request) =>
        Ok(await _inventoryService.CheckAvailabilityAsync(request));

    [HttpDelete("product/{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int productId) =>
        await _inventoryService.DeleteAsync(productId) ? NoContent() : NotFound(Problem404(productId));

    private static ProblemDetails Problem404(int productId) => new()
    {
        Title = "Inventory record not found",
        Detail = $"No inventory record for product {productId}",
        Status = 404
    };
}
