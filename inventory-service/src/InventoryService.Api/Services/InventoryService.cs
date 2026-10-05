using InventoryService.Api.Data;
using InventoryService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Services;

public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(InventoryDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<List<InventoryItem>> GetAllAsync() =>
        _context.InventoryItems.OrderBy(i => i.ProductId).ToListAsync();

    public Task<InventoryItem?> GetByIdAsync(int id) =>
        _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == id);

    public Task<InventoryItem?> GetByProductIdAsync(int productId) =>
        _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);

    public Task<List<InventoryItem>> GetLowStockAsync() =>
        _context.InventoryItems
            .Where(i => i.QuantityOnHand <= i.ReorderLevel)
            .OrderBy(i => i.QuantityOnHand)
            .ToListAsync();

    public async Task<InventoryItem> CreateAsync(CreateInventoryItemRequest request)
    {
        if (await _context.InventoryItems.AnyAsync(i => i.ProductId == request.ProductId))
            throw new DuplicateInventoryException(request.ProductId);

        var item = new InventoryItem
        {
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            ProductSku = request.ProductSku,
            QuantityOnHand = request.QuantityOnHand,
            ReorderLevel = request.ReorderLevel,
            WarehouseLocation = request.WarehouseLocation,
            LastRestocked = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(int productId, UpdateInventoryItemRequest request)
    {
        var item = await RequireByProductIdAsync(productId);
        if (request.ProductName is not null) item.ProductName = request.ProductName;
        if (request.ProductSku is not null) item.ProductSku = request.ProductSku;
        if (request.ReorderLevel is not null) item.ReorderLevel = request.ReorderLevel.Value;
        if (request.WarehouseLocation is not null) item.WarehouseLocation = request.WarehouseLocation;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> RestockAsync(int productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");

        var item = await RequireByProductIdAsync(productId);
        item.QuantityOnHand += quantity;
        item.LastRestocked = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Restocked product {ProductId} by {Quantity}; on hand {OnHand}", productId, quantity, item.QuantityOnHand);
        return item;
    }

    public async Task<InventoryItem> DeductAsync(int productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");

        var item = await RequireByProductIdAsync(productId);
        if (item.QuantityOnHand < quantity)
            throw new InsufficientStockException(productId, quantity, item.QuantityOnHand);

        item.QuantityOnHand -= quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deducted {Quantity} from product {ProductId}; on hand {OnHand}", quantity, productId, item.QuantityOnHand);
        return item;
    }

    public async Task<StockCheckResponse> CheckAvailabilityAsync(StockCheckRequest request)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var onHand = await _context.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToDictionaryAsync(i => i.ProductId, i => i.QuantityOnHand);

        var results = request.Items.Select(line =>
        {
            var available = onHand.GetValueOrDefault(line.ProductId, 0);
            return new StockCheckResult(line.ProductId, line.Quantity, available, available >= line.Quantity);
        }).ToList();

        return new StockCheckResponse(results.All(r => r.Sufficient), results);
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        var item = await GetByProductIdAsync(productId);
        if (item is null) return false;
        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<InventoryItem> RequireByProductIdAsync(int productId) =>
        await GetByProductIdAsync(productId) ?? throw new InventoryNotFoundException(productId);
}
