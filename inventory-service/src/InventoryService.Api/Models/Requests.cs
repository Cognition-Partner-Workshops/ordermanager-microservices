using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.Models;

public record CreateInventoryItemRequest(
    [Range(1, int.MaxValue)] int ProductId,
    [Required, MaxLength(200)] string ProductName,
    [Required, MaxLength(50)] string ProductSku,
    [Range(0, int.MaxValue)] int QuantityOnHand,
    [Range(0, int.MaxValue)] int ReorderLevel,
    [MaxLength(50)] string WarehouseLocation);

public record UpdateInventoryItemRequest(
    [MaxLength(200)] string? ProductName,
    [MaxLength(50)] string? ProductSku,
    [Range(0, int.MaxValue)] int? ReorderLevel,
    [MaxLength(50)] string? WarehouseLocation);

public record QuantityRequest([Range(1, int.MaxValue)] int Quantity);

public record StockCheckLine([Range(1, int.MaxValue)] int ProductId, [Range(1, int.MaxValue)] int Quantity);

public record StockCheckRequest([Required, MinLength(1)] List<StockCheckLine> Items);

public record StockCheckResult(int ProductId, int Requested, int Available, bool Sufficient);

public record StockCheckResponse(bool AllAvailable, List<StockCheckResult> Items);
